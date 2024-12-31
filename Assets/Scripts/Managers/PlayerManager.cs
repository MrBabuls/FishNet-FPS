using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    
    private static PlayerManager instance;
    
    private Dictionary<int, Player> _players = new Dictionary<int, Player>();
    private List<int> _deadPlayers = new List<int>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        if (!IsServerInitialized)
            return;
            
        for (int i = 0; i < _deadPlayers.Count; i++)
        {
            if (_players[_deadPlayers[i]].deathTime < Time.time - respawnTime)
            {
                RespawnPlayer(_deadPlayers[i]);
                _deadPlayers.RemoveAt(i);
                return;
            }
        }
    }

    private void RespawnPlayer(int clientID)
    {
        PlayerController.SetPlayerPosition(clientID, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].position);
        PlayerController.TogglePlayer(clientID, true);
        if (PlayerHealth.Players.TryGetValue(clientID, out PlayerHealth playerHealth))
            playerHealth.ResetHealth();
    }

    public static void InitializeNewPlayer(int clientID)
    {
        instance._players.Add(clientID,new Player());
    }

    public static void PlayerDisconnected(int clientID)
    {
        instance._players.Remove(clientID);
    }

    public static void PlayerDied(int player, int killer)
    {
        if (instance._players.TryGetValue(killer, out Player killerPlayer))
            killerPlayer.Score++;

        if (instance._players.TryGetValue(killer, out Player deadPlayer))
        {
            deadPlayer.Deaths++;
            deadPlayer.deathTime = Time.time;
        }
        
        UIManager.SetKills(killer, killerPlayer.Score);
        UIManager.SetDeaths(player, deadPlayer.Deaths);
        instance._deadPlayers.Add(player);
    }
    
    class Player
    {
        public int Score = 0;
        public int Deaths = 0;

        public float deathTime = -99;
    }
}
