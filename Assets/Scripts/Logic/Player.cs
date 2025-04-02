using System;
using UnityEngine;
using UnityEngine.Events;

public class Player
{
    public ulong clientId;
    public const int handSize = 3;
    public Card[] hand = new Card[handSize];
    public int score;

    public Player(ulong clientId)
    {
        this.clientId = clientId;
    }

    public UnityAction<int, Card> OnCardDealt;

    public void DealCard(Card card)
    {
        for (int i = 0; i < handSize; i++)
        {
            if (hand[i] == null)
            {
                hand[i] = card;
                OnCardDealt?.Invoke(i, card);
                return;
            }
        }

        throw new InvalidOperationException($"Client{clientId}: Hand was full, could not deal card!");
    }
    
    public UnityAction<int, Card> OnCardPlayed;

    public void PlayCard(Card card)
    {
        for (int i = 0; i < handSize; i++)
        {
            if (card.Equals(hand[i]))
            {
                hand[i] = null;
                OnCardPlayed?.Invoke(i, card);
                return;
            }
        }
        throw new InvalidOperationException($"Client{clientId}: Card to play not found! {card}");
    }

    public void Score(int points)
    {
        score += points;
    }

    public override string ToString()
    {
        return $"Player{clientId} == {hand[0]}, {hand[1]}, {hand[2]}";
    }

}