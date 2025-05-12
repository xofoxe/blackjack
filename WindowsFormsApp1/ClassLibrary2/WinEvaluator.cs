using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class WinEvaluator
    {
        private readonly ScoreService _scoreService;
        public WinEvaluator(ScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        public string GetWinner(Player player, Player dealer)
        {
            int playerScore = _scoreService.CalculateScore(player);
            int dealerScore = _scoreService.CalculateScore(dealer);

            if (_scoreService.IsBust(player)) return "Dealer";
            if (_scoreService.IsBust(dealer)) return "Player";

            if (playerScore > dealerScore) return "Player";
            if (dealerScore > playerScore) return "Dealer";
            return "Tie";
        }
    }
} 
