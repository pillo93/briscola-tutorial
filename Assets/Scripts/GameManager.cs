using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    private Player p1;
    private Player p2;
    private List<Card> deck;

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameManager spawned on network! And it's owned by the Server: " + IsOwnedByServer);
    }

    public void ClientConnect(ulong clientId)
    {
        Debug.Log($"GM: Client{clientId} connected.");
        if (p1 == null)
        {
            p1 = new Player(clientId);
        }
        else if (p2 == null)
        {
            p2 = new Player(clientId);
            StartCoroutine(StartGame());
        }
        else
        {
            //We refuse more than 2 clients.
            throw new InvalidOperationException("Too many players, game already started!!");
        }
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1f);
        InitDeck();
        DealInitialHand();
    }

    private void InitDeck()
    {
        deck = new();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Value value in Enum.GetValues(typeof(Value)))
            {
                deck.Add(new Card(suit, value));
            }
        }

        Shuffle();
        Debug.Log("Deck initialized");
        Debug.Log(string.Concat(deck.ConvertAll(c => c + ", ")));
    }

    private void DealInitialHand()
    {
        for (int i = 0; i < Player.handSize; i++)
        {
            p1.DealCard(GetFirstCard());
            p2?.DealCard(GetFirstCard());
        }
    }

    private void Shuffle()
    {
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
    }

    private Card GetFirstCard()
    {
        Card card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public Player GetPlayer(ulong clientId)
    {
        if (p1.clientId == clientId) return p1;
        return p2.clientId == clientId ? p2 : null;
    }
}