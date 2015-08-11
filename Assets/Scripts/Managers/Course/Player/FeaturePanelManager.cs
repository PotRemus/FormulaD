using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    public class FeaturePanelManager : MonoBehaviour
    {
        public RectTransform panelRaceFeature;
        public Text noteTire;
        public Text noteBrake;
        public Text noteGearbox;
        public Text noteBody;
        public Text noteMotor;
        public Text noteHandling;
        public Text noteBend;

        public RectTransform panelQualificationFeature;
        public Text notePenalty;
        public Text noteQualificationBend;

        void Awake()
        {
        }

        public void UpdateFeature(GameStateType state, FeatureContext feature, int bendStop, int maxBendStop)
        {
            var currentBendStop = bendStop;
            if (bendStop > maxBendStop)
            {
                currentBendStop = maxBendStop;
            }

            if (state == GameStateType.Race)
            {
                panelRaceFeature.gameObject.SetActive(true);
                panelQualificationFeature.gameObject.SetActive(false);
                noteTire.text = feature.tire.ToString();
                noteTire.color = Color.black;

                noteBrake.text = feature.brake.ToString();
                noteBrake.color = Color.black;

                noteGearbox.text = feature.gearbox.ToString();
                noteGearbox.color = Color.black;

                noteBody.text = feature.body.ToString();
                noteBody.color = Color.black;

                noteMotor.text = feature.motor.ToString();
                noteMotor.color = Color.black;

                noteHandling.text = feature.handling.ToString();
                noteHandling.color = Color.black;

                noteBend.text = string.Format("{0}/{1}", currentBendStop, maxBendStop);
            }
            else if (state == GameStateType.Qualification)
            {
                panelRaceFeature.gameObject.SetActive(false);
                panelQualificationFeature.gameObject.SetActive(true);

                notePenalty.text = feature.outOfBend.ToString();
                notePenalty.color = Color.black;

                noteQualificationBend.text = string.Format("{0}/{1}", currentBendStop, maxBendStop);
            }
        }

        public void WarningFeature(GameStateType state, FeatureContext source, FeatureContext target)
        {
            if (state == GameStateType.Race)
            {
                if (source.tire != target.tire)
                {
                    noteTire.text = target.tire.ToString();
                    noteTire.color = Color.red;
                }
                if (source.brake != target.brake)
                {
                    noteBrake.text = target.brake.ToString();
                    noteBrake.color = Color.red;
                }
                if (source.gearbox != target.gearbox)
                {
                    noteGearbox.text = target.gearbox.ToString();
                    noteGearbox.color = Color.red;
                }
                if (source.body != target.body)
                {
                    noteBody.text = target.body.ToString();
                    noteBody.color = Color.red;
                }
                if (source.motor != target.motor)
                {
                    noteMotor.text = target.motor.ToString();
                    noteMotor.color = Color.red;
                }
                if (source.handling != target.handling)
                {
                    noteHandling.text = target.handling.ToString();
                    noteHandling.color = Color.red;
                }
            }
            else if (state == GameStateType.Qualification)
            {
                if (source.outOfBend != target.outOfBend)
                {
                    notePenalty.text = target.outOfBend.ToString();
                    notePenalty.color = Color.red;
                }
            }
        }
    }
}