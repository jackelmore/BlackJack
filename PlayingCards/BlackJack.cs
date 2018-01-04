using System;
using System.Collections.Generic;

namespace jackel.Cards
{
    public enum HandResult { Win, Lose, Bust, Push };
    public class BlackJack
    {
        public const int houseHand = 0;
        public const int maxHandValue = 21;
        public readonly int numPlayers;
        private Deck pile;
        private Deck[] hands;

        public BlackJack(int players = 1)
        {
            numPlayers = players;
            hands = new Deck[numPlayers + 1];
            pile = new Deck("Pile", hasJokers: false, withCards: true, useStaticCards: true); // get a full deck of cards

            for (int i = 0; i <= numPlayers; i++)
            {
                if (i == 0)
                    hands[i] = new Deck("House", hasJokers: false, withCards: false);
                else
                    hands[i] = new Deck($"Player {i.ToString()}", hasJokers: false, withCards: false);
                hands[i].EvCalculate += BJCalculate; // Use BlackJack calculation for card/hand values
            }
        }
        public int NumPileCards => pile.Count;
        public IEnumerable<PlayingCard> GetEnumerableHand(int hand)
        {
            if (hand > numPlayers)
                return null;
            return hands[hand];
        }
        public void ShowHand(int hand)
        {
            if (hand > numPlayers)
                return;
            hands[hand].SetAllFaceUp(true);
        }
        public void BJCalculate(object o, EventArgs e)
        {
            int numAces = 0;
            if (o is Deck deck)
            {
                deck.TotalValue = 0;
                foreach (PlayingCard p in deck)
                {
                    switch (p.Rank)
                    {
                        case Ranks.Joker:
                            break;
                        case Ranks.Jack:
                        case Ranks.Queen:
                        case Ranks.King:
                            deck.TotalValue += 10;
                            break;
                        case Ranks.Ace: // Count aces last (can be 1 or 11)
                            numAces++;
                            break;
                        default:
                            deck.TotalValue += p.RankAsInt;
                            break;
                    }
                }
                while (numAces-- > 0) // count Aces last to determine 1 vs. 11 value
                {
                    deck.TotalValue += 11;
                    if (deck.TotalValue > maxHandValue)
                        deck.TotalValue -= 10;
                }
            }
        }
        public void CollectCards()
        {
            // collect any cards out there back into the deck, set all to face down
            foreach (Deck d in hands)
                pile.CollectFrom(d);
            pile.SetAllFaceUp(false);
        }
        public void Deal()
        {
            CollectCards();
            pile.Shuffle();
            if (!pile.IsPristine)
            {
                throw new InvalidOperationException("Pile is not pristine and should be");
            }
            for (int i = 0; i < 2; i++) // deal two cards per player
                foreach (Deck d in hands)
                {
                    PlayingCard p = d.AddCardFrom(pile);
                    if (d.DeckName == "House" && i == 1)
                        p.faceUp = true;
                }
        }
        public void SortPile() => pile.Sort();
        public Deck ClonePile() => (Deck)pile.Clone();
        public PlayingCard Peek(int Player) => Player <= numPlayers ? hands[Player].Peek(1) : null;
        public int GetHandValue(int Hand) => Hand <= numPlayers ? hands[Hand].TotalValue : 0;
        public void PrintHand(int number)
        {
            if (number <= numPlayers)
                Console.Write(hands[number]);
        }
        public PlayingCard Hit(int Player)
        {
            if (hands[Player].TotalValue < maxHandValue)
            {
                PlayingCard p = hands[Player].AddCardFrom(pile);
                p.faceUp = true;
                return p;
            }
            return null;
        }
        public void AutoHit(int Player)
        {
            if (Player <= numPlayers)
                while (hands[Player].TotalValue <= 16)
                    Hit(Player);
        }
        public HandResult WinLoseOrBust(int player)
        {
            if (player > numPlayers)
                throw new InvalidOperationException($"player index {player} invalid!");
            int houseValue = GetHandValue(houseHand);
            int playerValue = GetHandValue(player);
            if (playerValue > maxHandValue) // Player busts
            {
                return HandResult.Bust;
            }
            else if (houseValue > maxHandValue || playerValue > houseValue) // Dealer busts, player wins
            {
                return HandResult.Win;
            }
            else if (playerValue < houseValue) // Player loses
            {
                return HandResult.Lose;
            }
            else // Push (Tie)
            {
                return HandResult.Push;
            }
        }
    }
}
