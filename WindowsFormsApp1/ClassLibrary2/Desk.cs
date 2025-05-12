namespace ClassLibrary1
{
    using ClassLibrary1.Cards;
    using System;
    using System.Collections.Generic;

    public class Deck
    {
        private List<Card> _cards;
        private int _currentIndex = 0;
        private static readonly Random _random = new Random();


        public int CardsRemaining => _cards.Count;

        public Deck()
        {
            InitializeDeck();
            Shuffle();
        }

        private void InitializeDeck()
        {
            _cards = new List<Card>();
            string[] suits = { "♥", "♦", "♣", "♠" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (var suit in suits)
                foreach (var value in values)
                    _cards.Add(new Card(value, suit));  
        }


        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                var temp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = temp;
            }
            _currentIndex = 0;
        }

        public Card DrawCard()
        {
            if (_currentIndex >= _cards.Count)
            {
                Shuffle();
            }
            return _cards[_currentIndex++];
        }
    }
}
