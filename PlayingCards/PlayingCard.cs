using System;
using System.Runtime.Serialization;
using System.Xml;

namespace jackel.Cards
{
    public enum Suits { Clubs = 1, Diamonds, Hearts, Spades, Joker };
    public enum Ranks { Two = 1, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace, Joker };
    public enum Colors { Red, Black };

    [DataContract]
    public class PlayingCard : ICloneable, IEquatable<PlayingCard>, IComparable<PlayingCard>, IComparable
    {
        public const int cardNumMin = 1;
        public const int cardNumMax = 52;
        public const int jokerNum = 53;
        static Random rand = new Random();

        [DataMember]
        private int cardNumber;
        [DataMember]
        public readonly Guid CardGUID = Guid.NewGuid();
        [DataMember]
        public bool faceUp = false;

        public static PlayingCard MakeRandomCard(bool includeJokers = false)
        {
            int number;
            if (includeJokers)
                number = rand.Next(cardNumMin, jokerNum + 1); // exclusive upper bound
            else
                number = rand.Next(cardNumMin, cardNumMax + 1); // exclusive upper bound
            return new PlayingCard(number);
        }

        public static bool IsValid(int cardNum) => ((cardNum >= cardNumMin) && (cardNum <= jokerNum)) ? true : false;
        public int CardInt => cardNumber;
        public int RankAsInt => (int)Rank + 1;

        /// <summary>
        /// Volatile - Assumes cardInt, Suits enum, and Ranks enum all start at Integer value 1
        /// </summary>
        /// <param name="suit">s == Suits enum</param>
        /// <param name="rank">r == Ranks enum</param>
        /// <returns>cardInt (integer)</returns>
        public static int GetCardInt(Suits suit, Ranks rank)
        {
            if ((suit == Suits.Joker) || (rank == Ranks.Joker))
                return jokerNum;
            else
                return ((((int)suit - 1) * 13) + (int)rank);
        }
        public Tuple<Suits, Ranks> SuitAndRank => new Tuple<Suits, Ranks>(Suit, Rank);

        public bool IsValid() => IsValid(cardNumber);

        /// <summary>
        /// Volatile - Assumes cardInt, Suits enum, and Ranks enum all start at Integer value 1
        /// </summary>
        public Ranks Rank
        {
            get
            {
                if (!IsValid(CardInt))
                    throw new InvalidOperationException($"GetCardRank: Invalid cardInt value {CardInt}");
                if (CardInt == jokerNum)
                    return Ranks.Joker;
                else
                    return (Ranks)((CardInt - 1) % 13) + 1;
            }
        }
        /// <summary>
        /// Volatile - Assumes cardInt, Suits enum, and Ranks enum all start at Integer value 1
        /// </summary>
        public Suits Suit
        {
            get
            {
                if (!IsValid(CardInt))
                    throw new InvalidOperationException($"GetCardSuit: Invalid cardInt value {CardInt}");
                if (CardInt == jokerNum)
                    return Suits.Joker;
                else
                    return (Suits)((CardInt - 1) / 13) + 1;
            }
        }
        public PlayingCard(int cardNum)
        {
            if (!IsValid(cardNum))
                throw new InvalidOperationException($"PlayingCard: Constructor cannot create invalid card #{cardNum}");
            else
            {
                cardNumber = cardNum;
            }
        }
        public PlayingCard(Suits suit, Ranks rank) : this(GetCardInt(suit, rank))
        { }
        private PlayingCard() { } // no default constructor
        public void Deconstruct(out Suits s, out Ranks r)
        {
            s = Suit;
            r = Rank;
        }
        public Colors Color => ((Suit == Suits.Diamonds) || (Suit == Suits.Hearts)) ? Colors.Red : Colors.Black;
        public bool IsJoker() => (cardNumber == jokerNum);
        public void ToXmlFile(string filename)
        {
            var ds = new DataContractSerializer(typeof(PlayingCard));
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter w = XmlWriter.Create(filename, settings))
                ds.WriteObject(w, this);
        }
        public string LongName => string.Format("     {0,2}({1,2}): {2,17}, {3,5}, {4,9}, {5}", ShortName, cardNumber, Rank.ToString() + " of " + Suit.ToString(), Color.ToString(), faceUp ? "Face Up" : "Face Down", CardGUID);
        public override string ToString() => LongName;
        public string ShortName
        {
            get
            {
                if (IsJoker())
                    return "J";
                else if ((int)Rank < (int)Ranks.Ten)
                    return Suit.ToString().Substring(0, 1) + (RankAsInt).ToString(); // "H2" == Two of Hearts
                else
                    return Suit.ToString().Substring(0, 1) + Rank.ToString().Substring(0, 1); // "HT" == Ten of Hearts, "HA" == Ace of Hearts
            }
        }
        public object Clone() => new PlayingCard(cardNumber);
        public override bool Equals(object obj)
        {
            if (!(obj is PlayingCard)) return false;
            return Equals((PlayingCard)obj);
        }
        public bool Equals(PlayingCard other)
        {
            if (other == null) return false;
            return (this.cardNumber == other.cardNumber); // comparing Suit and Rank
        }
        public int CompareTo(PlayingCard other) => (cardNumber - other.cardNumber);
        public int CompareTo(object obj)
        {
            if (!(obj is PlayingCard)) throw new InvalidOperationException("CompareTo: Not a PlayingCard");
            return CompareTo((PlayingCard)obj);
        }
        public static bool operator >(PlayingCard p1, PlayingCard p2) => p1.cardNumber > p2.cardNumber;
        public static bool operator <(PlayingCard p1, PlayingCard p2) => p1.cardNumber < p2.cardNumber;
        public override int GetHashCode() => cardNumber;
    }
}
