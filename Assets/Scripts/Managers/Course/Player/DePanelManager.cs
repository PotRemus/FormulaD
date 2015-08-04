using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using FormuleD.Engines;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    public class DePanelManager : MonoBehaviour
    {
        public Color playColor;
        public Color cancelColor;

        public DeManager[] buttonDes;
        public Button buttonPlay;
        public TextManager textButtonPlay;
        private Image imageButtonPlay;
        private bool isCancel;

        void Awake()
        {
            imageButtonPlay = buttonPlay.GetComponent<Image>();
        }

        public void UpdateDe(PlayerContext player)
        {
            if (player.state == PlayerStateType.RollDice)
            {
                this.LoadDeChoise(player);
            }
            else
            {
                this.ViewPlayer(player);
            }
        }

        private void LoadDeChoise(PlayerContext player)
        {
            var gear = PlayerEngine.Instance.GetTurnHistories(player).Last().gear;
            var indexMin = gear - 1 - 1;
            if (player.features.gearbox > 0 && player.features.brake > 0 && player.features.motor > 0)
            {
                indexMin -= 3;
            }
            else if (player.features.gearbox > 0 && player.features.brake > 0)
            {
                indexMin -= 2;
            }
            else if (player.features.gearbox > 0)
            {
                indexMin -= 1;
            }
            if (indexMin < 0)
            {
                indexMin = 0;
            }

            var indexMax = (gear + 1 - 1);
            if (indexMax > 5)
            {
                indexMax = 5;
            }

            DeManager selectedDe = null;
            var currentGear = gear != 0 ? gear : 1;
            for (int i = 0; i < 6; i++)
            {
                var currentDe = buttonDes[i];
                if (indexMin <= i && i <= indexMax)
                {
                    if (currentDe.gear == currentGear)
                    {
                        selectedDe = currentDe;
                    }
                    currentDe.LoadDe(true, gear, player.state == PlayerStateType.RollDice);
                }
                else
                {
                    currentDe.LoadDe(false, gear, player.state == PlayerStateType.RollDice);
                }
            }

            if (selectedDe != null)
            {
                RaceEngine.Instance.OnViewGear(selectedDe.gear, selectedDe.min, selectedDe.max);
            }

            isCancel = false;
            imageButtonPlay.color = playColor;
            textButtonPlay.UpdateResource("ButtonPlay");
            buttonPlay.interactable = true;
        }

        private void ViewPlayer(PlayerContext player)
        {
            int gear = 0;
            if (player.state == PlayerStateType.Waiting || !player.IsPlayable())
            {
                gear = PlayerEngine.Instance.GetTurnHistories(player).Last().gear;
            }
            else
            {
                gear = player.currentTurn.gear;
            }
            for (int i = 0; i < 6; i++)
            {
                var currentDe = buttonDes[i];
                if (gear == i + 1)
                {
                    int de = 0;
                    if (player.state == PlayerStateType.Waiting || !player.IsPlayable())
                    {
                        de = PlayerEngine.Instance.GetTurnHistories(player).Last().de;
                    }
                    else
                    {
                        de = player.currentTurn.de;
                    }
                    currentDe.LoadDe(true, gear, player.state == PlayerStateType.RollDice, de);
                }
                else
                {
                    currentDe.LoadDe(false, gear, player.state == PlayerStateType.RollDice);
                }
                currentDe.SelectGear(gear);
            }

            if (player.state == PlayerStateType.Aspiration)
            {
                isCancel = true;
                imageButtonPlay.color = cancelColor;
                textButtonPlay.UpdateResource("ButtonCancel");
                buttonPlay.interactable = true;
            }
            else
            {
                isCancel = false;
                imageButtonPlay.color = playColor;
                textButtonPlay.UpdateResource("ButtonPlay");
                buttonPlay.interactable = false;
            }
        }

        public void UpdateSelectedDe(int gear)
        {
            foreach (var de in buttonDes)
            {
                de.SelectGear(gear);
            }
        }

        public void OnClickPlay()
        {
            buttonPlay.interactable = false;
            if (isCancel)
            {
                RaceEngine.Instance.OnAspiration(false);
            }
            else
            {
                var de = buttonDes.FirstOrDefault(d => d.selected);
                if (de != null)
                {
                    RaceEngine.Instance.OnRollDice(de.gear, de.min, de.max);
                }
            }
        }
    }
}