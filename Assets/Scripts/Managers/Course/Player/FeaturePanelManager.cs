using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    public class FeaturePanelManager : MonoBehaviour
    {
        public Text noteTire;
        public Text noteBrake;
        public Text noteGearbox;
        public Text noteBody;
        public Text noteMotor;
        public Text noteHandling;
        public Text noteBend;

        void Awake()
        {
        }

        public void UpdateFeature(FeatureContext state, int bendStop, int maxBendStop)
        {
            noteTire.text = state.tire.ToString();
            noteTire.color = Color.black;

            noteBrake.text = state.brake.ToString();
            noteBrake.color = Color.black;

            noteGearbox.text = state.gearbox.ToString();
            noteGearbox.color = Color.black;

            noteBody.text = state.body.ToString();
            noteBody.color = Color.black;

            noteMotor.text = state.motor.ToString();
            noteMotor.color = Color.black;

            noteHandling.text = state.handling.ToString();
            noteHandling.color = Color.black;

            var currentBendStop = bendStop;
            if(bendStop > maxBendStop){
                currentBendStop = maxBendStop;
            }
            noteBend.text = string.Format("{0}/{1}", currentBendStop, maxBendStop);
        }

        public void WarningFeature(FeatureContext source, FeatureContext target)
        {
            if (source.tire != target.tire)
            {
                noteTire.text = string.Format("{0} > {1}", source.tire, target.tire);
                noteTire.color = Color.red;
            }
            if (source.brake != target.brake)
            {
                noteBrake.text = string.Format("{0} > {1}", source.brake, target.brake);
                noteBrake.color = Color.red;
            }
            if (source.gearbox != target.gearbox)
            {
                noteGearbox.text = string.Format("{0} > {1}", source.gearbox, target.gearbox);
                noteGearbox.color = Color.red;
            }
            if (source.body != target.body)
            {
                noteBody.text = string.Format("{0} > {1}", source.body, target.body);
                noteBody.color = Color.red;
            }
            if (source.motor != target.motor)
            {
                noteMotor.text = string.Format("{0} > {1}", source.motor, target.motor);
                noteMotor.color = Color.red;
            }
            if (source.handling != target.handling)
            {
                noteHandling.text = string.Format("{0} > {1}", source.handling, target.handling);
                noteHandling.color = Color.red;
            }
        }
    }

    public enum StateType
    {
        tire,
        brake,
        gearbox,
        body,
        motor,
        handling
    }
}