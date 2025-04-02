using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public static Client Local;
    public static Client Enemy;
    public bool isTurnActive { get; private set; }
    public bool isOwned { get; private set; }
    public ulong clientId { get; private set; }
    [SerializeField] private GameObject cardPrefab;
    protected GameManager gm;
    protected Transform handContainer;
    public CardUi lastPlayedCard;
    public Transform playedCardContainer { get; private set; }
    private TMP_Text scoreText;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client spawned on network with id {OwnerClientId}");
        Setup(IsOwner, OwnerClientId);
    }

    protected void Setup(bool isOwner, ulong clientId)
    {
        string goPrefix = isOwner ? "Player" : "Enemy";
        isOwned = isOwner;
        this.clientId = clientId;
        if (isOwned)
        {
            Local = this;
            GameObject.Find("NetworkUI").SetActive(false);
            ClientConnectServerRpc(clientId);
        }
        else
        {
            Enemy = this;
        }
        handContainer = GameObject.Find(goPrefix + "HandTransform").transform;
        playedCardContainer = GameObject.Find(goPrefix + "CardSpot").transform;
        scoreText = GameObject.Find(goPrefix + "Score").GetComponent<TMP_Text>();
    }

    [ServerRpc]
    protected void ClientConnectServerRpc(ulong clientId)
    {
        gm = FindObjectOfType<GameManager>();
        gm.ClientConnect(clientId);
        gm.OnActiveClientChange += OnClientTurnChangeClientRpc;
        gm.OnPlayerScore += OnClientScoreUpdateClientRpc;
        var player = gm.GetPlayer(clientId);
        player.OnCardDealt = OnCardDealtClientRpc;
        player.OnCardPlayed = OnCardPlayedClientRpc;
    }

    [ClientRpc]
    protected virtual void OnClientTurnChangeClientRpc(ulong activeClientId)
    {
        isTurnActive = activeClientId == clientId;
        if(isTurnActive) lastPlayedCard = null;
    }

    [ClientRpc]
    protected virtual void OnClientScoreUpdateClientRpc(ulong clientId, int newScore)
    {
        if (!IsOwner) return;
        var scoreText = clientId == OwnerClientId ? Local.scoreText : Enemy.scoreText;
        new ScoreCommand(
            scoreText,
            newScore
            ).AddToQueue();
    }

    [ClientRpc]
    private void OnCardDealtClientRpc(int i, Card c)
    {
        new DealCardCommand(cardPrefab, handContainer.GetChild(i), c, isOwned).AddToQueue();
    }

    [ServerRpc]
    public void PlayCardServerRpc(Card card)
    {
        gm.PlayCard(clientId, card);
    }

    [ClientRpc]
    private void OnCardPlayedClientRpc(int i, Card c)
    {
        var child = handContainer.GetChild(i).GetChild(0);
        new PlayCardCommand(c, child.GetComponent<CardUi>(), playedCardContainer).AddToQueue();
    }
}