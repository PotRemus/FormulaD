using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models.Contexts;
using FormuleD.Models;
using System.Linq;
using FormuleD.Engines;

namespace FormuleD.Managers.Course
{
    public class EndGamePlayerManager : MonoBehaviour
    {
        public Image trophy;
        public Text position;
        public Image playerImg;
        public Text name;
        public Text best;
        public Text turn;

        public void LoadPlayer(PlayerContext player, int index)
        {
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
            position.text = (index + 1).ToString();

            if (index < 3)
            {
                position.gameObject.SetActive(false);
                trophy.gameObject.SetActive(true);
            }
            else
            {
                position.gameObject.SetActive(true);
                trophy.gameObject.SetActive(false);
            }
            playerImg.color = player.GetColor();
            name.text = player.name;
            best.text = player.turnHistories.Max(t => t.paths.SelectMany(p => p.Skip(1)).Count()).ToString();
            if (player.state == PlayerStateType.Finish)
            {
                turn.text = player.turnHistories.Skip(1).Count().ToString();
            }
            else
            {
                turn.text = ResourceEngine.Instance.GetResource("EndGameNA");
            }
        }
    }
}