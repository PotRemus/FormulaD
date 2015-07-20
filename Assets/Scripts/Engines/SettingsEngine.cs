using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using FormuleD.Models;
using FormuleD.Models.Contexts;

namespace FormuleD.Engines
{
    public class SettingsEngine : MonoBehaviour
    {
        public static SettingsEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of GameEngine!");
            }
            Instance = this;
        }

        public void SaveGame()
        {

        }

        public void LoadGame(GameSettings game)
        {
            
        }

        public List<GameSettings> GameCandidate()
        {
            List<GameSettings> result = new List<GameSettings>();
            foreach (var game in ContextEngine.Instance.GetGameContexts())
            {
                result.Add(new GameSettings()
                {
                    board = game.mapName,
                    id = game.id,
                    players = game.players.Select(p => p.name).ToList()
                });
            }
            return result;
        }
    }
}