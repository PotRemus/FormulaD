using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Models;
using FormuleD.Engines;
using UnityEngine.EventSystems;
using FormuleD.Models.Board;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Board
{
    public class CaseManager : MonoBehaviour
    {
        public BoardItemDataSource itemDataSource;
        public BendDataSource bendDataSource;
        public bool hasPlayer;
        public bool isDangerous;
        public bool isCandidate;

        private SpriteRenderer _spriteLargeRenderer;
        private SpriteRenderer _spriteSmallRenderer;
        private SpriteRenderer _spriteWarningRenderer;
        private SpriteRenderer _spriteDangerousRenderer;
        private TextMesh _textMesh;
        private Color _previousColor;

        public void InitCase(BoardItemDataSource itemDataSource, BendDataSource turnDataSource)
        {
            _spriteSmallRenderer = this.transform.FindChild("case-board-small").GetComponent<SpriteRenderer>();
            _spriteLargeRenderer = this.transform.FindChild("case-board-large").GetComponent<SpriteRenderer>();
            _spriteWarningRenderer = this.transform.FindChild("case-board-warning").GetComponent<SpriteRenderer>();
            _spriteDangerousRenderer = this.transform.FindChild("case-board-dangerous").GetComponent<SpriteRenderer>();
            _textMesh = this.transform.FindChild("case-board-text").GetComponent<TextMesh>();

            this.itemDataSource = itemDataSource;
            this.bendDataSource = turnDataSource;
            this.SetDefaultBorder();
            _spriteWarningRenderer.color = new Color(1, 0, 0, 0);
            _spriteDangerousRenderer.color = new Color(1, 0, 0, 0);
            _textMesh.text = string.Empty;
        }

        public void ResetContent()
        {
            _textMesh.text = string.Empty;
            _spriteWarningRenderer.color = new Color(1, 0, 0, 0);
            _spriteSmallRenderer.color = Color.white;
        }

        public void UpdateContent(int gear, int min, int max, bool hasWarning, bool isBad)
        {
            _spriteSmallRenderer.color = ContextEngine.Instance.gameContext.colorDes[gear - 1];
            if (min != max)
            {
                _textMesh.text = string.Format("{0}-{1}", min, max);
            }
            else
            {
                _textMesh.text = min.ToString();
            }

            if (gear > 2)
            {
                _textMesh.color = Color.white;
            }
            else
            {
                _textMesh.color = Color.black;
            }

            if (hasWarning)
            {
                if (gear == 3)
                {
                    _spriteWarningRenderer.color = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                    _spriteWarningRenderer.color = new Color(1, 0, 0, 0.5f);
                }
            }
            else
            {
                _spriteWarningRenderer.color = new Color(1, 0, 0, 0);
            }

            if (isBad)
            {
                _spriteSmallRenderer.color = Color.black;
            }
        }

        public void UpdateBorder(Color? color = null)
        {
            if (color.HasValue)
            {
                _previousColor = _spriteLargeRenderer.color;
                _spriteLargeRenderer.color = color.Value;
            }
            else
            {
                _spriteLargeRenderer.color = _previousColor;
            }
        }

        public void SetDefaultBorder(Color color)
        {
            if (_spriteLargeRenderer.color == color)
            {
                this.SetDefaultBorder();
            }
        }

        public void SetDangerous(bool enable)
        {
            isDangerous = enable;
            if (isDangerous)
            {
                _spriteDangerousRenderer.color = new Color(0, 0, 0, 0.5f);
            }
            else
            {
                _spriteDangerousRenderer.color = new Color(0, 0, 0, 0);
            }
        }

        private void SetDefaultBorder()
        {
            if (bendDataSource != null)
            {
                _spriteLargeRenderer.color = Config.BoardColor.turnColor;
            }
            else
            {
                _spriteLargeRenderer.color = Config.BoardColor.lineColor;
            }
        }

        void OnMouseEnter()
        {
            if (isCandidate)
            {
                GameEngine.Instance.OnViewRoute(this, true);
            }
        }

        void OnMouseExit()
        {
            if (isCandidate)
            {
                GameEngine.Instance.OnViewRoute(this, false);
            }
        }

        void OnMouseUp()
        {
            if (isCandidate)
            {
                GameEngine.Instance.OnSelectRoute(this);
            }
        }
    }
}