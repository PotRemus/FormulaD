using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models.Contexts;
using UnityEngine.UI;
using FormuleD.Engines;

namespace FormuleD.Managers.Course
{
    public class QualificationManager : MonoBehaviour
    {
        public RectTransform playersPanelContainer;
        public Transform playerPanelPrefab;
        public Button continueButton;

        private List<Transform> transformPlayers;
        public void OnOpen(List<PlayerContext> players)
        {
            this.CleanPlayers();
            this.LoadPlayers(players);

            if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
            {
                continueButton.gameObject.SetActive(false);
            }
            else
            {
                continueButton.gameObject.SetActive(true);
            }
        }

        private void LoadPlayers(List<PlayerContext> players)
        {
            playersPanelContainer.sizeDelta = new Vector2(680, players.Count * 50);
            int index = 0;
            foreach (var player in players)
            {
                var playerPanelTransfo = Object.Instantiate(playerPanelPrefab);
                playerPanelTransfo.SetParent(playersPanelContainer);
                playerPanelTransfo.localScale = new Vector3(1, 1, 1);
                var playerManager = playerPanelTransfo.GetComponent<PlayerQualificationManager>();
                playerManager.LoadPlayer(player, index);
                index++;
                transformPlayers.Add(playerPanelTransfo);
            }
        }

        private void CleanPlayers()
        {
            if (transformPlayers != null)
            {
                playersPanelContainer.sizeDelta = new Vector2(680, 0);
                foreach (var transformPlayer in transformPlayers)
                {
                    Object.DestroyImmediate(transformPlayer.gameObject);
                }
            }
            transformPlayers = new List<Transform>();
        }

        public void OnContinueRace()
        {
            this.gameObject.SetActive(false);
            RaceEngine.Instance.MouseLeaveGUI();
            //RaceEngine.Instance.OnStartQualification();
        }
    }
}