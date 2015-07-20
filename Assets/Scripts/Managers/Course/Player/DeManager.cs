using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Engines;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    public class DeManager : MonoBehaviour
    {
        public int gear;
        public int min;
        public int max;
        public bool selected;

        private Button _buttonDe;
        private Image _imageSelector;
        private Image _imageWarning;
        private Image _imageDe;
        private Text _textDe;

        public void LoadDe(bool isEnable, int playerGear, bool isPlayerRollDice, int? de = null)
        {
            if (_imageSelector == null)
            {
                _imageSelector = this.transform.FindChild("Selector").GetComponent<Image>();
            }
            if (_imageDe == null)
            {
                _imageDe = this.GetComponent<Image>();
            }
            if (_textDe == null)
            {
                _textDe = this.transform.FindChild("Text").GetComponent<Text>();
                _textDe.text = string.Format("{0}-{1}", min, max);
            }
            if (_buttonDe == null)
            {
                _buttonDe = this.GetComponent<Button>();
            }
            if (_imageWarning == null)
            {
                _imageWarning = this.transform.FindChild("Warning").GetComponent<Image>();
            }

            if (isEnable)
            {
                if (de.HasValue)
                {
                    _textDe.text = de.Value.ToString();
                }
                else
                {
                    _textDe.text = string.Format("{0}-{1}", min, max);
                }
                var gearDif = playerGear - gear;
                if (gearDif >= 2 && isPlayerRollDice)
                {
                    if (gear == 3)
                    {
                        _imageWarning.color = new Color(1, 1, 1, 0.5f);
                    }
                    else
                    {
                        _imageWarning.color = new Color(1, 0, 0, 0.5f);
                    }
                }
                else
                {
                    _imageWarning.color = new Color(1, 0, 0, 0);
                }
                _imageDe.color = ContextEngine.Instance.gameContext.colorDes[gear - 1];
                _textDe.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1);
                if (isPlayerRollDice)
                {
                    _buttonDe.interactable = true;
                }
                else
                {
                    _buttonDe.interactable = false;
                }
            }
            else
            {
                _imageWarning.color = new Color(1, 0, 0, 0);
                _imageDe.color = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f);
                _textDe.color = new Color(0, 0, 0, 0);
                _buttonDe.interactable = false;
            }
        }

        public void SelectGear(int playerGear)
        {
            if (_imageSelector != null)
            {
                if (playerGear == gear)
                {
                    selected = true;
                    _imageSelector.color = new Color(0, 0, 0, 1f);
                }
                else
                {
                    selected = false;
                    _imageSelector.color = new Color(0, 0, 0, 0);
                }
                _previousColor = _imageSelector.color;
            }
        }

        public void OnClick()
        {
            GameEngine.Instance.OnViewGear(gear, min, max);
        }

        private Color _previousColor;
        public void OnPointerEnter()
        {
            if (_buttonDe.interactable)
            {
                _previousColor = _imageSelector.color;
                _imageSelector.color = new Color(0, 0, 0, 0.5f);
            }
        }

        public void OnPointerExit()
        {
            if (_buttonDe.interactable)
            {
                _imageSelector.color = _previousColor;
            }
        }
    }
}