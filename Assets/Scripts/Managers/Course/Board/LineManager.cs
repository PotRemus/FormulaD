using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models;
using FormuleD.Engines;

namespace FormuleD.Managers.Course.Board
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineManager : MonoBehaviour
    {
        public CaseManager source;
        public CaseManager target;

        private LineRenderer _lineRenderer;
        private Color _previousColor;
        private Color _currentColor;

        void Awake()
        {
            _lineRenderer = this.GetComponent<LineRenderer>();
        }

        public void InitLine(CaseManager source, CaseManager target)
        {
            this.source = source;
            this.target = target;
            this.SetDefaultColor();
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z));
        }

        public void UpdateColor(Color? color = null)
        {
            if (color.HasValue)
            {
                _previousColor = _currentColor;
                _currentColor = color.Value;
                _lineRenderer.SetColors(color.Value, color.Value);
            }
            else
            {
                _currentColor = _previousColor;
                _lineRenderer.SetColors(_previousColor, _previousColor);
            }
        }

        public void SetDefaultColor(Color color)
        {
            if (_currentColor == color)
            {
                this.SetDefaultColor();
            }
        }

        private void SetDefaultColor()
        {
            if (source.bendDataSource != null && target.bendDataSource != null)
            {
                _currentColor = Config.BoardColor.turnColor;
                _lineRenderer.SetColors(Config.BoardColor.turnColor, Config.BoardColor.turnColor);
            }
            else
            {
                _currentColor = Config.BoardColor.lineColor;
                _lineRenderer.SetColors(Config.BoardColor.lineColor, Config.BoardColor.lineColor);
            }
        }
    }
}