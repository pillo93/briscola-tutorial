using Unity.Netcode;
using Unity.VisualScripting;

public class Card : INetworkSerializable
{
    public Suit suit;
    public Value value;

    public Card()
    {
        //Required for serialization
    }

    public Card(Suit suit, Value value)
    {
        this.suit = suit;
        this.value = value;
    }

    public static bool operator >(Card c1, Card c2)
    {
        return c1.Score() > c2.Score();
    }

    public static bool operator <(Card c1, Card c2)
    {
        return c1.Score() < c2.Score();
    }

    public override string ToString()
    {
        return $"{value} di {suit}";
    }

    public override bool Equals(object obj)
    {
        return obj is Card c && value == c.value && suit == c.suit;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref suit);
        serializer.SerializeValue(ref value);
    }

    public int Score()
    {
        switch (value)
        {
            case Value.ASSO: return 11;
            case Value.TRE: return 10;
            case Value.RE: return 4;
            case Value.CAVALLO: return 3;
            case Value.FANTE: return 2;
            default: return 0;
        }
    }

}

public enum Suit
{
    DENARI,
    COPPE,
    BASTONI,
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

