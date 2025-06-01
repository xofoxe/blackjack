using ClassLibrary1.Cards;
using System;

namespace ClassLibrary1
{
    public class GameManager
    {
        private const int InitialPlayerBalance = 2000;
        private const int InitialDealerBalance = 1000000;
        private const int DealerMinScore = 17;

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

            Player = new Player("Player", InitialPlayerBalance);
            Dealer = new Player("Dealer", InitialDealerBalance);
        }

        public void RestoreFromSave(GameSaveData data)
        {
            Player.SetBalance(data.PlayerBalance);
            Dealer.SetBalance(data.DealerBalance);
            Player.PlaceBet(data.CurrentBet);

            RestoreHandFromSave(Player, data.PlayerHand);
            RestoreHandFromSave(Dealer, data.DealerHand);

            if (Enum.TryParse(data.GameState, out GameState state))
                CurrentState = state;

            OnGameStateChanged?.Invoke();
        }

        public void StartGame()
        {
            if (Player.Balance <= 0)
                Player.SetBalance(InitialPlayerBalance);

            if (!IsInState(GameState.WaitingForBet, GameState.GameOver)) return;

            InitializeHands();
            DealInitialCards();

            CurrentState = GameState.PlayerTurn;
            OnGameStateChanged?.Invoke();
        }

        public void PlayerHit()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            if (!_scoreService.IsBust(Player))
                Player.ReceiveCard(_deck.DrawCard());

            OnGameStateChanged?.Invoke();

            if (_scoreService.IsBust(Player))
                EndGame(GameResult.DealerWin);
        }

        public void PlayerStand()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            while (_scoreService.CalculateScore(Dealer) < DealerMinScore)
                Dealer.ReceiveCard(_deck.DrawCard());

            OnGameStateChanged?.Invoke();
            FinishGame();
        }

        public void PlayerDoubleDown()
        {
            if (CurrentState != GameState.PlayerTurn || Player.Hand.Count != 2)
                return;

            Player.DoubleBet();
            Player.ReceiveCard(_deck.DrawCard());
            OnGameStateChanged?.Invoke();

            if (_scoreService.IsBust(Player))
                EndGame(GameResult.DealerWin);
            else
                PlayerStand();
        }

        public void PlayerSplit()
        {
            if (CurrentState != GameState.PlayerTurn) return;

            if (Player.Hand.Count == 2 && Player.Hand[0].Value == Player.Hand[1].Value)
            {
                // Реалізація спліту (опціонально)
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

        public int GetPlayerScore() => _scoreService.CalculateScore(Player);
        public int GetDealerScore() => _scoreService.CalculateScore(Dealer);

        // ─────────────────────────────────────────
        // Внутрішні допоміжні методи

        private void InitializeHands()
        {
            Player.ResetHand();
            Dealer.ResetHand();
            _deck.Shuffle();
        }

        private void DealInitialCards()
        {
            Player.ReceiveCard(_deck.DrawCard());
            Player.ReceiveCard(_deck.DrawCard());

            Dealer.ReceiveCard(_deck.DrawCard());
            Dealer.ReceiveCard(_deck.DrawCard());
        }

        private void RestoreHandFromSave(Player player, CardData[] savedCards)
        {
            player.ResetHand();
            foreach (var c in savedCards)
                player.ReceiveCard(new Card(c.Value, c.Suit));
        }

        private void EndGame(GameResult result)
        {
            GameResult = result;
            CurrentState = GameState.GameOver;
            Player.UpdateBalance(result);
            OnGameEnded?.Invoke(result);
        }

        private void FinishGame()
        {
            if (_scoreService.IsBust(Player))
                EndGame(GameResult.DealerWin);
            else if (_scoreService.IsBust(Dealer))
                EndGame(GameResult.PlayerWin);
            else
                DetermineWinner();
        }

        private void DetermineWinner()
        {
            var winner = _winEvaluator.GetWinner(Player, Dealer);
            if (winner == "Player")
                EndGame(GameResult.PlayerWin);
            else if (winner == "Dealer")
                EndGame(GameResult.DealerWin);
            else
                EndGame(GameResult.Tie);
        }

        private bool IsInState(params GameState[] states) => Array.Exists(states, s => s == CurrentState);
    }
}
