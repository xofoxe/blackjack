using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace ClassLibrary1
{
    public class GameSaveData
    {
        public List<CardData> PlayerHand { get; set; }
        public List<CardData> DealerHand { get; set; }
        public float PlayerBalance { get; set; }
        public float DealerBalance { get; set; }
        public float CurrentBet { get; set; }
        public string GameState { get; set; }
    }
    public class CardData
    {
        public string Value { get; set; }
        public string Suit { get; set; }
    }

    public class GameSaver
    {
        private const string SavePath = "savegame.json";

        public void Save(GameManager manager)
        {
            var data = new GameSaveData
            {
                PlayerHand = manager.Player.Hand.Select(c => new CardData { Value = c.Value, Suit = c.Suit }).ToList(),
                DealerHand = manager.Dealer.Hand.Select(c => new CardData { Value = c.Value, Suit = c.Suit }).ToList(),
                PlayerBalance = manager.Player.Balance,
                DealerBalance = manager.Dealer.Balance,
                CurrentBet = manager.Player.CurrentBet,
                GameState = manager.CurrentState.ToString()
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(SavePath, json);
        }

        public GameSaveData Load()
        {
            if (!File.Exists(SavePath)) return null;
            var json = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<GameSaveData>(json);
        }
        public void DeleteSave() => File.Delete(SavePath);
    }
}
