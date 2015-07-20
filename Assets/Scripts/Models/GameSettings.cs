using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace FormuleD.Models
{
    public class GameSettings
    {
        public Guid id;

        public List<string> players;

        public string board;

        public string preview;

        public DateTime lastTurn;
    }
}