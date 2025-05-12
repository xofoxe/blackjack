using ClassLibrary1.Cards;
using System;

namespace ClassLibrary1
{
    public class GameManager
    {
        private readonly Deck _deck;
        private readonly ScoreService _scoreService;
        private readonly WinEvaluator _winEvaluator;

        public event Action OnGameStateChanged;
        public event Action<GameResult> OnGameEnded;


        public GameState CurrentState { get; private set; } = GameState.WaitingForBet;
        public GameResult GameResult { get; private set; }


        public Player Player { get; }
        public Player Dealer { get; }

        public GameManager(Deck deck, ScoreService scoreService, WinEvaluator winEvaluator)
        {
            _deck = deck;
            _scoreService = scoreService;
            _winEvaluator = winEvaluator;

            Player = new Player("Player", 2000);
            Dealer = new Player("Dealer", 1000000);
        }

        public void RestoreFromSave(GameSaveData data)
        {
            Player.SetBalance(data.PlayerBalance);
            Dealer.SetBalance(data.DealerBalance);
            Player.PlaceBet(data.CurrentBet);


            Player.ResetHand();
            foreach (var c in data.PlayerHand)
                Player.ReceiveCard(new Card(c.Value, c.Suit));

            Dealer.ResetHand();
            foreach (var c in data.DealerHand)
                Dealer.ReceiveCard(new Card(c.Value, c.Suit));

            if (Enum.TryParse(data.GameState, out GameState state))
                CurrentState = state;

            OnGameStateChanged?.Invoke();
        }

        public void StartGame()
        {
            if (Player.Balance <= 0)
            {
                Player.SetBalance(1000);
            }
            if (CurrentState != GameState.WaitingForBet && CurrentState != GameState.GameOver) return;

            Player.ResetHand();
            Dealer.ResetHand();
            _deck.Shuffle();

            Player.ReceiveCard(_deck.DrawCard());
            Player.ReceiveCard(_deck.DrawCard());

            Dealer.ReceiveCard(_deck.DrawCard());
            Dealer.ReceiveCard(_deck.DrawCard());

            CurrentState = GameState.PlayerTurn;
            OnGameStateChanged?.Invoke();
        }

        public void PlayerHit()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            if (!_scoreService.IsBust(Player))
            {
                Player.ReceiveCard(_deck.DrawCard());
                OnGameStateChanged?.Invoke();

                if (_scoreService.IsBust(Player))
                {
                    CurrentState = GameState.GameOver;
                    FinishGame();
                }
            }
        }

        public void PlayerStand()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            while (_scoreService.CalculateScore(Dealer) < 17)
            {
                Dealer.ReceiveCard(_deck.DrawCard());
            }

            OnGameStateChanged?.Invoke();
            FinishGame();
        }

        public void PlayerDoubleDown()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            if (Player.Hand.Count != 2)
                return;

            Player.DoubleBet();
            Player.ReceiveCard(_deck.DrawCard());

            OnGameStateChanged?.Invoke();

            if (_scoreService.IsBust(Player))
                FinishGame();
            else
                PlayerStand();
        }

        public void PlayerSplit()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            if (Player.Hand.Count == 2 && Player.Hand[0].Value == Player.Hand[1].Value)
            {

            }
        }

        public void HandlePlayerAction(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Hit:
                    PlayerHit();
                    break;
                case PlayerAction.Stand:
                    PlayerStand();
                    break;
                case PlayerAction.Double:
                    PlayerDoubleDown();
                    break;
                case PlayerAction.Split:
                    PlayerSplit();
                    break;
            }
        }
        public int GetPlayerScore()
        {
            return _scoreService.CalculateScore(Player);
        }

        public int GetDealerScore()
        {
            return _scoreService.CalculateScore(Dealer);
        }

        private void FinishGame()
        {
            if (_scoreService.IsBust(Player))
            {
                Player.LoseBet();
                GameResult = GameResult.DealerWin;
            }
            else if (_scoreService.IsBust(Dealer))
            {
                Player.WinBet();
                GameResult = GameResult.PlayerWin;
            }
            else
            {
                var winner = _winEvaluator.GetWinner(Player, Dealer);
                if (winner == "Player")
                {
                    Player.WinBet();
                    GameResult = GameResult.PlayerWin;
                }
                else if (winner == "Dealer")
                {
                    Player.LoseBet();
                    GameResult = GameResult.DealerWin;
                }
                else
                {
                    Player.ReturnBet();
                    GameResult = GameResult.Tie;
                }
            }
            CurrentState = GameState.GameOver;
            OnGameEnded?.Invoke(GameResult);
        }

    }
}
