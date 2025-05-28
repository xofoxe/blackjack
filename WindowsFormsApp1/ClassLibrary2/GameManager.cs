using ClassLibrary1.Cards;
using System;

namespace ClassLibrary1
{
    public class GameManager
    {
        private const int PLAYER_INITIAL_BALANCE = 2000;
        private const int DEALER_INITIAL_BALANCE = 1000000;
        private const int PLAYER_BALANCE_IF_OVER = 1000;
        private const int DEALER_MIN_STAND = 17;

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

            Player = new Player("Player", PLAYER_INITIAL_BALANCE);
            Dealer = new Player("Dealer", DEALER_INITIAL_BALANCE);
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
                Player.SetBalance(PLAYER_BALANCE_IF_OVER);
            }
            if (CurrentState != GameState.WaitingForBet && CurrentState != GameState.GameOver) return;

            _deck.Shuffle();

            ResetPlayerHand(Player);
            ResetPlayerHand(Dealer);

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
                    FinishGame();
                }
            }
        }

        public void PlayerStand()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            while (_scoreService.CalculateScore(Dealer) < DEALER_MIN_STAND)
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
                PlayerLose();
            }
            else if (_scoreService.IsBust(Dealer))
            {
                PlayerWin();
            }
            else
            {
                switch (_winEvaluator.GetWinner(Player, Dealer))
                {
                    case "Player":
                        PlayerWin();
                        break;
                    case "Dealer":
                        PlayerLose();
                        break;
                    default:
                        PlayerTie();
                        break;
                }
            }
            CurrentState = GameState.GameOver;
            OnGameEnded?.Invoke(GameResult);
        }

        private void ResetPlayerHand(Player player)
        {
            player.ResetHand();

            player.ReceiveCard(_deck.DrawCard());
            player.ReceiveCard(_deck.DrawCard());
        }

        private void PlayerWin()
        {
            Player.WinBet();
            GameResult = GameResult.PlayerWin;
        }

        private void PlayerLose()
        {
            Player.LoseBet();
            GameResult = GameResult.DealerWin;
        }

        private void PlayerTie()
        {
            Player.ReturnBet();
            GameResult = GameResult.Tie;
        }

    }
}
