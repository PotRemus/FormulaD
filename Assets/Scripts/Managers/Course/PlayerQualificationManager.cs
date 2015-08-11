using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models.Contexts;
using FormuleD.Engines;

namespace FormuleD.Managers.Course
{
    public class PlayerQualificationManager : MonoBehaviour
    {
        public Text numberText;
        public Text nameText;
        public Button startQuanlification;
        public Transform resultContainer;
        public Text turnText;
        public Text durationText;
        public Text penaltyText;
        public Text totalText;
        public Image playerImage;

        public Color startColor;
        public Color continueColor;

        private PlayerContext _player;

        public void OnStartQualification()
        {
            RaceEngine.Instance.OnStartQualification(_player);
        }

        public void LoadPlayer(PlayerContext player, int index)
        {
            _player = player;
            numberText.text = string.Format("{0}.", index + 1);
            nameText.text = player.name;
            playerImage.color = player.GetColor();
            if (player.qualification != null && player.qualification.state == QualificationStateType.Completed)
            {
                startQuanlification.gameObject.SetActive(false);
                resultContainer.gameObject.SetActive(true);
                turnText.text = (player.qualification.turnHistories.Count - 1).ToString();
                var duration = player.qualification.endDate - player.qualification.startDate;
                int totalMin = Mathf.FloorToInt((float)duration.TotalMinutes);
                durationText.text = string.Format(ResourceEngine.Instance.GetResource("QualificationDurationMinute"), totalMin);
                penaltyText.text = player.qualification.outOfBend.ToString();
                totalText.text = player.qualification.total.ToString();
            }
            else
            {
                startQuanlification.gameObject.SetActive(true);
                
                var imageStartQuanlification = startQuanlification.GetComponent<Image>();
                var textStartQuanlification = startQuanlification.GetComponentInChildren<Text>();
                if (player.qualification == null || player.qualification.state == QualificationStateType.NoPlay)
                {
                    imageStartQuanlification.color = startColor;
                    textStartQuanlification.text = ResourceEngine.Instance.GetResource("StartQualificationButton");
                }
                else
                {
                    imageStartQuanlification.color = continueColor;
                    textStartQuanlification.text = ResourceEngine.Instance.GetResource("ContinueQualificationButton");
                }

                resultContainer.gameObject.SetActive(false);
            }
        }
    }
}