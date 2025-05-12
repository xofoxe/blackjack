using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
     
    public enum PlayerAction
    {
        Hit,
        Stand,
        Double,
        Split
    }
    public enum GameResult
    {
        PlayerWin,
        DealerWin,
        Tie,
        PlayerBust,
        DealerBust
    }

    public enum GameState
    {
        NotStarted,
        WaitingForBet,
        Tie,
        PlayerTurn,
        DealerTurn,
        GameOver,
        DealerWin,
        PlayerWin
    }

}
