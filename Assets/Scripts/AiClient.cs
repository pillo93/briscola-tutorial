using Unity.Netcode;
using UnityEngine;

public class AiClient : Client
{
    private new const ulong clientId = ulong.MaxValue;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"AIClient spawned on network with id {clientId}");
        Setup(false, clientId);
        ClientConnectServerRpc(clientId);
    }

    [ClientRpc]
    protected override void OnClientTurnChangeClientRpc(ulong activeClientId)
    {
        if (activeClientId == clientId)
        {
            Debug.Log($"AIClient's turn to pick a card");
        }
    }

}