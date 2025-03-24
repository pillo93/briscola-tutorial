using Unity.Netcode;
using UnityEngine;

public class Client : NetworkBehaviour
{

    GameManager gm;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Client spawned on network with id {OwnerClientId}");
        Debug.Log($"IsLocalClient? {IsLocalPlayer}");
        if (IsOwner)
        {
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
    }

}
