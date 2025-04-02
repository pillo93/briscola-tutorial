using System.Collections.Generic;
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
            new DelegateCommand(delegate
            {
                var playedCard = GetOpponentCard(); // controlliamo se c'e' una carta a tavola
                var ownedCards = GetHandCards(); // controlliamo quali carte abbiamo in mano
                var cardToPlay = PickCardToPlay(ownedCards, playedCard); // scegliamo quella da giocare
                PlayCardServerRpc(cardToPlay);
            }).AddToQueue();
        }
    }

    [ClientRpc]
    protected override void OnClientScoreUpdateClientRpc(ulong clientId, int newScore)
    {
        //skip, all AI non serve aggiornare la UI, ci pensa il client locale
    }

    private Card PickCardToPlay(List<Card> ownedCards, Card opponentCard)
    {
        if (opponentCard != null)
        {
            foreach (var c in ownedCards)
            {
                if (c.suit == opponentCard.suit && c > opponentCard) return c;
                if (c.suit == gm.briscolaCard.suit && opponentCard.suit != gm.briscolaCard.suit) return c;
            }
            Card res = ownedCards[0];
            foreach (var c in ownedCards)
            {
                if (c < res) res = c;
            }
            return res; // giochiamo la carta piu bassa
        }
        else
        {
            Card res = ownedCards[0];
            foreach (var c in ownedCards)
            {
                if (c > res) res = c;
            }
            return res; // giochiamo la carta piu alta
        }
    }

    private Card GetOpponentCard()
    {
        var playedCard = Local.lastPlayedCard;
        if (playedCard != null) // opponent has played a card
        {
            return playedCard.card;
        }
        return null;
    }

    private List<Card> GetHandCards()
    {
        var res = new List<Card>();
        for (int i = 0; i < handContainer.childCount; i++)
        {
            var slot = handContainer.GetChild(i);
            if (slot.childCount > 0)
                res.Add(slot.GetChild(0).GetComponent<CardUi>().card);
        }
        return res;
    }

}