using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace FormuleD.Managers.Course.Board
{
    public class FinishManager : MonoBehaviour
    {
        public void InitFinish(CaseManager[] firstCases)
        {
            var twoFirstCases = firstCases.OrderBy(c => c.itemDataSource.order).Take(2).ToList();
            var firstPosition = twoFirstCases[0].transform.localPosition;
            var lastPosition = twoFirstCases[1].transform.localPosition;
            var firstDif = firstPosition - lastPosition;
            var newPosition = lastPosition + firstDif / 2;
            this.transform.localPosition = new Vector3(newPosition.x, newPosition.y, 0.8f);

            var vectorRotation = this.ComputeRotation(twoFirstCases[0].transform.localPosition, twoFirstCases[1].transform.localPosition);
            var rotation = new Quaternion(0, 0, 0, 1);
            rotation.eulerAngles = vectorRotation;
            this.transform.localRotation = rotation;
        }

        private Vector3 ComputeRotation(Vector3 position1, Vector3 position2)
        {
            Vector3 result = Vector3.zero;
            if (position1.x != position2.x && position1.y != position2.y)
            {
                float ab = 0f;
                float ac = 0f;
                Vector2 direction = Vector2.zero;
                if (position1.x < position2.x)
                {
                    ab = position2.x - position1.x;
                    direction.x = 1;
                }
                else
                {
                    ab = position1.x - position2.x;
                    direction.x = -1;
                }
                if (position1.y < position2.y)
                {
                    ac = position2.y - position1.y;
                    direction.y = 1;
                }
                else if (position1.y > position2.y)
                {
                    ac = position1.y - position2.y;
                    direction.y = -1;
                }

                var acb = Mathf.Atan(ac / ab);
                if (position1.x < position2.x && position1.y < position2.y)
                {
                    var z = acb * Mathf.Rad2Deg;
                    result = new Vector3(0, 0, z);
                }
                else if (position1.x > position2.x && position1.y < position2.y)
                {
                    var abc = 90 - acb * Mathf.Rad2Deg;
                    var z = abc + 90;
                    result = new Vector3(0, 0, z);
                }
                else if (position1.x > position2.x && position1.y > position2.y)
                {
                    var z = acb * Mathf.Rad2Deg + 180;
                    result = new Vector3(0, 0, z);
                }
                else if (position1.x < position2.x && position1.y > position2.y)
                {
                    var abc = 90 - acb * Mathf.Rad2Deg;
                    var z = abc + 270;
                    result = new Vector3(0, 0, z);
                }
            }
            else if (position1.x != position2.x)
            {
                if (position1.x < position2.x)
                {
                    result = new Vector3(0, 0, 0);
                }
                else
                {
                    result = new Vector3(0, 0, 180);
                }
            }
            else if (position1.y != position2.y)
            {
                if (position1.y < position2.y)
                {
                    result = new Vector3(0, 0, 90);
                }
                else
                {
                    result = new Vector3(0, 0, 270);
                }
            }
            return result;
        }
    }
}