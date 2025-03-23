using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

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
            StartGame();
        }
        else if (p2 == null)
        {
            p2 = new Player(clientId);
            StartGame();
        }
        else
        {
            //We refuse more than 2 clients.
            throw new InvalidOperationException("Too many players, game already started!!");
        }
    }

    private void StartGame()
    {
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
        Debug.Log(string.Concat(deck.ConvertAll(c => c.ToString() + ", ")));
    }

    private void DealInitialHand()
    {

    }

    private void Shuffle()
    {
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
    }

}
