using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Engines;

namespace FormuleD.Managers
{
    public class TextManager : MonoBehaviour
    {
        public string resourceKey;

        private Text _text;
        void Awak()
        {
            _text = this.GetComponent<Text>();
        }

        // Use this for initialization
        void Start()
        {
            try
            {
                _text.text = ResourceEngine.Instance.GetResource(resourceKey);
            }
            catch { }
        }

        public void UpdateResource(string key)
        {
            if (_text == null)
            {
                _text = this.GetComponent<Text>();
            }
            resourceKey = key;
            _text.text = ResourceEngine.Instance.GetResource(key);
        }
    }
}