using Unity.Netcode;

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