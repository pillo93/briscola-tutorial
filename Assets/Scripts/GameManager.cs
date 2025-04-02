using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    ulong activePlayerId;
    private Player player1;
    private Player player2;
    private List<Card> deck;
    public Card briscolaCard { get; private set; }
    private int cardsPlayed = 0;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject aiClientPrefab;
    [SerializeField] private bool singlePlayer;

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameManager spawned on network! And it's owned by the Server: " + IsOwnedByServer);
        if (singlePlayer)
        {
            var aiClient = Instantiate(aiClientPrefab);
            aiClient.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        }
    }

    public void ClientConnect(ulong clientId)
    {
        Debug.Log($"GM: Client{clientId} connected.");
        if (player1 == null)
        {
            player1 = new Player(clientId);
        }
        else if (player2 == null)
        {
            player2 = new Player(clientId);
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
        StartPlayerTurn(player1, false);
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
        Debug.Log(string.Concat(deck.ConvertAll(c => c + ", ")));
    }

    private void DealInitialHand()
    {
        for (int i = 0; i < Player.handSize; i++)
        {
            player1.DealCard(GetFirstCard());
            player2?.DealCard(GetFirstCard());
        }
    }

    public UnityAction<ulong> OnActiveClientChange;

    private void StartPlayerTurn(Player p, bool shouldDealCards)
    {
        activePlayerId = p.clientId;
        if (shouldDealCards)
        {
            DrawCardsForTurn();
        }
        OnActiveClientChange?.Invoke(activePlayerId);
        Debug.Log("P1: " + player1);
        Debug.Log("P2: " + player2);
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
        if (deck.Count == 0)
        {
            HideDeckAndBriscolaClientRpc();
        }
    }

    [ClientRpc]
    private void DisplayBriscolaCardClientRpc(Card card)
    {
        var go = GameObject.Find("BriscolaCard");
        go.GetComponent<CardUi>().SetCard(card, true);
        go.GetComponent<CardUi>().enabled = false;
    }

    [ClientRpc]
    private void HideDeckAndBriscolaClientRpc()
    {
        GameObject.Find("BriscolaCard").GetComponent<Image>().enabled = false;
        GameObject.Find("Deck").GetComponent<Image>().enabled = false;
    }

    private Card cardPlayed;

    public void PlayCard(ulong ownerClientId, Card card)
    {
        Debug.Log($"GM: Client{ownerClientId} is playing {card}");
        var player = GetPlayer(ownerClientId);
        player.PlayCard(card);
        var opponent = player == player1 ? player2 : player1;
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
            if (cardsPlayed < 40)
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
        int points = c1.Score() + c2.Score();
        if (s1 == s2) // Quando i due semi sono uguali
        {
            Score(c1 > c2 ? p1 : p2, points);
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

    private Player winner;
    public UnityAction<ulong, int> OnPlayerScore;
    private void Score(Player p, int score)
    {
        winner = p;
        p.Score(score);
        OnPlayerScore?.Invoke(p.clientId, p.score);
    }

    private void CheckWhoWon()
    {
        if (player1.score > player2.score) DisplayWinnerClientRpc(player1.clientId);
        else if (player2.score > player1.score) DisplayWinnerClientRpc(player2.clientId);
        else DisplayTieClientRpc();
    }

    [ClientRpc]
    private void DisplayWinnerClientRpc(ulong winnerClientId)
    {
        var msg = winnerClientId == Client.Local.OwnerClientId ? "You Won!!!" : "You Lost :(((";
        var endPanel = GameObject.Find("EndGamePanel");
        endPanel.GetComponent<CanvasGroup>().alpha = 1f;
        endPanel.GetComponentInChildren<TMP_Text>().text = msg;
    }

    [ClientRpc]
    private void DisplayTieClientRpc()
    {
        var msg = "The match was a tie!";
        var endPanel = GameObject.Find("EndGamePanel");
        endPanel.SetActive(true);
        endPanel.GetComponentInChildren<TMP_Text>().text = msg;
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
        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public Player GetPlayer(ulong clientId)
    {
        if (player1.clientId == clientId) return player1;
        return player2.clientId == clientId ? player2 : null;
    }

    private Player GetOpponent(ulong clientId)
    {
        if (player1.clientId == clientId) return player2;
        return player2.clientId == clientId ? player1 : null;
    }

    #endregion
}