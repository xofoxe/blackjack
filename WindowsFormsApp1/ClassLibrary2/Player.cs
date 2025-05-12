using ClassLibrary1.Cards;
using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class Player
    {
        private readonly List<Card> _hand = new List<Card> { };
        private readonly List<List<Card>> _hands = new List<List<Card>>(); 

        public List<Card> Hand => _hands.Count > 0 ? _hands[0] : _hand; 
        public List<List<Card>> Hands => _hands;  

        public string Name { get; }
        public float Balance { get; private set; }
        public float CurrentBet { get; private set; }
        public bool PlaceBet(float betAmount)
        {
            if (betAmount <= Balance && betAmount > 0)
            {
                CurrentBet = betAmount;
                Balance -= betAmount;
                return true;
            }
            return false;
        }
        public void SetBalance(float amount)
        {
            Balance = amount;
        }
        public float WinBet()
        {
            float amount = CurrentBet * 2;
            Balance += amount;
            return amount;
        }
        public void DoubleBet()
        {
            if (Balance < CurrentBet)
                throw new InvalidOperationException("Not enough balance to double down.");

            Balance -= CurrentBet;
            CurrentBet *= 2;
        }
        public void LoseBet()
        {
            CurrentBet = 0;
        }
        public Player(string name, int balance)
        {
            Name = name;
            Balance = balance;
            _hands.Add(new List<Card>());
        }

        public void ReceiveCard(Card card)
        {
            Hands[0].Add(card); 
        }

        public void ResetHand()
        {
            _hands.Clear(); 
            _hands.Add(new List<Card>()); 
        }

        public void SplitHand()
        {
            if (_hands[0].Count == 2 && _hands[0][0].Value == _hands[0][1].Value)
            {
                var newHand = new List<Card> { _hands[0][1] };
                _hands[0].RemoveAt(1);
                _hands.Add(newHand); 
            }
        }

        public void ReturnBet()
        {
            Balance += CurrentBet;
            CurrentBet = 0;
        }
    }
}
