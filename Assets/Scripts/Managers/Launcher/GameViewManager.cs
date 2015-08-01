using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models;
using FormuleD.Engines;
using System;

namespace FormuleD.Managers.Launcher
{
    public class GameViewManager : MonoBehaviour
    {
        public Image image;
        public Text mapTitle;
        public Text subTitle;
        public Sprite imageMap1;
        public Sprite imageMap2;
        public PlayerViewManager[] players;

        private string _gameId;

        public void LoadGame(GameSettings game)
        {
            _gameId = game.id;
            mapTitle.text = string.Concat(game.board, " (", game.id, ")");
            if (game.board == "Map1")
            {
                image.sprite = imageMap1;
            }
            else if (game.board == "Map2")
            {
                image.sprite = imageMap2;
            }
            var duration = DateTime.Now - game.lastTurn;
            if (duration.TotalDays >= 1)
            {
                subTitle.text = string.Format(ResourceEngine.Instance.GetResource("LauncherSubTitleDay"), (int)duration.TotalDays);
            }
            else if (duration.TotalHours >= 1)
            {
                subTitle.text = string.Format(ResourceEngine.Instance.GetResource("LauncherSubTitleHour"), (int)duration.TotalHours);
            }
            else if (duration.TotalMinutes >= 1)
            {
                subTitle.text = string.Format(ResourceEngine.Instance.GetResource("LauncherSubTitleMinute"), (int)duration.TotalMinutes);
            }
            else
            {
                subTitle.text = string.Format(ResourceEngine.Instance.GetResource("LauncherSubTitleSecond"), (int)duration.TotalSeconds);
            }
            int index = 0;
            foreach (var player in players)
            {
                PlayerSettings gamePlayer = null;
                if (game.players.Count > index)
                {
                    gamePlayer = game.players[index];
                }
                player.LoadPlayer(gamePlayer, game.type, index);
                index++;
            }

        }

        public void OnSelectGame()
        {
            ContextEngine.Instance.LoadContext(_gameId);
            UnityEngine.Object.DontDestroyOnLoad(ContextEngine.Instance.gameObject);
            Application.LoadLevel("Race");
        }
    }
}