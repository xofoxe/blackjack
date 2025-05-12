using System.Collections.Generic;

namespace ClassLibrary1
{
    public class ScoreService
    {
        private readonly Dictionary<string, int> _cardValues = new Dictionary<string, int>()
{
    { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
    { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 },
    { "10", 10 }, { "J", 10 }, { "Q", 10 }, { "K", 10 }, { "A", 11 }
};
        
        public int CalculateScore(Player player)
        {
            int total = 0;
            int aceCount = 0;

            foreach (var card in player.Hand)
            {
                total += _cardValues[card.Value];
                if (card.Value == "A") aceCount++;
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }

        public bool IsBust(Player player)
        {
            return CalculateScore(player) > 21;
        }
    }
}
