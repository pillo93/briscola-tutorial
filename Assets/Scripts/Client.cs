using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    public static Client Local;
    [SerializeField] private GameObject cardPrefab;
    private GameManager gm;
    private Transform handContainer;
    private Transform playedCardContainer;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client spawned on network with id {OwnerClientId}");
        if (IsOwner)
        {
            Local = this;
            GameObject.Find("NetworkUI").SetActive(false);
            handContainer = GameObject.Find("HandTransform").transform;
            playedCardContainer = GameObject.Find("PlayerCardSpot").transform;
            Invoke(nameof(ClientConnectServerRpc), 1f);
        }
        else
        {
            handContainer = GameObject.Find("EnemyHandTransform").transform;
            playedCardContainer = GameObject.Find("EnemyCardSpot").transform;
        }
    }

    [ServerRpc]
    private void ClientConnectServerRpc()
    {
        gm = FindObjectOfType<GameManager>();
        gm.ClientConnect(OwnerClientId);
        var player = gm.GetPlayer(OwnerClientId);
        player.OnCardDealt = OnCardDealtClientRpc;
        player.OnCardPlayed = OnCardPlayedClientRpc;
    }

    [ClientRpc]
    private void OnCardDealtClientRpc(int i, Card c)
    {
        Debug.Log($"Client{OwnerClientId} was dealt {c} at index {i}");
        var go = Instantiate(cardPrefab, handContainer);
        go.transform.SetSiblingIndex(i);
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
        var child = handContainer.GetChild(i);
        var cardUi = child.GetComponent<CardUi>();
        cardUi.SetCard(c);
        cardUi.enabled = false;
        child.SetParent(playedCardContainer);
        child.localPosition = Vector3.zero;
    }
}