using UnityEngine;
using System.Collections;
using FormuleD.Engines;
using System.Linq;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Launcher
{
    public class GameManager : MonoBehaviour
    {
        public OnlinePanelManager onlinePanelManager;
        public GamePanelManager localPanelManagers;
        public GamePanelManager completedPanelManagers;
        public RectTransform[] newContainer;
        public RectTransform gameScrollContent;

        public Transform gamePreviewPrefab;

        //TODO gerer l'authentification
        public string currentPlayerName = "Remus";

        // Use this for initialization
        void Start()
        {
            var candidates = SettingsEngine.Instance.CandidateGames();
            onlinePanelManager.LoadGames(
                candidates.Where(c => c.state != GameStateType.Completed && c.type == GameType.Online && c.players.Any(p => p.isCurrent && p.id == currentPlayerName)).ToList(),
                candidates.Where(c => c.state != GameStateType.Completed && c.type == GameType.Online && !c.players.Any(p => p.isCurrent && p.id == currentPlayerName)).ToList(), 
                gamePreviewPrefab);
            var rectOnlinePanel = onlinePanelManager.GetComponent<RectTransform>();

            float localOffset = rectOnlinePanel.sizeDelta.y + newContainer[0].sizeDelta.y;
            localPanelManagers.LoadGames(candidates.Where(c => c.state != GameStateType.Completed && c.type == GameType.Local).ToList(), gamePreviewPrefab, localOffset);
            var rectLocalPanel = localPanelManagers.GetComponent<RectTransform>();
            
            var completedOffset = localOffset + rectLocalPanel.sizeDelta.y;
            completedPanelManagers.LoadGames(candidates.Where(c => c.state == GameStateType.Completed).ToList(), gamePreviewPrefab, completedOffset);
            var rectCompletedPanel = localPanelManagers.GetComponent<RectTransform>();

            gameScrollContent.sizeDelta = new Vector2(gameScrollContent.sizeDelta.x,
            rectOnlinePanel.sizeDelta.y + rectLocalPanel.sizeDelta.y + rectCompletedPanel.sizeDelta.y + newContainer.Sum(n => n.sizeDelta.y));

            var lastNew = newContainer.Last();
            lastNew.anchoredPosition = new Vector2(lastNew.anchoredPosition.x, (rectOnlinePanel.sizeDelta.y + rectLocalPanel.sizeDelta.y + rectCompletedPanel.sizeDelta.y + lastNew.sizeDelta.y) * -1);
        }

        public void OnNewGame()
        {
            UnityEngine.Object.DontDestroyOnLoad(ContextEngine.Instance.gameObject);
            Application.LoadLevel("Race");
        }
    }
}