using ClassLibrary1.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2
{
    public class CardSpriteMapper
    {
        public string GetCardCode(Card card)
        {
            string suitPrefix;
            switch (card.Suit)
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
                    throw new ArgumentException($"Unknown suit: {card.Suit}");
            }


            return $"{suitPrefix}{card.Value}";
        }
    }

}
