using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    private GameManager gm;
    private Transform handContainer;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client spawned on network with id {OwnerClientId}");
        if (IsOwner)
        {
            handContainer = GameObject.Find("HandTransform").transform;
            Invoke(nameof(ClientConnectServerRpc), 1f);
        }
    }

    [ServerRpc]
    private void ClientConnectServerRpc()
    {
        gm = FindObjectOfType<GameManager>();
        gm.ClientConnect(OwnerClientId);
        var player = gm.GetPlayer(OwnerClientId);
        player.OnCardDealt = OnCardDealtClientRpc;
    }

    [ClientRpc]
    private void OnCardDealtClientRpc(int i, Card c)
    {
        Debug.Log($"Client{OwnerClientId} was dealt {c} at index {i}");
        var cui = Instantiate(cardPrefab, handContainer)
            .GetComponent<CardUi>();
        cui.SetCard(c);
    }
}