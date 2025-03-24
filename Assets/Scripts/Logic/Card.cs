using Unity.Netcode;

public class Card : INetworkSerializable
{
    public Suit suit;
    public Value value;

    public Card()
    {
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