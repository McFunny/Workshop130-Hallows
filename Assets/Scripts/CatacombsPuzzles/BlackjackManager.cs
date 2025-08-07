using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackjackManager : MonoBehaviour
{
    public bool activeGame = false;
    public bool gameEnded = false;

    public static BlackjackManager Instance;

    public BlackjackBettingBox bettingBox;
    public BlackjackStartInteraction startInteraction;

    private List<Card> deck = new List<Card>();
    public int cardsLeftInDeck;
    private List<PlayingCard> playerHand = new List<PlayingCard>();
    private List<PlayingCard> dealerHand = new List<PlayingCard>();

    public List<Transform> playerCardSlots;
    public List<Transform> dealerCardSlots;

    private int playerCardIndex = 0;
    private int dealerCardIndex = 0;


    public Transform cardInstantiationPoint;

    public GameObject cardPrefab;

    public bool coroutineRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public IEnumerator StartGame()
    {
        coroutineRunning = true;
        ResetGame();
        if (deck.Count == 0) GenerateDeck();
        playerHand = new List<PlayingCard>();
        dealerHand = new List<PlayingCard>();
        activeGame = true;

        DrawCard(playerHand, false); // Player first card
        yield return new WaitForSeconds(0.5f);
        DrawCard(dealerHand, false); // Dealer first card
        yield return new WaitForSeconds(0.5f);
        DrawCard(playerHand, false); // Player second card
        yield return new WaitForSeconds(0.5f);
        DrawCard(dealerHand, true); // Dealer second card (face-down?)
        yield return new WaitForSeconds(0.5f);


        CheckPlayerBlackjack();
        coroutineRunning = false;
    }

    public IEnumerator Hit()
    {
        DrawCard(playerHand, false);
        yield return new WaitForSeconds(0.5f);

        int value = GetHandValue(playerHand);
        if (value > 21)
        {
            dealerHand[1].FlipDealerCard();
            EndGame(false);
        }
        else if (value == 21)
        {
            Stand(); 
        }
    }


    public void Stand()
    {
        if (!activeGame) return;

        StartCoroutine(DealerPlay());
    }

    private IEnumerator DealerPlay()
    {
        dealerHand[1].FlipDealerCard();

        yield return new WaitForSeconds(1f);

        while (GetHandValue(dealerHand) < 17)
        {
            DrawCard(dealerHand, false);
            yield return new WaitForSeconds(1f);
        }

        int dealerValue = GetHandValue(dealerHand);
        int playerValue = GetHandValue(playerHand);

        if (dealerValue > 21)
        {
            EndGame(true); 
        }
        else if (dealerValue > playerValue)
        {
            EndGame(false); 
        }
        else if (dealerValue < playerValue)
        {
            EndGame(true); 
        }
        else
        {
            EndGame(null); 
        }
    }


    void EndGame(bool? playerWon)
    {
        activeGame = false;
        int payout = 0;

        int betAmount = bettingBox.itemsDeposited;

        if (playerWon == true)
        {
            bool isBlackjack = playerHand.Count == 2 && GetHandValue(playerHand) == 21;
            if (isBlackjack)
            {
                payout = Mathf.FloorToInt(betAmount * 1.5f);
                Debug.Log("Blackjack! 3:2 payout: +" + payout);
            }
            else
            {
                payout = betAmount;
                Debug.Log("Win! 1:1 payout: +" + payout);
            }
        }
        else if (playerWon == false)
        {
            payout = -betAmount;
            Debug.Log("Loss: -" + betAmount);
        }
        else
        {
            payout = 0;
            Debug.Log("Push — bet returned.");
        }

        bettingBox.itemsDeposited += payout;
    }

    void GenerateDeck()
    {
        deck = new List<Card>();

        for (int i = 2; i <= 10; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                deck.Add(new Card(i));
            }
        }

       
        for (int j = 0; j < 4; j++)
            deck.Add(new Card(10, "J"));
        for (int j = 0; j < 4; j++)
            deck.Add(new Card(10, "Q"));
        for (int j = 0; j < 4; j++)
            deck.Add(new Card(10, "K"));

        
        for (int j = 0; j < 4; j++)
            deck.Add(new Card(11));

        ShuffleDeck();
    }



    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randIndex];
            deck[randIndex] = temp;
        }
    }

    void DrawCard(List<PlayingCard> hand, bool faceDown)
    {
        if (deck.Count == 0) GenerateDeck();

        Card cardData = deck[0];
        deck.RemoveAt(0);

        GameObject cardGameobject = Instantiate(cardPrefab, cardInstantiationPoint.position, Quaternion.identity);
        PlayingCard drawnCard = cardGameobject.GetComponent<PlayingCard>();
        drawnCard.SetupCard(cardData, faceDown);

        Transform targetSlot = null;
        if (hand == playerHand)
        {
            if (playerCardIndex < playerCardSlots.Count)
            {
                targetSlot = playerCardSlots[playerCardIndex];
                playerCardIndex++;
            }
        }
        else if (hand == dealerHand)
        {
            if (dealerCardIndex < dealerCardSlots.Count)
            {
                targetSlot = dealerCardSlots[dealerCardIndex];
                dealerCardIndex++;
            }
        }

        if (targetSlot != null)
            drawnCard.MoveToPoint(targetSlot, faceDown);

        hand.Add(drawnCard);
    }


    int GetHandValue(List<PlayingCard> hand)
    {
        int total = 0;
        int aceCount = 0;
        foreach (PlayingCard playingcard in hand)
        {
            total += playingcard.card.GetValue();
            if (playingcard.card.isAce) aceCount++;
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10; // convert Ace from 11 to 1
            aceCount--;
        }

        return total;
    }

    void CheckPlayerBlackjack()
    {
        if (GetHandValue(playerHand) == 21)
        {
            Debug.Log("Player has Blackjack!");
            EndGame(true);
        }
    }

    public void ResetGame()
    {
        if (playerHand.Count > 0)
        {
            foreach (var card in playerHand)
                Destroy(card.gameObject);
        }
        if (dealerHand.Count > 0)
        {
            foreach (var card in dealerHand)
                Destroy(card.gameObject);
        }

        playerHand.Clear();
        dealerHand.Clear();
        playerCardIndex = 0;
        dealerCardIndex = 0;
    }

    private void Update()
    {
        cardsLeftInDeck = deck.Count;
    }

}

[System.Serializable]
public class Card
{
    public int value; 
    public bool isAce;
    public string display; //if j q k

    public Card(int val, string displayOverride = null)
    {
        value = val;
        isAce = (val == 11);
        display = isAce ? "A" : displayOverride ?? val.ToString();
    }

    public int GetValue() => value;
}

