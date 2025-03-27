using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public static Client Local;
    public bool isTurnActive { get; private set; }
    [SerializeField] private GameObject cardPrefab;
    private GameManager gm;
    private Transform handContainer;
    private Transform playedCardContainer;
    private TMP_Text scoreText;
    private CardUi lastPlayedCard;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client spawned on network with id {OwnerClientId}");
        string goPrefix = IsOwner ? "Player" : "Enemy";
        if (IsOwner)
        {
            Local = this;
            GameObject.Find("NetworkUI").SetActive(false);
            Invoke(nameof(ClientConnectServerRpc), 1f);
        }
        handContainer = GameObject.Find(goPrefix + "HandTransform").transform;
        playedCardContainer = GameObject.Find(goPrefix + "CardSpot").transform;
        scoreText = GameObject.Find(goPrefix + "Score").GetComponent<TMP_Text>();
    }

    [ServerRpc]
    private void ClientConnectServerRpc()
    {
        gm = FindObjectOfType<GameManager>();
        gm.ClientConnect(OwnerClientId);
        gm.OnActiveClientChange += OnClientTurnChangeClientRpc;
        var player = gm.GetPlayer(OwnerClientId);
        player.OnCardDealt = OnCardDealtClientRpc;
        player.OnCardPlayed = OnCardPlayedClientRpc;
        player.OnScoreUpdate = OnClientScoreUpdateClientRpc;
    }

    [ClientRpc]
    private void OnClientTurnChangeClientRpc(ulong activeClientId)
    {
        isTurnActive = activeClientId == OwnerClientId;
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
        Debug.Log($"Client{OwnerClientId} was dealt {c} at index {i}");
        var go = Instantiate(cardPrefab, handContainer.GetChild(i));
        var cui = go.GetComponent<CardUi>();
        cui.SetCard(IsOwner ? c : null);
    }

    [ServerRpc]
    public void PlayCardServerRpc(Card card)
    {
        gm.PlayCard(OwnerClientId, card);
    }

    [ClientRpc]
    private void OnCardPlayedClientRpc(int i, Card c)
    {
        var child = handContainer.GetChild(i).GetChild(0);
        lastPlayedCard = child.GetComponent<CardUi>();
        lastPlayedCard.SetCard(c);
        lastPlayedCard.enabled = false;
        child.SetParent(playedCardContainer);
        child.localPosition = Vector3.zero;
    }
}