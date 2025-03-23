using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public Suit suit;
    public Value value;

    public Card(Suit suit, Value value)
    {
        this.suit = suit;
        this.value = value;
    }

    public override string ToString()
    {
        return $"{value} di {suit}";
    }

}

public enum Suit
{
    BASTONI,
    COPPE,
    DENARI,
    SPADE
}

public enum Value
{
    ASSO,
    DUE,
    TRE,
    QUATTRO,
    CINQUE,
    SEI,
    SETTE,
    FANTE,
    CAVALLO,
    RE
}
