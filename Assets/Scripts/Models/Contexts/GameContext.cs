using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models.Board;
using System;

namespace FormuleD.Models.Contexts
{
    [Serializable]
    public class GameContext
    {
        public Guid id;
        public string mapName = "Map1";
        public int lap;
        public List<PlayerContext> players = new List<PlayerContext>();
        public Color[] colorDes;
        public List<IndexDataSource> dangerousCases = new List<IndexDataSource>();
        public int totalLap;
    }
}