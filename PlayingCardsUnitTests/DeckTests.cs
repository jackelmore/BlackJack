using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jackel.Cards;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    [TestClass]
    public class DeckTests
    {
        [TestMethod]
        public void SortDeck()
        {
            Deck d1 = new Deck("With Jokers", true, true, true);
            Deck d2 = new Deck("Without Jokers", false, true, true);
            Assert.IsTrue(d1.IsPristine);
            Assert.IsTrue(d2.IsPristine);

            d1.Shuffle();
            d2.Shuffle();
            Assert.IsTrue(d1.IsPristine);
            Assert.IsTrue(d2.IsPristine);

            d1.Sort();
            d2.Sort();
            Assert.AreEqual(PlayingCard.cardNumMin, d1.Peek(0).CardInt);
            Assert.AreEqual(PlayingCard.jokerNum, d1.Peek(d1.Count - 1).CardInt);
            Assert.AreEqual(PlayingCard.cardNumMin, d2.Peek(0).CardInt);
            Assert.AreEqual(PlayingCard.cardNumMax, d2.Peek(d2.Count - 1).CardInt);

            d1.SortByRank();
            d2.SortByRank();
            Assert.AreEqual(Ranks.Two, d1.Peek(0).Rank);
            Assert.AreEqual(Ranks.Joker, d1.Peek(d1.Count - 1).Rank);
            Assert.AreEqual(Ranks.Two, d2.Peek(0).Rank);
            Assert.AreEqual(Ranks.Ace, d2.Peek(d2.Count - 1).Rank);

            d1.SortBySuit();
            d2.SortBySuit();
            Assert.AreEqual(Suits.Clubs, d1.Peek(0).Suit);
            Assert.AreEqual(Suits.Joker, d1.Peek(d1.Count - 1).Suit);
            Assert.AreEqual(Suits.Clubs, d2.Peek(0).Suit);
            Assert.AreEqual(Suits.Spades, d2.Peek(d2.Count - 1).Suit);

            d1.DrawOne();
            d2.DrawOne();
            Assert.IsFalse(d1.IsPristine);
            Assert.IsFalse(d2.IsPristine);
        }

        [TestMethod]
        public void MakeDecks()
        {
            Deck d1 = new Deck("Deck 1", true, true, true);
            Deck d2 = new Deck("Deck 2", false, true, true);
            Deck d3 = (Deck)d1.Clone();
            Deck d4 = new Deck("Empty", true, false, true);

            Assert.AreEqual(54, d1.Count);
            Assert.AreEqual(52, d2.Count);
            Assert.AreEqual(d1.Count, d3.Count);
            Assert.AreEqual(0, d4.Count);

            Assert.IsTrue(d1.IsPristine && d2.IsPristine && d3.IsPristine);
            d4.CollectFrom(d1);
            Assert.IsFalse(d1.IsPristine);
            Assert.IsTrue(d4.IsPristine);
            d1.CollectFrom(d4);
            Assert.IsTrue(d1.IsPristine);
            Assert.IsFalse(d4.IsPristine);
            d1.CollectFrom(d3);
            Assert.AreEqual(108, d1.Count);
            Assert.AreEqual(0, d3.Count);

            while (d1.Count > 0)
                d3.Add(d1.DrawOne());
            Assert.AreEqual(0, d1.Count);
            Assert.AreEqual(108, d3.Count);
        }

        [TestMethod]
        public void JSONSerialize()
        {
            var deck = new Deck("Serialized", hasJokers: true, withCards: false, useStaticCards: false);
            deck.Add(new PlayingCard(Suits.Clubs, Ranks.Two));
            deck.Add(new PlayingCard(Suits.Spades, Ranks.Ace));
            deck.Add(new PlayingCard(Suits.Joker, Ranks.Joker));

            var json = deck.ToJsonString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var jDeck = JObject.Parse(json);
            Assert.AreEqual(deck.DeckName, (string)jDeck[nameof(Deck.DeckName)]);

            var jCards = (JArray)jDeck["cards"];
            Assert.AreEqual(deck.Count, jCards.Count);
            Assert.AreEqual(deck.Peek(0).CardInt, (int)jCards[0][nameof(PlayingCard.CardInt)]);
            Assert.AreEqual(deck.Peek(1).CardInt, (int)jCards[1][nameof(PlayingCard.CardInt)]);
            Assert.AreEqual(deck.Peek(2).CardInt, (int)jCards[2][nameof(PlayingCard.CardInt)]);
        }
    }
}
