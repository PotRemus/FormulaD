using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace FormuleD.Managers.Course.Board
{
    public class FinishManager : MonoBehaviour
    {
        public void InitFinish(CaseManager[] firstCases, CaseManager[] previousCases)
        {
            this.InitPosistion(firstCases, previousCases);

            var firstPoint = firstCases.First().transform.localPosition;
            var lastPoint = firstCases.Last().transform.localPosition;
            
            var vectorRotation = this.ComputeRotation(firstPoint, lastPoint);
            var rotation = new Quaternion(0, 0, 0, 1);
            rotation.eulerAngles = vectorRotation;
            this.transform.localRotation = rotation;
        }

        private void InitPosistion(CaseManager[] firstCases, CaseManager[] previousCases)
        {
            float smallerDist = float.MaxValue;
            Vector3 smallerFirst = Vector3.zero;
            Vector3 smallerPrevious = Vector3.zero;
            foreach (var firstCase in firstCases)
            {
                foreach (var previousCase in previousCases)
                {
                    var dist = Vector3.Distance(firstCase.transform.localPosition, previousCase.transform.localPosition);
                    if (dist != 0 && dist < smallerDist)
                    {
                        smallerDist = dist;
                        smallerFirst = firstCase.transform.localPosition;
                        smallerPrevious = previousCase.transform.localPosition;
                    }
                }
            }

            var dif = smallerPrevious - smallerFirst;
            var newLocation = smallerFirst + dif / 2;
            this.transform.localPosition = new Vector3(newLocation.x, newLocation.y, 0.2f);
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