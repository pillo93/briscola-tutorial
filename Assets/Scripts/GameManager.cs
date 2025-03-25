using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    ulong activePlayerId;
    private Player p1;
    private Player p2;
    private List<Card> deck;
    private Card briscolaCard;
    private int cardsPlayed = 0;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] private bool singlePlayer;

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
            if (singlePlayer)
            {
                StartCoroutine(StartGame());
            }
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
        //Dopo aver dato carte ai giocatori, ne prendiamo una che fara' da briscola
        briscolaCard = GetFirstCard();
        DisplayBriscolaCardClientRpc(briscolaCard);
        deck.Add(briscolaCard); // we put the briscola as last card
        StartPlayerTurn(p1, false);
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

    public UnityAction<ulong> OnActiveClientChange;
    private void StartPlayerTurn(Player p, bool shouldDealCards)
    {
        activePlayerId = p.clientId;
        OnActiveClientChange?.Invoke(activePlayerId);
        if (shouldDealCards)
        {
            DrawCardsForTurn();
        }
    }

    private void DrawCardsForTurn()
    {
        if (deck.Count > 0)
        {
            var activePlayer = GetPlayer(activePlayerId);
            activePlayer.DealCard(GetFirstCard());
            var opp = GetOpponent(activePlayerId);
            opp.DealCard(GetFirstCard());
        }
    }

    [ClientRpc]
    private void DisplayBriscolaCardClientRpc(Card card)
    {
        var deckTransform = GameObject.Find("Deck").transform;
        var go = Instantiate(cardPrefab, deckTransform.parent);
        go.transform.SetSiblingIndex(deckTransform.GetSiblingIndex() - 1);
        go.transform.position = deckTransform.position;
        go.transform.rotation = Quaternion.Euler(0, 0, -90);
        go.GetComponent<CardUi>().SetCard(card);
        go.GetComponent<CardUi>().enabled = false;
    }

    Card cardPlayed = null;
    public void PlayCard(ulong ownerClientId, Card card)
    {
        var player = GetPlayer(ownerClientId);
        player.PlayCard(card);
        var opponent = player == p1 ? p2 : p1;
        //Se e' la prima carta sul tavolo, la salviamo e diamo priorita' all'avversario
        if (cardPlayed == null)
        {
            cardPlayed = card;
            StartPlayerTurn(opponent, false);
        }
        else
        {
            //Se c'era gia' una carta a tavola, calcoliamo chi fa punto
            //La carta gia' giocata e' dell'opponent e ha precedenza di punto
            CheckScore(cardPlayed, opponent, card, player);
            cardPlayed = null;
            cardsPlayed += 2;
            if(cardsPlayed < 40)
            {
                StartPlayerTurn(winner, true); // Inizia il turno chi ha preso, e si pesca
            }
            else
            {
                CheckWhoWon();
            }
        }
    }

    // p1 e' il giocatore che ha giocato la prima carta, avra' priorita' nel calcolo punti
    private void CheckScore(Card c1, Player p1, Card c2, Player p2)
    {
        Suit briscola = briscolaCard.suit;
        Suit s1 = c1.suit; 
        Suit s2 = c2.suit;
        Value v1 = c1.value;
        Value v2 = c2.value;
        int points = v1.Score() + v2.Score();
        if (s1 == s2) // Quando i due semi sono uguali
        {
            if (v1.Score() > v2.Score()) Score(p1, points);
            else Score(p2, points);
        }
        else
        {
            if (s1 == briscola && s2 != briscola) // se p1 ha giocato briscola e p2 no
            {
                Score(p1, points);
            }
            else if (s2 == briscola && s1 != briscola) // il contrario
            {
                Score(p2, points);
            }
            else // nessuno ha giocato briscola e i semi sono diversi, vince p1 per priorita'
            {
                Score(p1, points);
            }
        }

    }

    Player winner;
    private void Score(Player p, int score)
    {
        winner = p;
        p.Score(score);
        GetOpponent(p.clientId).Score(0);
    }

    private void CheckWhoWon()
    {
        if (p1.score > p2.score) DisplayWinnerClientRpc(p1.clientId);
        else if (p2.score > p1.score) DisplayWinnerClientRpc(p2.clientId);
        else DisplayTieClientRpc();
    }

    [ClientRpc]
    private void DisplayWinnerClientRpc(ulong winnerClientId)
    {
        var msg = winnerClientId == Client.Local.OwnerClientId ? "You Won!!!" : "You Lost :(((";
        Debug.Log(msg);
    }

    [ClientRpc]
    private void DisplayTieClientRpc()
    {
        Debug.Log("The match was a tie!");
    }

    #region utils

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

    public Player GetOpponent(ulong clientId)
    {
        if (p1.clientId == clientId) return p2;
        return p2.clientId == clientId ? p1 : null;
    }

    #endregion

}