using ClassLibrary1.Cards;
using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class GameManager
    {

        private const int InitialPlayerBalance = 2000;
        private const int InitialDealerBalance = 1000000;
        private const int PlayerBankruptcyResetBalance = 1000;
        private const int RequiredCardsToDouble = 2;
        private const int DealerStandScore = 17;

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

            RestoreHand(Player, data.PlayerHand);
            RestoreHand(Dealer, data.DealerHand);

            if (Enum.TryParse(data.GameState, out GameState state))
                CurrentState = state;

            OnGameStateChanged?.Invoke();
        }

        private void RestoreHand(Player player, List<CardData> hand)
        {
            player.ResetHand();
            foreach (var card in hand)
                player.ReceiveCard(new Card(card.Value, card.Suit));
        }

        public void StartGame()
        {
            if (Player.Balance <= 0)
                Player.SetBalance(PlayerBankruptcyResetBalance);

            if (!IsReadyToStart()) return;

            Player.ResetHand();
            Dealer.ResetHand();
            _deck.Shuffle();

            DealInitialCards(Player);
            DealInitialCards(Dealer);

            CurrentState = GameState.PlayerTurn;
            OnGameStateChanged?.Invoke();
        }

        private void DealInitialCards(Player player)
        {
            player.ReceiveCard(_deck.DrawCard());
            player.ReceiveCard(_deck.DrawCard());
        }

        private bool IsReadyToStart() =>
            CurrentState == GameState.WaitingForBet || CurrentState == GameState.GameOver;

        private bool IsPlayerTurn() => CurrentState == GameState.PlayerTurn;

        public void PlayerHit()
        {
            if (!IsPlayerTurn() || _scoreService.IsBust(Player)) return;

            Player.ReceiveCard(_deck.DrawCard());
            OnGameStateChanged?.Invoke();

            if (_scoreService.IsBust(Player))
            {
                CurrentState = GameState.GameOver;
                FinishGame();
            }
        }

        public void PlayerStand()
        {
            if (!IsPlayerTurn()) return;

            while (_scoreService.CalculateScore(Dealer) < DealerStandScore)
            {
                Dealer.ReceiveCard(_deck.DrawCard());
            }

            OnGameStateChanged?.Invoke();
            FinishGame();
        }

        public void PlayerDoubleDown()
        {
            if (!IsPlayerTurn() || Player.Hand.Count != RequiredCardsToDouble) return;

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
            }
        }

        public int GetPlayerScore() => _scoreService.CalculateScore(Player);
        public int GetDealerScore() => _scoreService.CalculateScore(Dealer);

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
                switch (_winEvaluator.GetWinner(Player, Dealer))
                {
                    case "Player":
                        Player.WinBet();
                        GameResult = GameResult.PlayerWin;
                        break;
                    case "Dealer":
                        Player.LoseBet();
                        GameResult = GameResult.DealerWin;
                        break;
                    default:
                        Player.ReturnBet();
                        GameResult = GameResult.Tie;
                        break;
                }
            }

            CurrentState = GameState.GameOver;
            OnGameEnded?.Invoke(GameResult);
        }
    }
}