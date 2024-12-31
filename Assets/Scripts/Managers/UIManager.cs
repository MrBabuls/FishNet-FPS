using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    private static UIManager instance;

    [SerializeField] private GameObject scoreboard;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private PlayerCard playerCardPrefab;
    [SerializeField] private Transform playerCardParent;

    private Dictionary<int, PlayerCard> _playerCards = new Dictionary<int, PlayerCard>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        
        scoreboard.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            scoreboard.SetActive(true);
        
        if (Input.GetKeyUp(KeyCode.Tab))
            scoreboard.SetActive(false);
    }

    public static void PlayerJoined(int clientID)
    {
        PlayerCard newPlayerCard = Instantiate(instance.playerCardPrefab, instance.playerCardParent);
        instance._playerCards.Add(clientID, newPlayerCard);
        newPlayerCard.Initialize(clientID.ToString());
    }
    
    public static void PlayerLeft(int clientID)
    {
        if (instance._playerCards.TryGetValue(clientID, out PlayerCard playerCard))
        {
            Destroy(playerCard.gameObject);
            instance._playerCards.Remove(clientID);
        }
    }
    
    public static void SetHealthText(string health)
    {
        instance.healthText.text = health;
    }

    public static void SetKills(int clientID, int kills)
    {
        instance.SetKillsServer(clientID, kills);
    }

    [Server]
    private void SetKillsServer(int clientID, int kills)
    {
        SetKillsObserver(clientID, kills);
    }

    [ObserversRpc]
    private void SetKillsObserver(int clientID, int kills)
    {
        instance._playerCards[clientID].SetKills(kills);
    }
    
    public static void SetDeaths(int clientID, int deaths)
    {
        instance.SetDeathsServer(clientID, deaths);
    }

    [Server]
    private void SetDeathsServer(int clientID, int deaths)
    {
        SetDeathsObserver(clientID, deaths);
    }

    [ObserversRpc]
    private void SetDeathsObserver(int clientID, int deaths)
    {
        instance._playerCards[clientID].SetDeaths(deaths);
    }
}
