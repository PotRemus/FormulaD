using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models.Board;
using FormuleD.Engines;
using FormuleD.Models.Contexts;
using FormuleD.Managers.Course.Board;

namespace FormuleD.Managers.Course.Player
{
    public class CarLayoutManager : MonoBehaviour
    {
        public Transform carPrefab;

        private Dictionary<string, CarManager> _cars;

        public void BuildCars(List<PlayerContext> players)
        {
            this.DeleteCars();
            this.CreateCars(players);
        }

        public CarManager FindCarManager(PlayerContext player)
        {
            CarManager result = null;
            if (_cars != null && _cars.ContainsKey(player.name))
            {
                result = _cars[player.name];
            }
            return result;
        }

        private void CreateCars(List<PlayerContext> players)
        {
            if (_cars == null)
            {
                _cars = new Dictionary<string, CarManager>();
            }
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var lastIndex = player.GetLastIndex();
                var currentCase = BoardEngine.Instance.GetCase(lastIndex);
                var carManager = this.CreateCarManager(currentCase);

                var nextCase = BoardEngine.Instance.GetNextCase(currentCase);
                carManager.BuildCar(player, currentCase.transform.localPosition, nextCase.transform.localPosition);
                currentCase.hasPlayer = true;

                _cars.Add(player.name, carManager);
            }
        }

        private CarManager CreateCarManager(CaseManager start)
        {
            var carTransform = Instantiate(carPrefab);
            carTransform.SetParent(this.transform);
            carTransform.localPosition = new Vector3(start.transform.position.x, start.transform.position.y, 0);
            return carTransform.GetComponent<CarManager>();
        }

        private void DeleteCars()
        {
            if (_cars != null && _cars.Any())
            {
                foreach (var car in _cars)
                {
                    Destroy(car.Value);
                }
                _cars = null;
            }
        }
    }
}