using UnityEngine;
using System.Collections;
using FormuleD.Engines;
using System.Collections.Generic;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course
{
    public class EndGameManager : MonoBehaviour
    {
        public RectTransform playersPanelContent;
        public RectTransform playerPrefab;

        private List<RectTransform> _players;

        public void OnOpen(List<PlayerContext> players)
        {
            RaceEngine.Instance.MouseEnterGUI();
            this.EmptyPlayers();
            this.LoadPlayers(players);
        }

        private void LoadPlayers(List<PlayerContext> players)
        {
            playersPanelContent.sizeDelta = new Vector2(600, players.Count * 50);
            int index = 0;
            foreach (var player in players)
            {
                var playerTransform = Object.Instantiate(playerPrefab).GetComponent<RectTransform>();
                playerTransform.SetParent(playersPanelContent);
                playerTransform.localScale = Vector3.one;
                _players.Add(playerTransform);
                var script = playerTransform.GetComponent<EndGamePlayerManager>();
                script.LoadPlayer(player, index);
                index++;
            }
        }

        private void EmptyPlayers()
        {
            if (_players != null && _players.Count > 0)
            {
                foreach (var player in _players)
                {
                    Object.DestroyImmediate(player.gameObject);
                }
            }
            _players = new List<RectTransform>();
            playersPanelContent.sizeDelta = new Vector2(460, 0);
        }

        public void OnViewGame()
        {
        }

        public void OnRestartGame()
        {
        }
    }
}