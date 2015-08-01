using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models;

namespace FormuleD.Managers.Course
{
    public class ExistingSaveManager : MonoBehaviour
    {
        public SaveManager saveManager;
        public GameSettings gameSettings;
        public Text labelToggle;

        private Toggle toggle;
        
        void Awake()
        {
            toggle = this.GetComponent<Toggle>();
        }

        public void SetGameSettings(GameSettings game)
        {
            gameSettings = game;
            labelToggle.text = gameSettings.id;
        }

        public void OnToggle(bool isOn)
        {
            if (isOn)
            {
                saveManager.SelectedGame(this);
            }
            else
            {
                saveManager.SelectedGame(null);
            }
        }

        public void SetIsOn(bool isOn)
        {
            toggle.isOn = isOn;
        }
    }
}