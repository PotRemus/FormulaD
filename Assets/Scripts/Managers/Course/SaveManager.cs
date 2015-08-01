using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Engines;
using System.Collections.Generic;
using FormuleD.Models;

namespace FormuleD.Managers.Course
{
    public class SaveManager : MonoBehaviour
    {
        public RectTransform existingGameContainer;
        public InputField newGameInput;
        public RectTransform saveContainer;
        public Transform togglePrefab;

        private ExistingSaveManager _selectedGame;
        private List<RectTransform> _existingGames;

        public void OnSave()
        {
            string id = string.Empty;
            if (newGameInput.text.Trim() == string.Empty)
            {
                if (_selectedGame != null)
                {
                    id = _selectedGame.gameSettings.id;
                }
            }
            else
            {
                id = newGameInput.text.Trim();
            }

            ContextEngine.Instance.gameContext.id = id;
            ContextEngine.Instance.SaveContext();
            saveContainer.gameObject.SetActive(false);
        }

        public void OnClose()
        {
            saveContainer.gameObject.SetActive(false);
            RaceEngine.Instance.MouseLeaveGUI();
        }

        public void OnOpen()
        {
            RaceEngine.Instance.MouseEnterGUI();
            this.EmptyExistingGame();
            saveContainer.gameObject.SetActive(true);
            var candidate = SettingsEngine.Instance.CandidateGames();
            this.LoadExistingGame(candidate);
            
        }

        private void LoadExistingGame(List<GameSettings> games)
        {
            newGameInput.text = string.Empty;
            existingGameContainer.sizeDelta = new Vector2(460, games.Count * 30);
            int index = 0;
            foreach (var game in games)
            {
                var toggle = Object.Instantiate(togglePrefab).GetComponent<RectTransform>();
                toggle.SetParent(existingGameContainer);
                _existingGames.Add(toggle);
                var script = toggle.GetComponent<ExistingSaveManager>();
                script.saveManager = this;
                script.SetGameSettings(game);
                index++;
            }
        }

        private void EmptyExistingGame()
        {
            if (_existingGames != null && _existingGames.Count > 0)
            {
                foreach (var existingGame in _existingGames)
                {
                    Object.DestroyImmediate(existingGame.gameObject);
                }
            }
            _existingGames = new List<RectTransform>();
            existingGameContainer.sizeDelta = new Vector2(460, 0);
        }

        public void SelectedGame(ExistingSaveManager existingGame)
        {
            _selectedGame = existingGame;
            if (existingGame != null)
            {
                newGameInput.text = string.Empty;
            }
        }

        public void OnNewGameChange(string text)
        {
            if (_selectedGame != null && newGameInput.text.Trim() != string.Empty)
            {
                _selectedGame.SetIsOn(false);
                _selectedGame = null;
            }
        }
    }
}