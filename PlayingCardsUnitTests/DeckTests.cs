using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jackel.Cards;

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
            Assert.IsTrue(d1.IsPristine);
            Assert.IsTrue(d2.IsPristine);
            d1.SortByRank();
            d2.SortByRank();
            Assert.IsTrue(d1.IsPristine);
            Assert.IsTrue(d2.IsPristine);
            d1.SortBySuit();
            d2.SortBySuit();
            Assert.IsTrue(d1.IsPristine);
            Assert.IsTrue(d2.IsPristine);
            d1.DrawOne();
            d2.DrawOne();
            Assert.IsFalse(d1.IsPristine);
            Assert.IsFalse(d2.IsPristine);
        }
        [TestMethod]
        public void MakeDecks()
        {
            Deck d1 = new Deck("Deck 1", true, true, true);  // with jokers
            Deck d2 = new Deck("Deck 2", false, true, true); // no jokers
            Deck d3 = (Deck)d1.Clone();
            Deck d4 = new Deck("Empty", true, false, true);

            // Check card counts
            Assert.IsTrue(d1.Count == 54); // has jokers
            Assert.IsTrue(d2.Count == 52); // doesn't have jokers;
            Assert.IsTrue(d1.Count == d3.Count);
            Assert.IsTrue(d4.Count == 0);

            // Check pristine
            Assert.IsTrue(d1.IsPristine && d2.IsPristine && d3.IsPristine);
            d4.CollectFrom(d1);
            Assert.IsFalse(d1.IsPristine);
            Assert.IsTrue(d4.IsPristine);
            d1.CollectFrom(d4);
            Assert.IsTrue(d1.IsPristine);
            Assert.IsFalse(d4.IsPristine);
            d1.CollectFrom(d3);
            Assert.IsTrue(d1.Count == 108);
            Assert.IsTrue(d3.Count == 0);

            while (d1.Count > 0)
                d3.Add(d1.DrawOne());
            Assert.IsTrue(d1.Count == 0);
            Assert.IsTrue(d3.Count == 108);
        }
        [TestMethod]

        public void JSONSerialize()
        {
            Console.WriteLine(new Deck().ToJsonString());
        }
    }
}
