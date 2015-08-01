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
        //public float maxX;
        //public float minX;
        //public float maxY;
        //public float minY;

        private UnityEngine.Camera _camera;

        private float _targetOrthographicSize;
        private Vector3 _lastPosition;

        void Awake()
        {
            _camera = UnityEngine.Camera.main;
            _targetOrthographicSize = _camera.orthographicSize;
        }

        void LateUpdate()
        {
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
                if (_targetOrthographicSize != _camera.orthographicSize)
                {
                    _camera.orthographicSize = Mathf.MoveTowards(_camera.orthographicSize, _targetOrthographicSize, zoomSmoothSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    _lastPosition = Input.mousePosition;
                }
            }

            if (bounds != null)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, bounds.Value.min.x, bounds.Value.max.x),
                    Mathf.Clamp(transform.position.y, bounds.Value.min.y, bounds.Value.max.y),
                    transform.position.z);
            }
            //if (bounds != null)
            //{
            //    var v3 = transform.position;
            //    v3.x = Mathf.Clamp(v3.x, bounds.Value.min.x, bounds.Value.max.x);
            //    v3.y = Mathf.Clamp(v3.y, bounds.Value.min.y, bounds.Value.max.y);
            //    transform.position = v3;
            //    bounds = null;
            //}
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