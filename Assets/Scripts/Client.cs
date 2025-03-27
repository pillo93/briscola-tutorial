using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public static Client Local;
    public bool isTurnActive { get; private set; }
    public bool isOwned { get; private set; }
    public ulong clientId { get; private set; }
    [SerializeField] private GameObject cardPrefab;
    protected GameManager gm;
    protected Transform handContainer;
    public Transform playedCardContainer { get; private set; }
    private TMP_Text scoreText;
    private CardUi lastPlayedCard;

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
        var player = gm.GetPlayer(clientId);
        player.OnCardDealt = OnCardDealtClientRpc;
        player.OnCardPlayed = OnCardPlayedClientRpc;
        player.OnScoreUpdate = OnClientScoreUpdateClientRpc;
    }

    [ClientRpc]
    protected virtual void OnClientTurnChangeClientRpc(ulong activeClientId)
    {
        isTurnActive = activeClientId == clientId;
    }

    [ClientRpc]
    private void OnClientScoreUpdateClientRpc(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
        lastPlayedCard.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void OnCardDealtClientRpc(int i, Card c)
    {
        Debug.Log($"Client{clientId} was dealt {c} at index {i}");
        var go = Instantiate(cardPrefab, handContainer.GetChild(i));
        var cui = go.GetComponent<CardUi>();
        cui.SetCard(c, isOwned);
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
        lastPlayedCard = child.GetComponent<CardUi>();
        lastPlayedCard.SetCard(c, true);
        lastPlayedCard.enabled = false;
        child.SetParent(playedCardContainer);
        child.localPosition = Vector3.zero;
    }
}