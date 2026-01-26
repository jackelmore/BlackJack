using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace jackel.Cards
{
    public enum Suits { Clubs = 1, Diamonds, Hearts, Spades, Joker };
    public enum Ranks { Two = 1, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace, Joker };
    public enum Colors { Red, Black };

    [JsonObject(MemberSerialization.OptIn)]
    public class PlayingCard : ICloneable, IEquatable<PlayingCard>, IComparable<PlayingCard>, IComparable
    {
        public const int cardNumMin = 1;
        public const int cardNumMax = 52;
        public const int jokerNum = 53;
        private static readonly Random rand = new Random();
        private readonly int cardNumber;

        [JsonProperty(Order=10)]
        public readonly Guid CardGUID = Guid.NewGuid();
        [JsonProperty(Order=5)]
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

        public static bool IsValid(int cardNum)
        {
            bool v = ((cardNum >= cardNumMin) && (cardNum <= jokerNum));
            return v;
        }
        public bool IsValid() => IsValid(cardNumber);
        [JsonProperty(Order=8)]
        public int CardInt => cardNumber;
        [JsonProperty(Order=6)]
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
            {
                int v = ((((int)suit - 1) * 13) + (int)rank);
                return v;
            }
        }
        public Tuple<Suits, Ranks> SuitAndRank => new Tuple<Suits, Ranks>(Suit, Rank);

        /// <summary>
        /// Volatile - Assumes cardInt, Suits enum, and Ranks enum all start at Integer value 1
        /// </summary>
        [JsonProperty(Order=1), JsonConverter(typeof(StringEnumConverter))]
        public Ranks Rank
        {
            get
            {
                if (!IsValid(CardInt))
                    throw new InvalidOperationException($"GetCardRank: Invalid cardInt value {CardInt}, GUID {CardGUID}");
                if (CardInt == jokerNum)
                    return Ranks.Joker;
                else
                    return (Ranks)((CardInt - 1) % 13) + 1;
            }
        }
        /// <summary>
        /// Volatile - Assumes cardInt, Suits enum, and Ranks enum all start at Integer value 1
        /// </summary>
        [JsonProperty(Order=2), JsonConverter(typeof(StringEnumConverter))]
        public Suits Suit
        {
            get
            {
                if (!IsValid(CardInt))
                    throw new InvalidOperationException($"GetCardSuit: Invalid cardInt value {CardInt}, GUID {CardGUID}");
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
        public PlayingCard() : this(Suits.Joker, Ranks.Joker)
        { }
        public void Deconstruct(out Suits s, out Ranks r)
        {
            s = Suit;
            r = Rank;
        }
        [JsonProperty(Order=3), JsonConverter(typeof(StringEnumConverter))]
        public Colors Color => ((Suit == Suits.Diamonds) || (Suit == Suits.Hearts)) ? Colors.Red : Colors.Black;
        [JsonProperty(Order=4)]
        public bool IsJoker => (cardNumber == jokerNum);
        [JsonProperty(Order=9)]
        public string LongName => string.Format("     {0,2}({1,2}): {2,17}, {3,5}, {4,9}, {5}", ShortName, cardNumber, Rank.ToString() + " of " + Suit.ToString(), Color.ToString(), faceUp ? "Face Up" : "Face Down", CardGUID);
        public override string ToString() => LongName;
        public string ToJsonString() => JsonConvert.SerializeObject(this);
        public string ToXmlString() => JsonConvert.DeserializeXNode(this.ToJsonString(), nameof(PlayingCard)).ToString();

        [JsonProperty(Order=7)]
        public string ShortName
        {
            get
            {
                if (IsJoker)
                    return "J";
                else if ((int)Rank <= (int)Ranks.Nine)
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
            if (!(obj is PlayingCard))
                throw new InvalidOperationException("CompareTo: Not a PlayingCard");
            return CompareTo((PlayingCard)obj);
        }
        public static bool operator >(PlayingCard p1, PlayingCard p2) => p1.cardNumber > p2.cardNumber;
        public static bool operator <(PlayingCard p1, PlayingCard p2) => p1.cardNumber < p2.cardNumber;
        public override int GetHashCode() => cardNumber;
    }
}
