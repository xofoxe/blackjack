using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Cards
{
    public static class CardCodeMapper
    {
        private static readonly Dictionary<string, string> _suitPrefixes = new Dictionary<string, string>
        {
            ["♠"] = "cardSpades",
            ["♥"] = "cardHearts",
            ["♦"] = "cardDiamonds",
            ["♣"] = "cardClubs"
        };

        public static string GetCode(Card card)
        {
            if (!_suitPrefixes.TryGetValue(card.Suit, out var prefix))
                throw new ArgumentException($"Unknown suit symbol: {card.Suit}");

            return $"{prefix}{card.Value}";
        }
    }
}
