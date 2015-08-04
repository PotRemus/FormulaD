using UnityEngine;
using System.Collections;
using FormuleD.Models;
using FormuleD.Engines;

namespace FormuleD.Managers.Course
{
    public class CameraManager : MonoBehaviour
    {
        public bool isEnable = true;

        public float moveSensitivity = 0.00333333f;

        public float zoomSpeed = 5;
        public float zoomSmoothSpeed = 10.0f;
        public float zoomMinSize = 1.0f;
        public float zoomMaxSize = 20.0f;

        public Bounds? bounds;

        private UnityEngine.Camera _camera;

        private float _targetOrthographicSize;
        private Vector3 _lastPosition;
        private Vector3? _targetPosition;

        void Awake()
        {
            _camera = UnityEngine.Camera.main;
            _targetOrthographicSize = _camera.orthographicSize;
        }

        void LateUpdate()
        {
            if (_targetPosition.HasValue)
            {
                if (_targetPosition.Value != transform.position)
                {
                    var dist = Vector3.Distance(_targetPosition.Value, transform.position);
                    if (dist < 2)
                    {
                        dist = 1;
                    }
                    _camera.transform.Translate((_targetPosition.Value - transform.position) / dist);
                    //transform.position = _targetPosition.Value;
                }
                else
                {
                    _targetPosition = null;
                }
            }

            if (!RaceEngine.Instance.isHoverGUI && this.HasMouseInView())
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _lastPosition = Input.mousePosition;
                }

                if (Input.GetMouseButton(1))
                {
                    var mouseSensitivity = moveSensitivity * _camera.orthographicSize;
                    Vector3 delta = Input.mousePosition - _lastPosition;
                    _camera.transform.Translate(-1 * delta.x * mouseSensitivity, -1 * delta.y * mouseSensitivity, 0);
                    _lastPosition = Input.mousePosition;
                }

                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0.0f)
                {
                    _targetOrthographicSize -= scroll * zoomSpeed;
                    _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, zoomMinSize, zoomMaxSize);
                }
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    _lastPosition = Input.mousePosition;
                }
            }

            if (_targetOrthographicSize != _camera.orthographicSize)
            {
                _camera.orthographicSize = Mathf.MoveTowards(_camera.orthographicSize, _targetOrthographicSize, zoomSmoothSpeed * Time.deltaTime);
            }

            if (bounds != null)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, bounds.Value.min.x, bounds.Value.max.x),
                    Mathf.Clamp(transform.position.y, bounds.Value.min.y, bounds.Value.max.y),
                    transform.position.z);
            }
        }

        public void UpdateZoomPosition(Vector3 position1, Vector3 position2)
        {
            _targetPosition = new Vector3(position1.x, position1.y, transform.position.z);
            var dist = Vector3.Distance(position1, position2);
            _targetOrthographicSize = dist;
            _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, zoomMinSize, zoomMaxSize);
        }

        private bool HasMouseInView()
        {
            bool result = false;
            var mousePosition = Input.mousePosition;
            if (mousePosition.x > 0 && mousePosition.x < _camera.pixelWidth && mousePosition.y > 0 && mousePosition.y < _camera.pixelHeight)
            {
                result = true;
            }
            return result;
        }
    }
}