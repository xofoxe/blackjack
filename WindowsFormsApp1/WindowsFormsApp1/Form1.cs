using ClassLibrary1;
using ClassLibrary1.Cards;
using ClassLibrary2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private const int CardOffsetX = 25;
        private const int CardOffsetY = 10;
        private const int PlayerBaseY = 170;
        private const int DealerBaseY = 50;
        private const int BaseX = 150;

        private GameManager _gameManager;
        private CardAnimator _cardAnimator;
        private GameSaver _gameSaver;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
            RegisterGameManagerEvents();
            this.FormClosing += Form1_FormClosing;
        }

        private void InitializeGame()
        {
            try
            {
                Deck deck = new Deck();
                ScoreService scoreService = new ScoreService();
                WinEvaluator winEvaluator = new WinEvaluator(scoreService);

                _gameManager = new GameManager(deck, scoreService, winEvaluator);
                _cardAnimator = new CardAnimator(GamePanel);
                _gameSaver = new GameSaver();

                var savedData = _gameSaver.Load();
                if (savedData != null)
                {
                    _gameManager.RestoreFromSave(savedData);
                    RedrawHands();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося завантажити гру: " + ex.Message);
            }
        }

        private void RedrawHands()
        {
            ClearCardsFromGamePanel();
            AnimateHand(_gameManager.Player.Hand, false);
            AnimateHand(_gameManager.Dealer.Hand, true);
        }

        private void ClearCardsFromGamePanel()
        {
            foreach (var card in GamePanel.Controls.OfType<SpriteCard>().ToList())
            {
                card.Dispose();
                GamePanel.Controls.Remove(card);
            }
        }

        private void AnimateHand(List<Card> hand, bool toDealer)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                AnimateCardFromDeck(hand[i], i, toDealer);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _gameSaver.Save(_gameManager);
        }

        private void RegisterGameManagerEvents()
        {
            _gameManager.OnGameStateChanged += UpdateUI;
            _gameManager.OnGameEnded += HandleGameEnd;
        }

        private void UpdateUI()
        {
            labelBalance.Text = _gameManager.Player.Balance.ToString();
            labelCurrentBet.Text = _gameManager.Player.CurrentBet.ToString();
            labelDealerScore.Text = _gameManager.GetDealerScore().ToString();
            labelPlayerScore.Text = _gameManager.GetPlayerScore().ToString();
        }

        private void HandleGameEnd(GameResult result)
        {
            if (result == GameResult.DealerWin)
            {
                labelPlayerScore.ForeColor = Color.Red;
            }
            else
            {
                labelWonAmount.Text = (_gameManager.Player.CurrentBet * 2f).ToString();
                panelWinDesk.Visible = true;
                panelWinDesk.BringToFront();
            }
        }

        private void buttonHit_Click(object sender, EventArgs e)
        {
            _gameManager.HandlePlayerAction(PlayerAction.Hit);
            if (_gameManager.CurrentState != GameState.WaitingForBet && _gameManager.CurrentState != GameState.GameOver)
                AnimateNextCard(_gameManager.Player.Hand, false);
        }

        private void buttonStand_Click(object sender, EventArgs e)
        {
            _gameManager.HandlePlayerAction(PlayerAction.Stand);
            if (_gameManager.CurrentState != GameState.WaitingForBet)
                AnimateNextCard(_gameManager.Dealer.Hand, true);
        }

        private void buttonDouble_Click(object sender, EventArgs e)
        {
            _gameManager.HandlePlayerAction(PlayerAction.Double);
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            _gameManager.HandlePlayerAction(PlayerAction.Split);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            ResetGamePanel();
            if (TryPlaceBet(out int betAmount))
            {
                _gameManager.StartGame();
                AnimateHand(_gameManager.Dealer.Hand, true);
                AnimateHand(_gameManager.Player.Hand, false);
                labelPlayerScore.ForeColor = Color.Black;
                panelWinDesk.Visible = false;
            }
        }

        private void ResetGamePanel()
        {
            GamePanel.Controls.OfType<SpriteCard>().ToList().ForEach(card => GamePanel.Controls.Remove(card));
        }

        private bool TryPlaceBet(out int betAmount)
        {
            betAmount = 0;
            if (int.TryParse(textBoxAmount.Text, out betAmount))
            {
                if (!_gameManager.Player.PlaceBet(betAmount))
                {
                    MessageBox.Show("Invalid bet amount or insufficient funds.");
                    return false;
                }
                return true;
            }

            MessageBox.Show("Please enter a valid bet amount.");
            return false;
        }

        private void AnimateNextCard(List<Card> hand, bool toDealer)
        {
            int index = hand.Count - 1;
            if (index >= 0)
            {
                AnimateCardFromDeck(hand[index], index, toDealer);
            }
        }

        private void AnimateCardFromDeck(Card card, int index, bool toDealer)
        {
            Point start = DeskPictureBox.PointToScreen(new Point(DeskPictureBox.Width / 2, DeskPictureBox.Height / 2));
            start = GamePanel.PointToClient(start);

            Point end = GetTargetCardPosition(index, toDealer);
            string spriteCode = card.GetCardCode();

            _cardAnimator.AnimateCard(start, end, spriteCode, true);
        }

        private Point GetTargetCardPosition(int index, bool toDealer)
        {
            int baseY = toDealer ? DealerBaseY : GamePanel.Height - PlayerBaseY;
            return new Point(BaseX + index * CardOffsetX, baseY + index * CardOffsetY);
        }

        private void ShowResult(string result)
        {
            MessageBox.Show(result);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}

