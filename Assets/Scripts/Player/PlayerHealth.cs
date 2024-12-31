using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using NetworkConnection = FishNet.Connection.NetworkConnection;

public class PlayerHealth : NetworkBehaviour
{
    public static Dictionary<int, PlayerHealth> Players = new Dictionary<int, PlayerHealth>();
    
    [SerializeField] private int maxHealth = 100;

    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Players.Add(OwnerId, this);

        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        UIManager.SetHealthText(maxHealth.ToString());
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Players.Remove(OwnerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetHealth()
    {
        _currentHealth = maxHealth;
        LocalSetHealth(Owner, _currentHealth);
    }
    
    public void TakeDamage(int damage, int attackerID)
    {
        TakeDamageServer(damage, attackerID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServer(int damage, int attackerID)
    {
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die(attackerID);
        }
        
        LocalSetHealth(Owner, _currentHealth);
    }
    
    private void Die(int attackerID)
    {
        //Debug.Log("Player is dead");
        PlayerController.TogglePlayer(OwnerId, false);
        PlayerManager.PlayerDied(OwnerId,attackerID);
    }

    [TargetRpc]
    private void LocalSetHealth(NetworkConnection connection, int newHealth)
    {
        UIManager.SetHealthText(newHealth.ToString());
    }
}
