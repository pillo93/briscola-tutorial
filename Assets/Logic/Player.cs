using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{

    public ulong clientId;
    public const int handSize = 3;
    public Card[] hand = new Card[handSize];

    public Player(ulong clientId) 
    {
        this.clientId = clientId;
    }

    public Action<int, Card> OnCardDealt;
    public void DealCard(Card card)
    {
        for (int i = 0; i < handSize; i++)
        {
            if (hand[i] == null)
            {
                hand[i] = card;
                OnCardDealt?.Invoke(i, card);
            }
        }
        throw new InvalidOperationException($"Client{clientId}: Hand was full, could not deal card!");
    }

}
