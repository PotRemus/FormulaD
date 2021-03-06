﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using FormuleD.Engines;
using System;

namespace FormuleD.Managers
{
    public class TextManager : MonoBehaviour
    {
        public string resourceKey;

        private Text _text;
        void Awak()
        {

        }

        // Use this for initialization
        void Start()
        {
            _text = this.GetComponent<Text>();
            _text.text = ResourceEngine.Instance.GetResource(resourceKey);
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