using System;
using System.Collections.Generic;

namespace jackel.Cards
{
    /// <summary>
    /// Represents the result of a BlackJack hand comparison.
    /// </summary>
    public enum HandResult { Win, Lose, Bust, Push };

    /// <summary>
    /// Manages the game logic for BlackJack, including dealing, hitting, and determining hand values.
    /// </summary>
    public class BlackJack
    {
        /// <summary>
        /// The player number representing the house/dealer.
        /// </summary>
        public const int houseHand = 0;

        /// <summary>
        /// The maximum hand value before busting.
        /// </summary>
        public const int maxHandValue = 21;

        /// <summary>
        /// The threshold at which the dealer must stand (17 or higher).
        /// </summary>
        private const int DEALER_STAND_VALUE = 17;

        /// <summary>
        /// The number of cards dealt to each player at the start of a game.
        /// </summary>
        private const int INITIAL_DEAL_COUNT = 2;

        /// <summary>
        /// The index of the second card dealt (house's face-up card in BlackJack).
        /// </summary>
        private const int HOUSE_FACE_UP_CARD_INDEX = 1;

        /// <summary>
        /// The number of players in the game.
        /// </summary>
        public readonly int numPlayers;

        private readonly Deck pile;
        private readonly Deck[] hands;

        /// <summary>
        /// Initializes a new instance of the BlackJack class.
        /// </summary>
        /// <param name="players">The number of players for the game. Defaults to 1.</param>
        public BlackJack(int players = 1)
        {
            if (players < 1)
                throw new ArgumentException("Number of players must be at least 1.", nameof(players));

            numPlayers = players;
            hands = new Deck[numPlayers + 1];
            pile = new Deck("Pile", hasJokers: false, withCards: true, useStaticCards: true);

            for (int i = 0; i <= numPlayers; i++)
            {
                hands[i] = new Deck(name: i == 0 ? "House" : $"Player {i}", hasJokers: false, withCards: false);
                hands[i].EvCalculate += BJCalculate;
            }
        }

        /// <summary>
        /// Gets the number of cards remaining in the pile.
        /// </summary>
        public int NumPileCards => pile.Count;

        /// <summary>
        /// Gets the enumerable hand for the specified player.
        /// </summary>
        /// <param name="hand">The player number (0 for house).</param>
        /// <returns>An enumerable of the player's cards, or null if player number is invalid.</returns>
        public IEnumerable<PlayingCard> GetEnumerableHand(int hand)
        {
            if (!IsValidPlayerNumber(hand))
                return null;
            return hands[hand];
        }

        /// <summary>
        /// Attempts to get the enumerable hand for the specified player.
        /// </summary>
        /// <param name="hand">The player number (0 for house).</param>
        /// <param name="result">The enumerable of the player's cards if successful.</param>
        /// <returns>True if the hand was retrieved; otherwise, false.</returns>
        public bool TryGetEnumerableHand(int hand, out IEnumerable<PlayingCard> result)
        {
            result = null;
            if (!IsValidPlayerNumber(hand))
                return false;
            result = hands[hand];
            return true;
        }

        /// <summary>
        /// Reveals all cards in the specified hand by setting them face-up.
        /// </summary>
        /// <param name="hand">The player number (0 for house).</param>
        public void ShowHand(int hand)
        {
            if (IsValidPlayerNumber(hand))
                hands[hand].SetAllFaceUp(true);
        }

        /// <summary>
        /// Calculates the BlackJack hand value, treating aces as either 1 or 11 as needed.
        /// </summary>
        /// <param name="o">The deck object whose value should be calculated.</param>
        /// <param name="e">Event arguments (unused).</param>
        public void BJCalculate(object o, EventArgs e)
        {
            if (!(o is Deck deck))
                return;

            deck.TotalValue = 0;
            int numAces = CountAndAddCardValues(deck);
            HandleAceValues(deck, numAces);
        }

        /// <summary>
        /// Counts non-ace cards and adds their values to the total.
        /// </summary>
        /// <param name="deck">The deck being evaluated.</param>
        /// <returns>The number of aces in the deck.</returns>
        private int CountAndAddCardValues(Deck deck)
        {
            int numAces = 0;
            foreach (PlayingCard card in deck)
            {
                switch (card.Rank)
                {
                    case Ranks.Joker:
                        break;
                    case Ranks.Jack:
                    case Ranks.Queen:
                    case Ranks.King:
                        deck.TotalValue += 10;
                        break;
                    case Ranks.Ace:
                        numAces++;
                        break;
                    default:
                        deck.TotalValue += card.RankAsInt;
                        break;
                }
            }
            return numAces;
        }

        /// <summary>
        /// Handles ace value calculation, counting them as 11 or 1 to avoid busting.
        /// </summary>
        /// <param name="deck">The deck being evaluated.</param>
        /// <param name="numAces">The number of aces in the deck.</param>
        private void HandleAceValues(Deck deck, int numAces)
        {
            while (numAces-- > 0)
            {
                deck.TotalValue += 11;
                if (deck.TotalValue > maxHandValue)
                    deck.TotalValue -= 10;
            }
        }

        /// <summary>
        /// Collects all cards from players back into the pile and resets them to face-down.
        /// </summary>
        public void CollectCards()
        {
            foreach (Deck hand in hands)
                pile.CollectFrom(hand);
            pile.SetAllFaceUp(false);
        }

        /// <summary>
        /// Deals the initial cards to all players and the house.
        /// </summary>
        public void Deal()
        {
            CollectCards();
            pile.Shuffle();

            if (!pile.IsPristine)
                throw new InvalidOperationException("Pile is not pristine and should be.");

            DealCardsToPlayers();
        }

        /// <summary>
        /// Deals the initial cards to all players and house.
        /// </summary>
        private void DealCardsToPlayers()
        {
            for (int cardIndex = 0; cardIndex < INITIAL_DEAL_COUNT; cardIndex++)
            {
                foreach (Deck hand in hands)
                {
                    PlayingCard card = hand.AddCardFrom(pile);
                    
                    // House's second card is face-up; others are face-down initially
                    if (hand.DeckName == "House" && cardIndex == HOUSE_FACE_UP_CARD_INDEX)
                        card.faceUp = true;
                }
            }
        }

        /// <summary>
        /// Sorts the pile in ascending order.
        /// </summary>
        public void SortPile() => pile.Sort();

        /// <summary>
        /// Creates a deep clone of the pile.
        /// </summary>
        /// <returns>A cloned deck representing the current pile state.</returns>
        public Deck ClonePile() => (Deck)pile.Clone();

        /// <summary>
        /// Peeks at a card in the specified player's hand without removing it.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        /// <returns>The card at the peek position, or null if the player number is invalid.</returns>
        public PlayingCard Peek(int playerNumber) => IsValidPlayerNumber(playerNumber) ? hands[playerNumber].Peek(1) : null;

        /// <summary>
        /// Gets the total value of the specified player's hand.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        /// <returns>The hand value, or 0 if the player number is invalid.</returns>
        public int GetHandValue(int playerNumber) => IsValidPlayerNumber(playerNumber) ? hands[playerNumber].TotalValue : 0;

        /// <summary>
        /// Prints the specified player's hand to the console.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        public void PrintHand(int playerNumber)
        {
            if (IsValidPlayerNumber(playerNumber))
                Console.Write(hands[playerNumber]);
        }

        /// <summary>
        /// Draws a card for the specified player if their hand value is under 21.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        /// <returns>The card drawn, or null if the player cannot hit.</returns>
        public PlayingCard Hit(int playerNumber)
        {
            if (!IsValidPlayerNumber(playerNumber))
                return null;

            if (hands[playerNumber].TotalValue >= maxHandValue)
                return null;

            PlayingCard card = hands[playerNumber].AddCardFrom(pile);
            card.faceUp = true;
            return card;
        }

        /// <summary>
        /// Automatically deals cards to the specified player until their hand reaches the dealer stand value.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        public void AutoHit(int playerNumber)
        {
            if (!IsValidPlayerNumber(playerNumber))
                return;

            while (hands[playerNumber].TotalValue < DEALER_STAND_VALUE)
                Hit(playerNumber);
        }

        /// <summary>
        /// Determines the result of a player's hand against the dealer's hand.
        /// </summary>
        /// <param name="playerNumber">The player number to evaluate.</param>
        /// <returns>The hand result (Win, Lose, Bust, or Push).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the player number is invalid.</exception>
        public HandResult WinLoseOrBust(int playerNumber)
        {
            if (!IsValidPlayerNumber(playerNumber))
                throw new InvalidOperationException($"Player index {playerNumber} is invalid.");

            int houseValue = GetHandValue(houseHand);
            int playerValue = GetHandValue(playerNumber);

            if (playerValue > maxHandValue)
                return HandResult.Bust;

            if (houseValue > maxHandValue || playerValue > houseValue)
                return HandResult.Win;

            if (playerValue < houseValue)
                return HandResult.Lose;

            return HandResult.Push;
        }

        /// <summary>
        /// Validates that a player number is within the valid range.
        /// </summary>
        /// <param name="playerNumber">The player number to validate.</param>
        /// <returns>True if the player number is valid; otherwise, false.</returns>
        private bool IsValidPlayerNumber(int playerNumber) => playerNumber >= 0 && playerNumber <= numPlayers;
    }
}
