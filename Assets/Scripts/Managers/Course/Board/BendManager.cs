using UnityEngine;
using System.Collections;
using FormuleD.Models;
using FormuleD.Models.Board;

namespace FormuleD.Managers.Course.Board
{
    public class BendManager : MonoBehaviour
    {
        public BendDataSource bendDataSource;
        private TextMesh _stop;
        private TextMesh _min;
        private TextMesh _max;

        public void InitTurn(BendDataSource turn)
        {
            bendDataSource = turn;
            _stop = this.transform.FindChild("stop-bend-board").GetComponent<TextMesh>();
            _min = this.transform.FindChild("min-bend-board").GetComponent<TextMesh>();
            _max = this.transform.FindChild("max-bend-board").GetComponent<TextMesh>();

            _stop.text = bendDataSource.stop.ToString();
            _max.text = bendDataSource.max.ToString();
            _min.text = bendDataSource.min.ToString();
        }
    }
}