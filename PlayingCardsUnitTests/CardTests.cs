using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using jackel.Cards;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class PlayingCardTests
    {
        [TestMethod]
        public void CardNumTooHigh() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(PlayingCard.jokerNum + 1));

        [TestMethod]
        public void CardNumTooLow() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(PlayingCard.cardNumMin - 1));

        [TestMethod]
        public void SuitTooLow() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(Suits.Clubs - 1, Ranks.Two));

        [TestMethod]
        public void SuitTooHigh() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(Suits.Joker + 1, Ranks.Two));

        [TestMethod]
        public void RankTooLow() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(Suits.Clubs, Ranks.Two - 1));

        [TestMethod]
        public void RankTooHigh() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(Suits.Spades, Ranks.Joker + 1));

        [TestMethod]
        public void NegativeCardNum() => Assert.ThrowsException<InvalidOperationException>(() => new PlayingCard(-1));

        [TestMethod]
        public void ShortName()
        {
            PlayingCard twohearts = new(Suits.Hearts, Ranks.Two);
            PlayingCard joker = new(Suits.Joker, Ranks.Joker);
            PlayingCard queenclubs = new(Suits.Clubs, Ranks.Queen);

            Assert.AreEqual("H2", twohearts.ShortName);
            Assert.AreEqual("J", joker.ShortName);
            Assert.AreEqual("CQ", queenclubs.ShortName);
        }

        [TestMethod]
        public void CardColor()
        {
            PlayingCard hearts = new(Suits.Hearts, Ranks.Two);
            PlayingCard diamonds = new(Suits.Diamonds, Ranks.Ten);
            PlayingCard clubs = new(Suits.Clubs, Ranks.Ace);
            PlayingCard spades = new(Suits.Spades, Ranks.Eight);
            PlayingCard joker = new(Suits.Joker, Ranks.Joker);

            Assert.AreEqual(Colors.Red, hearts.Color);
            Assert.AreEqual(Colors.Red, diamonds.Color);
            Assert.AreEqual(Colors.Black, clubs.Color);
            Assert.AreEqual(Colors.Black, spades.Color);
            Assert.AreEqual(Colors.Black, joker.Color);
        }

        [TestMethod]
        public void JSONSerialize()
        {
            var original = new PlayingCard(Suits.Spades, Ranks.Queen);
            var json = original.ToJsonString();
            var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
            int cardInt = (int)jsonObj.CardInt;
            var reconstructed = new PlayingCard(cardInt);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));
            Assert.AreEqual(original.CardInt, cardInt);
            Assert.AreEqual(original.CardInt, reconstructed.CardInt);
            Assert.AreEqual(original.ShortName, reconstructed.ShortName);
            Assert.AreEqual(original.Color, reconstructed.Color);
        }

        [TestMethod]
        public void ComparisonEquality()
        {
            PlayingCard p1 = new(2);
            PlayingCard p2 = new(52);
            PlayingCard p3 = (PlayingCard)p2.Clone();
            PlayingCard joker1 = new(Suits.Joker, Ranks.Two);
            PlayingCard joker2 = new(Suits.Hearts, Ranks.Joker);
            PlayingCard joker3 = new(Suits.Joker, Ranks.Joker);

            Assert.AreNotEqual(p1.CardGUID, p2.CardGUID);
            Assert.AreNotEqual(p2.CardGUID, p3.CardGUID);

            Assert.AreEqual(Suits.Clubs, p1.Suit);
            Assert.AreEqual(Ranks.Three, p1.Rank);
            Assert.AreEqual(Suits.Spades, p2.Suit);
            Assert.AreEqual(Ranks.Ace, p2.Rank);
            Assert.AreEqual(2, PlayingCard.GetCardInt(Suits.Clubs, Ranks.Three));
            Assert.AreEqual(52, PlayingCard.GetCardInt(Suits.Spades, Ranks.Ace));

            Assert.AreEqual(2, p1.CardInt);
            Assert.AreEqual(52, p2.CardInt);
            Assert.AreEqual(52, p3.CardInt);
            Assert.AreEqual(PlayingCard.jokerNum, joker1.CardInt);
            Assert.AreEqual(PlayingCard.jokerNum, joker2.CardInt);
            Assert.AreEqual(PlayingCard.jokerNum, joker3.CardInt);

            Assert.IsTrue(p1.IsValid());
            Assert.IsTrue(p2.IsValid());
            Assert.IsTrue(p3.IsValid());
            Assert.IsTrue(joker1.IsValid());
            Assert.IsTrue(joker2.IsValid());
            Assert.IsTrue(joker3.IsValid());
            Assert.IsFalse(PlayingCard.IsValid(-100));
            Assert.IsFalse(PlayingCard.IsValid(PlayingCard.cardNumMin - 1));
            Assert.IsFalse(PlayingCard.IsValid(PlayingCard.jokerNum + 1));

            Assert.IsTrue(p1.CompareTo((object)p2) < 0);
            Assert.AreEqual(0, p2.CompareTo((object)p3));
            Assert.IsTrue(p2.CompareTo((object)p1) > 0);
            Assert.IsTrue(p1 < p2);
            Assert.IsTrue(p2 > p1);

            Assert.IsTrue(p2.Equals(p3));
            Assert.IsTrue(p2.Equals((object)p3));
            Assert.IsFalse(p1.Equals(p3));
            Assert.IsFalse(p1.Equals(null));

            Assert.IsFalse(p1.IsJoker);
            Assert.IsFalse(p2.IsJoker);
            Assert.IsFalse(p3.IsJoker);
            Assert.IsTrue(joker1.IsJoker);
            Assert.IsTrue(joker2.IsJoker);
            Assert.IsTrue(joker3.IsJoker);
        }

        [TestMethod]
        public void MakeEveryPossibleCard()
        {
            List<PlayingCard> cardList = new();
            for (int i = PlayingCard.cardNumMin; i <= PlayingCard.cardNumMax; i++)
                cardList.Add(new PlayingCard(i));
            Assert.AreEqual(PlayingCard.cardNumMax, cardList.Count);
        }

        [TestMethod]
        public void TestStaticFunctions()
        {
            for (int i = (int)Suits.Clubs; i <= (int)Suits.Spades; i++)
            {
                for (int j = (int)Ranks.Two; j <= (int)Ranks.Ace; j++)
                {
                    PlayingCard p1 = new((Suits)i, (Ranks)j);
                    PlayingCard p2 = new(p1.CardInt);
                    Assert.IsTrue(p1.IsValid());
                    Assert.IsTrue(p2.IsValid());
                    Assert.IsTrue(p1.Equals(p2));
                    Assert.IsTrue(PlayingCard.Equals(p1, p2));
                    var tuple = p1.SuitAndRank;
                    Assert.AreEqual((Suits)i, p1.Suit);
                    Assert.AreEqual((Suits)i, tuple.Item1);
                    Assert.AreEqual((Ranks)j, p1.Rank);
                    Assert.AreEqual((Ranks)j, tuple.Item2);

                    Assert.AreEqual(PlayingCard.GetCardInt((Suits)i, (Ranks)j), p1.CardInt);
                }
            }
        }
    }
}
