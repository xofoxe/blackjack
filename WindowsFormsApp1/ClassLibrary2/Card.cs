using System;

namespace ClassLibrary1.Cards
{
    public class Card
    {
        public string Suit { get; }
        public string Value { get; }

        public Card(string value, string suit)
        {
            Value = value;
            Suit = suit;
        }
        public string GetCardCode()
        { 
            string suitPrefix;

            switch (Suit)
            {
                case "♠":
                    suitPrefix = "cardSpades";
                    break;
                case "♥":
                    suitPrefix = "cardHearts";
                    break;
                case "♦":
                    suitPrefix = "cardDiamonds";
                    break;
                case "♣":
                    suitPrefix = "cardClubs";
                    break;
                default:
                    throw new ArgumentException($"Unknown suit symbol: {Suit}");
            }

            return $"{suitPrefix}{Value}";
        }

        public override string ToString()
        {
            return $"{Value}{Suit}";
        }
        public string ToShortString()
        {
            return $"{Value}{Suit}";
        }
    }
}
