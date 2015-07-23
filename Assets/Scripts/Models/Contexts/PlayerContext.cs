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
        public string name;
        public int position;
        public PlayerColor color;
        public int lap = -1;
        public string lastBend;
        public int stopBend;
        public PlayerStateType state;
        public FeatureContext features;
        public HistoryContext currentTurn;
        public List<HistoryContext> turnHistories;

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
            }
            return result;
        }

        public IndexDataSource GetLastIndex()
        {
            IndexDataSource result = null;
            HistoryContext history = null;
            if (currentTurn != null && currentTurn.path.Any())
            {
                history = currentTurn;
            }
            else
            {
                history = turnHistories.Last();
            }

            if (history != null)
            {
                if (history.aspirations != null && history.aspirations.Any())
                {
                    result = history.aspirations.Last().Last();
                }
                else
                {
                    result = history.path.Last();
                }
            }
            return result;
        }
    }

    public enum PlayerColor
    {
        bleu,
        yellow,
        green,
    }

    public enum PlayerStateType
    {
        Waiting,
        RollDice,
        ChoseRoute,
        Aspiration,
        EndTurn,
        Dead,
        Finish
    }
}