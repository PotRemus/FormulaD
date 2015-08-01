using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models;

namespace FormuleD.Managers.Launcher
{
    public class OnlinePanelManager : MonoBehaviour
    {
        public RectTransform titleContainer;
        public RectTransform gameContainer;

        public RectTransform waintingTitleContainer;
        public RectTransform waintingGameContainer;

        private List<RectTransform> _loadedGames;

        public void LoadGames(List<GameSettings> games, List<GameSettings> waitingGames, Transform gameTemplate)
        {
            if (_loadedGames != null && _loadedGames.Count > 0)
            {
                foreach (var loadedGame in _loadedGames)
                {
                    Object.DestroyImmediate(loadedGame.gameObject);
                }
            }
            _loadedGames = new List<RectTransform>();

            if (games.Count > 0 || waitingGames.Count > 0)
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

            waintingTitleContainer.anchoredPosition = new Vector2(waintingTitleContainer.anchoredPosition.x, (titleContainer.sizeDelta.y + gameContainer.sizeDelta.y) * -1);
            waintingGameContainer.anchoredPosition = new Vector2(waintingTitleContainer.anchoredPosition.x, (titleContainer.sizeDelta.y + gameContainer.sizeDelta.y + waintingTitleContainer.sizeDelta.y) * -1);

            if (waitingGames.Count > 0)
            {
                waintingTitleContainer.sizeDelta = new Vector2(waintingTitleContainer.sizeDelta.x, 20);
                waintingGameContainer.sizeDelta = new Vector2(waintingGameContainer.sizeDelta.x, 150 * waitingGames.Count);

                foreach (var game in waitingGames)
                {
                    RectTransform gameTransform = Object.Instantiate(gameTemplate).GetComponent<RectTransform>();
                    _loadedGames.Add(gameTransform);
                    gameTransform.SetParent(waintingGameContainer);
                    gameTransform.localScale = new Vector3(1, 1, 1);
                    var gameViewerManager = gameTransform.GetComponent<GameViewManager>();
                    gameViewerManager.LoadGame(game);
                }
            }
            else
            {
                waintingTitleContainer.sizeDelta = new Vector2(waintingTitleContainer.sizeDelta.x, 0);
                waintingGameContainer.sizeDelta = new Vector2(waintingGameContainer.sizeDelta.x, 0);
            }

            var currentContainer = this.GetComponent<RectTransform>();
            currentContainer.sizeDelta = new Vector2(gameContainer.sizeDelta.x, titleContainer.sizeDelta.y + gameContainer.sizeDelta.y + waintingTitleContainer.sizeDelta.y + waintingGameContainer.sizeDelta.y);
        }
    }
}