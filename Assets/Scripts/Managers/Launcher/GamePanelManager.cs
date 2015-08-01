using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models;

namespace FormuleD.Managers.Launcher
{
    public class GamePanelManager : MonoBehaviour
    {
        public RectTransform titleContainer;
        public RectTransform gameContainer;

        private List<RectTransform> _loadedGames;

        public void LoadGames(List<GameSettings> games, Transform gameTemplate, float offsetHeight)
        {
            if (_loadedGames != null && _loadedGames.Count > 0)
            {
                foreach (var loadedGame in _loadedGames)
                {
                    Object.DestroyImmediate(loadedGame.gameObject);
                }
            }
            _loadedGames = new List<RectTransform>();

            if (games.Count > 0)
            {
                titleContainer.sizeDelta = new Vector2(titleContainer.sizeDelta.x, 30);
            }
            else
            {
                titleContainer.sizeDelta = new Vector2(titleContainer.sizeDelta.x, 0);
            }

            gameContainer.sizeDelta = new Vector2(gameContainer.sizeDelta.x, 150 * games.Count);
            foreach (var game in games)
            {
                RectTransform gameTransform = Object.Instantiate(gameTemplate).GetComponent<RectTransform>();
                _loadedGames.Add(gameTransform);
                gameTransform.SetParent(gameContainer);
                gameTransform.localScale = new Vector3(1, 1, 1);
                var gameViewerManager = gameTransform.GetComponent<GameViewManager>();
                gameViewerManager.LoadGame(game);
            }

            var currentContainer = this.GetComponent<RectTransform>();
            currentContainer.sizeDelta = new Vector2(gameContainer.sizeDelta.x, titleContainer.sizeDelta.y + gameContainer.sizeDelta.y);
            currentContainer.anchoredPosition = new Vector2(currentContainer.anchoredPosition.x, -1 * offsetHeight);
        }
    }
}