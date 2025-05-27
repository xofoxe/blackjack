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
            return CardCodeMapper.GetCode(this);
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
