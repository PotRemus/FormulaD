using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Engines;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CarManager : MonoBehaviour
    {
        private PlayerContext _player;
        private SpriteRenderer _spriteRenderer;

        private List<Vector3> _movements;
        private Vector3 _nextStep;
        private Vector3? _currentTarget;
        private Vector3? _previousTarget;

        private float _nbStep;
        private Vector3 _stepMovement;

        void Awake()
        {
            _movements = new List<Vector3>();
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        public void BuildCar(PlayerContext player, Vector3 startPosition, Vector3 nextPosition)
        {
            _player = player;
            _spriteRenderer.color = _player.GetColor();
            var vectorRotation = this.ComputeRotation(startPosition, nextPosition);
            var rotation = new Quaternion(0, 0, 0, 1);
            rotation.eulerAngles = vectorRotation;
            this.transform.localRotation = rotation;
        }

        public void AddMovements(IEnumerable<Vector3> movements, Vector3 nextStep)
        {
            _movements.AddRange(movements);
            _nextStep = nextStep;
        }

        public void ReturnCar(Vector3 previousPosition)
        {
            var vectorRotation = this.ComputeRotation(this.transform.position, previousPosition);
            var rotation = new Quaternion(0, 0, 0, 1);
            rotation.eulerAngles = vectorRotation;
            this.transform.localRotation = rotation;
        }

        public void Dead()
        {
            //TODO faire une animation de mort
            _spriteRenderer.color = new Color(0, 0, 0, 1);
        }

        void Update()
        {
            if (!_currentTarget.HasValue && _movements.Any())
            {
                //_previousTarget = _currentTarget;
                _currentTarget = _movements.First();
                var step = _player.currentTurn.gear / 25f;
                this.ComputeStepPosition(step, _currentTarget.Value);
                if (transform.position != _currentTarget.Value)
                {
                    var vectorRotation = this.ComputeRotation(transform.position, _currentTarget.Value);
                    var rotation = new Quaternion(0, 0, 0, 1);
                    rotation.eulerAngles = vectorRotation;
                    this.transform.localRotation = rotation;
                }                
                _movements.Remove(_currentTarget.Value);
            }

            if (_currentTarget.HasValue)
            {
                if (_nbStep < 1 || (_currentTarget.Value.x == transform.position.x && _currentTarget.Value.y == transform.position.y))
                {
                    transform.position = _currentTarget.Value;
                    if (!_movements.Any())
                    {
                        Vector3 vectorRotation;
                        if (_previousTarget.HasValue)
                        {
                            vectorRotation = this.ComputeRotation(_previousTarget.Value, _nextStep);
                        }
                        else
                        {
                            vectorRotation = this.ComputeRotation(_currentTarget.Value, _nextStep);
                        }
                        var rotation = new Quaternion(0, 0, 0, 1);
                        rotation.eulerAngles = vectorRotation;
                        this.transform.localRotation = rotation;
                        GameEngine.Instance.OnFinishMouvement();
                    }
                    _previousTarget = _currentTarget;
                    _currentTarget = null;
                }
                else
                {
                    var to = new Vector3(transform.position.x + _stepMovement.x, transform.position.y + _stepMovement.y, transform.localPosition.z);
                    transform.localPosition = to;
                    _nbStep--;
                }
            }
        }

        void OnMouseUp()
        {
            if (!GameEngine.Instance.isHoverGUI)
            {
                PlayerEngine.Instance.SelectedPlayerView(_player);
            }
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

        private void ComputeStepPosition(float step, Vector3 target)
        {
            _stepMovement = Vector3.zero;
            if (transform.position.x != target.x && transform.position.y != target.y)
            {
                float ab = 0f;
                float ac = 0f;
                Vector2 direction = Vector2.zero;
                if (transform.position.x < target.x)
                {
                    ab = target.x - transform.position.x;
                    direction.x = 1;
                }
                else
                {
                    ab = transform.position.x - target.x;
                    direction.x = -1;
                }
                if (transform.position.y < target.y)
                {
                    ac = target.y - transform.position.y;
                    direction.y = 1;
                }
                else if (transform.position.y > target.y)
                {
                    ac = transform.position.y - target.y;
                    direction.y = -1;
                }

                var acb = Mathf.Atan(ac / ab);
                _stepMovement.x = Mathf.Cos(acb) * step * direction.x;
                _stepMovement.y = Mathf.Sin(acb) * step * direction.y;

                var bc = ab / Mathf.Cos(acb);
                _nbStep = bc / step;
            }
            else if (transform.position.x != target.x)
            {
                _stepMovement.y = 0f;
                if (transform.position.x < target.x)
                {
                    _stepMovement.x = step;
                    _nbStep = (target.x - transform.position.x) / step;
                }
                else
                {
                    _stepMovement.x = -1 * step;
                    _nbStep = (transform.position.x - target.x) / step;
                }
            }
            else if (transform.position.y != target.y)
            {
                _stepMovement.x = 0f;
                if (transform.position.y < target.y)
                {
                    _stepMovement.y = step;
                    _nbStep = (target.y - transform.position.y) / step;
                }
                else
                {
                    _stepMovement.y = -1 * step;
                    _nbStep = (transform.position.y - target.y) / step;
                }
            }
            else
            {
                _stepMovement = target;
                _nbStep = 0;
            }
        }
    }
}