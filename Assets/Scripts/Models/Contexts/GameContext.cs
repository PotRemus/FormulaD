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
        public string id;
        public GameType type;
        public DateTime lastTurn;
        public string mapName = "Map1";
        public string mapPreview = @"Assets\Resources\Maps\Image\Map1.png";
        public int lap;
        public List<PlayerContext> players = new List<PlayerContext>();
        public Color[] colorDes;
        public List<IndexDataSource> dangerousCases = new List<IndexDataSource>();
        public int totalLap;
    }

    public enum GameType
    {
        Local,
        Online,
        Completed
    }
}