using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using FormuleD.Models.Contexts;

namespace FormuleD.Models
{
    public class GameSettings
    {
        public string id;

        public List<PlayerSettings> players;

        public string board;

        public string preview;

        public DateTime lastTurn;

        public GameType type;

        public GameStateType state;
    }

    public class PlayerSettings
    {
        public string name;
        public string id;
        public bool isCurrent;
        public bool isDead;
    }
}