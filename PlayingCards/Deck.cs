using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace jackel.Cards
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Deck : ICloneable, IEnumerable<PlayingCard>
    {
        [JsonProperty]
        private readonly List<PlayingCard> cards = new List<PlayingCard>();
        [JsonProperty]
        private bool AllowJokers;
        public int TotalValue { get; set; }
        [JsonProperty]
        private readonly Guid DeckGUID = Guid.NewGuid();
        [JsonProperty]
        public string DeckName { get; set; }
        [JsonProperty]
        public bool IsEmpty => (cards.Count == 0);
        [JsonProperty]
        public int Count => cards.Count;

        public event EventHandler EvDraw, EvCombine, EvCalculate;
        protected virtual void OnDraw() => EvDraw?.Invoke(this, EventArgs.Empty);
        protected virtual void OnCombine() => EvCombine?.Invoke(this, EventArgs.Empty);
        protected virtual void OnCalculate() => EvCalculate?.Invoke(this, EventArgs.Empty);

        private readonly static List<PlayingCard> defaultCards = new List<PlayingCard>();
        private Random rand = new Random();

        static Deck() // Create the single set of cards that all other decks reference.
        {
            for (int s = PlayingCard.cardNumMin; s <= PlayingCard.jokerNum; s++)
                defaultCards.Add(new PlayingCard(s));
            defaultCards.Add(new PlayingCard(PlayingCard.jokerNum)); // add the second joker
        }
        private void AddStaticCards()
        {
            foreach (PlayingCard p in defaultCards)
            {
                if (p.IsJoker())
                    if (AllowJokers)
                        cards.Add(p);
                    else
                        continue;
                else
                    cards.Add(p);
            }
        }
        private void AddDefaultCards()
        {
            for (int s = PlayingCard.cardNumMin; s <= PlayingCard.cardNumMax; s++)
            {
                cards.Add(new PlayingCard(s));
            }
            if (AllowJokers)
            {
                cards.Add(new PlayingCard(PlayingCard.jokerNum));
                cards.Add(new PlayingCard(PlayingCard.jokerNum));
            }
        }
        public Deck(string name = "Unnamed", bool hasJokers = false, bool withCards = true, bool useStaticCards = true)
        {
            DeckName = name;
            AllowJokers = hasJokers;
            if (withCards)
            {
                if (useStaticCards)
                    AddStaticCards();
                else
                    AddDefaultCards();
                OnCalculate();
            }
        }
        public Deck() : this(name: "Unnamed", hasJokers: false, withCards: true, useStaticCards: true)
        { }

        public bool IsPristine
        {
            get
            {
                if (cards.Count != (AllowJokers ? 54 : 52))
                    return false;
                for (int cardNum = PlayingCard.cardNumMin; cardNum <= PlayingCard.cardNumMax; cardNum++) // check without jokers first
                {
                    if (!cards.Exists(x => x.CardInt == cardNum))
                        return false;
                }
                if (AllowJokers)
                    if (cards.FindAll(x => x.CardInt == PlayingCard.jokerNum).Count != 2)
                        return false;
                return true;
            }
        }
        public string ToXmlString()
        {
            return JsonConvert.DeserializeXNode(this.ToJsonString(), nameof(Deck)).ToString();
        }
        public void SetAllFaceUp(bool isFaceUp)
        {
            foreach (PlayingCard p in cards)
                p.faceUp = isFaceUp;
        }
        public void Sort()
        {
            cards.Sort();
        }
        public void SortByRank()
        {
            cards.Sort((PlayingCard x, PlayingCard y) =>
            {
                if (x.Rank < y.Rank)
                    return -1;
                else if (x.Rank > y.Rank)
                    return 1;
                else return
                    0;
            }
            );
        }
        public void SortBySuit()
        {
            cards.Sort((PlayingCard x, PlayingCard y) =>
            {
                if (x.Suit < y.Suit)
                    return -1;
                else if (x.Suit > y.Suit)
                    return 1;
                else return 0;
            });
        }
        public PlayingCard Add(PlayingCard p)
        {
            if (p != null)
            {
                cards.Add(p);
                OnCalculate();
            }
            return p;
        }
        public bool Remove(PlayingCard p)
        {
            if (p != null)
            {
                bool result = cards.Remove(p);
                if (result == true)
                    OnCalculate();
                return result;
            }
            return false;
        }
        public IEnumerator<PlayingCard> GetEnumerator() => cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Shuffle()
        {
            PlayingCard temp;
            int j;
            if (Count < 2)
                return;
            for (int i = 0; i < (Count - 1); i++)
            {
                j = rand.Next(i + 1, Count);
                temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
        public void ShuffleWithDeck(Deck d2)
        {
            CollectFrom(d2);
            Shuffle();
        }
        public PlayingCard Peek(int index)
        {
            if (index > Count - 1)
                return null;
            return cards[index];
        }
        public PlayingCard DrawOne()
        {
            if (Count > 0)
            {
                PlayingCard returnCard = cards[0];
                Remove(returnCard);
                OnDraw();
                return returnCard;
            }
            return null;
        }
        public void RemoveCards()
        {
            cards.Clear();
        }
        public PlayingCard AddCardFrom(Deck fromDeck)
        {
            if (fromDeck.Count == 0)
                return null;
            return Add(fromDeck.DrawOne());
        }

        public bool AddRandomCardFrom(Deck fromDeck)
        {
            if (fromDeck.Count == 0)
                return false;
            int cardnum = rand.Next(0, fromDeck.cards.Count);
            PlayingCard theCard = fromDeck.cards[cardnum];
            fromDeck.Remove(theCard);
            Add(theCard);
            return true;
        }
        public Deck CollectFrom(Deck d1)
        {
            cards.AddRange(d1.cards);
            d1.cards.Clear();
            d1.OnCalculate();
            OnCombine();
            OnCalculate();
            return this;
        }
        public int CountOfSuit(Suits s)
        {
            int counter = 0;
            foreach (PlayingCard p in cards)
            {
                if (p.Suit == s) counter++;
            }
            return counter;
        }
        public string SuitToString(Suits s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{DeckName}: Cards for Suit {s.ToString()}{Environment.NewLine}");

            foreach (PlayingCard p in cards)
            {
                if (p.Suit == s)
                    sb.Append(p.ToString() + Environment.NewLine);
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{DeckName}: Number of Cards in Deck is {Count}, totalValue is {TotalValue}{Environment.NewLine}");
            foreach (PlayingCard p in cards)
            {
                sb.Append(p.ToString() + Environment.NewLine);
            }
            return sb.ToString();
        }
        public string ToJsonString() => JsonConvert.SerializeObject(this);
        /// <summary>
        /// This function REMOVES the cards from d2. All combined cards end up in d1 and d2's card List is cleared.
        /// </summary>
        /// <param name="d1">Target (destination) deck</param>
        /// <param name="d2">Source deck (removes cards)</param>
        /// <returns></returns>
        public static Deck operator +(Deck d1, Deck d2)
        {
            return d1.CollectFrom(d2);
        }
        public static Deck operator +(Deck d, PlayingCard p)
        {
            d.Add(p);
            return d;
        }
        public static Deck operator -(Deck d, PlayingCard p)
        {
            d.Remove(p);
            return d;
        }
        /// <summary>
        /// Makes a copy of the deck including event triggers. Add cards to underlying list then calls OnCalculate once instead of firing them for each card upon movement.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Deck newDeck = new Deck(name: DeckName, hasJokers: AllowJokers, withCards: false)
            {
                EvCalculate = EvCalculate,
                EvDraw = EvDraw,
                EvCombine = EvCombine
            }; // new empty deck
            newDeck.cards.AddRange(cards);
            newDeck.OnCalculate();
            return newDeck;
        }
    }
}
