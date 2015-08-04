using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Launcher
{
    public class PlayerViewManager : MonoBehaviour
    {
        public Sprite winnerSprite;
        public Sprite currentSprite;
        public Sprite deadSprite;

        public Text nameText;
        public Image image;

        public void LoadPlayer(PlayerSettings player, GameStateType gameState, int index)
        {
            if (player == null)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(true);
                nameText.text = player.name;
                image.color = new Color(1, 1, 1, 1);
                if (player.isDead)
                {
                    image.sprite = deadSprite;
                }
                else if (gameState == GameStateType.Completed)
                {
                    if (index == 0)
                    {
                        image.sprite = winnerSprite;
                    }
                    else
                    {
                        image.sprite = null;
                        image.color = new Color(1, 1, 1, 0);
                    }
                }
                else if (player.isCurrent)
                {
                    image.sprite = currentSprite;
                }
                else
                {
                    image.sprite = null;
                    image.color = new Color(1, 1, 1, 0);
                }
            }
        }
    }
}