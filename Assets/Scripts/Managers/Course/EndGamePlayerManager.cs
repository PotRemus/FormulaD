using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models.Contexts;
using FormuleD.Models;

namespace FormuleD.Managers.Course
{
    public class EndGamePlayerManager : MonoBehaviour
    {
        public Image trophy;
        public Text name;

        public void LoadPlayer(PlayerContext player, int index)
        {
            name.text = player.name;
            if (index == 0)
            {
                trophy.color = Config.TrophyColor.gold;
            }
            else if (index == 1)
            {
                trophy.color = Config.TrophyColor.silver;
            }
            else if (index == 2)
            {
                trophy.color = Config.TrophyColor.bronze;
            }
        }
    }
}