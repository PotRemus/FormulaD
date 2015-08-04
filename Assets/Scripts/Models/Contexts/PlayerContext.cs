using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using FormuleD.Models.Board;

namespace FormuleD.Models.Contexts
{
    [Serializable]
    public class PlayerContext
    {
        public PlayerContext()
        {
            features = new FeatureContext();
            turnHistories = new List<HistoryContext>();
        }

        public string name;
        public int position;
        public int index;
        public PlayerColor color;
        public int lap = -1;
        public string lastBend;
        public int stopBend;
        public PlayerStateType state;
        public FeatureContext features;
        public HistoryContext currentTurn;
        public List<HistoryContext> turnHistories;
        public QualificationPlayerContext qualification;

        public bool IsPlayable()
        {
            if (state != PlayerStateType.Finish && state != PlayerStateType.Dead)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Color GetColor()
        {
            Color result = Color.black;
            switch (color)
            {
                case PlayerColor.bleu:
                    result = Config.PlayerColor.bleu;
                    break;
                case PlayerColor.yellow:
                    result = Config.PlayerColor.yellow;
                    break;
                case PlayerColor.green:
                    result = Config.PlayerColor.green;
                    break;
                case PlayerColor.orange:
                    result = Config.PlayerColor.orange;
                    break;
                case PlayerColor.purple:
                    result = Config.PlayerColor.purple;
                    break;
            }
            return result;
        }
    }
    public enum PlayerColor
    {
        bleu,
        yellow,
        green,
        purple,
        orange
    }

    public enum PlayerStateType
    {
        Waiting,
        RollDice,
        ChoseRoute,
        Aspiration,
        StandOut,
        EndTurn,
        Dead,
        Finish
    }
}