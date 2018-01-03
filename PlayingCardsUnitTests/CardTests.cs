using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using jackel.Cards;

namespace UnitTests
{
    [TestClass]
    public class PlayingCardTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CardNumTooHigh() => new PlayingCard(PlayingCard.cardNumMax + 1);    // cardnum too high
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CardNumTooLow() => new PlayingCard(PlayingCard.cardNumMin - 1);     // cardnum too low
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SuitTooLow() => new PlayingCard(Suits.Clubs - 1, Ranks.Two);    // Suit out-of-bounds (low)
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SuitTooHigh() => new PlayingCard(Suits.Joker + 1, Ranks.Two);   // Suit out-of-bounds (high)
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RankTooLow() => new PlayingCard(Suits.Clubs, Ranks.Two - 1);    // Rank out-of-bounds (low)
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RankTooHigh() => new PlayingCard(Suits.Spades, Ranks.Joker + 1); // Rank out-of-bounds (high)
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NegativeCardNum() => new PlayingCard(-1);                       // cardnum is negative
        [TestMethod]
        public void ShortName()
        {
            PlayingCard twohearts = new PlayingCard(Suits.Hearts, Ranks.Two);
            PlayingCard joker = new PlayingCard(Suits.Joker, Ranks.Joker);
            PlayingCard queenclubs = new PlayingCard(Suits.Clubs, Ranks.Queen);

            Assert.IsTrue(twohearts.ShortName == "H2");
            Assert.IsTrue(joker.ShortName == "J");
            Assert.IsTrue(queenclubs.ShortName == "CQ");
        }
        [TestMethod]
        public void CardColor()
        {
            PlayingCard hearts = new PlayingCard(Suits.Hearts, Ranks.Two);
            PlayingCard diamonds = new PlayingCard(Suits.Diamonds, Ranks.Ten);
            PlayingCard clubs = new PlayingCard(Suits.Clubs, Ranks.Ace);
            PlayingCard spades = new PlayingCard(Suits.Spades, Ranks.Eight);
            PlayingCard joker = new PlayingCard(Suits.Joker, Ranks.Joker);

            Assert.IsTrue(hearts.Color == Colors.Red);
            Assert.IsFalse(hearts.Color != Colors.Red);
            Assert.IsTrue(diamonds.Color == Colors.Red);
            Assert.IsFalse(diamonds.Color != Colors.Red);
            Assert.IsTrue(clubs.Color == Colors.Black);
            Assert.IsFalse(clubs.Color != Colors.Black);
            Assert.IsTrue(spades.Color == Colors.Black);
            Assert.IsFalse(spades.Color != Colors.Black);
            Assert.IsTrue(joker.Color == Colors.Black);
            Assert.IsFalse(joker.Color != Colors.Black);
        }
        [TestMethod]
        public void ComparisonEquality()
        {
            PlayingCard p1 = new PlayingCard(2);
            PlayingCard p2 = new PlayingCard(52);
            PlayingCard p3 = (PlayingCard)p2.Clone();
            PlayingCard joker1 = new PlayingCard(Suits.Joker, Ranks.Two);
            PlayingCard joker2 = new PlayingCard(Suits.Hearts, Ranks.Joker);
            PlayingCard joker3 = new PlayingCard(Suits.Joker, Ranks.Joker);

            // Check Guids
            Assert.IsTrue(p1.CardGUID != p2.CardGUID);
            Assert.IsTrue(p2.CardGUID != p3.CardGUID);

            // Check Rank and Suit for specific card numbers
            Assert.IsTrue((p1.Suit == Suits.Clubs) && (p1.Rank == Ranks.Three));
            Assert.IsTrue((p2.Suit == Suits.Spades) && (p2.Rank == Ranks.Ace));
            Assert.IsTrue(PlayingCard.GetCardInt(Suits.Clubs, Ranks.Three) == 2);
            Assert.IsTrue(PlayingCard.GetCardInt(Suits.Spades, Ranks.Ace) == 52);

            // Check CardInt values
            Assert.IsTrue(p1.CardInt == 2);
            Assert.IsTrue(p2.CardInt == 52);
            Assert.IsTrue(p3.CardInt == 52);
            Assert.IsTrue(joker1.CardInt == 53);
            Assert.IsTrue(joker2.CardInt == 53);
            Assert.IsTrue(joker3.CardInt == 53);

            // IsValid tests
            Assert.IsTrue(p1.IsValid());
            Assert.IsTrue(p2.IsValid());
            Assert.IsTrue(p3.IsValid());
            Assert.IsTrue(joker1.IsValid());
            Assert.IsTrue(joker2.IsValid());
            Assert.IsTrue(joker3.IsValid());
            Assert.IsFalse(PlayingCard.IsValid(-100));
            Assert.IsFalse(PlayingCard.IsValid(PlayingCard.cardNumMin - 1));
            Assert.IsFalse(PlayingCard.IsValid(PlayingCard.cardNumMax + 1));

            // Comparison Tests
            Assert.IsTrue(p1.CompareTo((object)p2) < 0);
            Assert.IsTrue(p2.CompareTo((object)p3) == 0);
            Assert.IsTrue(p2.CompareTo((object)p1) > 0);
            Assert.IsTrue(p1.CompareTo(p2) < 0);
            Assert.IsTrue(p2.CompareTo(p3) == 0);
            Assert.IsTrue(p2.CompareTo(p1) > 0);
            Assert.IsTrue(p1 < p2);
            Assert.IsTrue(p2 > p1);
            Assert.IsFalse(p1 == p2);
            Assert.IsFalse(p1 > p2);
            Assert.IsFalse(p2 < p1);

            // Equality Tests
            Assert.IsTrue(p2.Equals((PlayingCard)p3));
            Assert.IsTrue(p2.Equals((object)p3));
            Assert.IsTrue(p1 != p3);

            // Joker Tests
            Assert.IsFalse(p1.IsJoker());
            Assert.IsFalse(p2.IsJoker());
            Assert.IsFalse(p3.IsJoker());
            Assert.IsTrue(joker1.IsJoker());
            Assert.IsTrue(joker2.IsJoker());
            Assert.IsTrue(joker3.IsJoker());

            // Random Card Test
            for (int i = 1; i < 100; i++)
                Assert.IsTrue(PlayingCard.MakeRandomCard(true).IsValid());
        }
        [TestMethod]
        public void MakeEveryPossibleCard()
        {
            List<PlayingCard> cardList = new List<PlayingCard>();
            for (int i = PlayingCard.cardNumMin; i <= PlayingCard.cardNumMax; i++)
                cardList.Add(new PlayingCard(i));
            Assert.IsTrue(cardList.Count == PlayingCard.cardNumMax);
        }
        [TestMethod]
        public void TestStaticFunctions()
        {
            for (int i = (int)Suits.Clubs; i <= (int)Suits.Spades; i++)
            {
                for (int j = (int)Ranks.Two; j <= (int)Ranks.Ace; j++)
                {
                    PlayingCard p1 = new PlayingCard((Suits)i, (Ranks)j);
                    PlayingCard p2 = new PlayingCard(p1.CardInt);
                    Assert.IsTrue(p1.IsValid());
                    Assert.IsTrue(p2.IsValid());
                    Assert.IsTrue(p1.Equals(p2));
                    Assert.IsTrue(PlayingCard.Equals(p1, p2));
                    var tuple = p1.SuitAndRank;
                    Assert.IsTrue(p1.Suit == (Suits)i);
                    Assert.IsTrue(tuple.Item1 == (Suits)i);
                    Assert.IsTrue(p1.Rank == (Ranks)j);
                    Assert.IsTrue(tuple.Item2 == (Ranks)j);

                    Assert.IsTrue(p1.CardInt == PlayingCard.GetCardInt((Suits)i, (Ranks)j));
                }
            }
        }
    }
}
