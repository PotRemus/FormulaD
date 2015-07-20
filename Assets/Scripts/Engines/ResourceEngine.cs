using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Linq;

namespace FormuleD.Engines
{
    public class ResourceEngine : MonoBehaviour
    {
        public string currentLanguage = "fr";

        private string _languageDirectory = @"Assets\Resources\Libelles";
        private Dictionary<string, string> _resources;

        public static ResourceEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of GameEngine!");
            }
            Instance = this;
            this.LoadResource(currentLanguage);
        }

        void Start()
        {
        }

        public void LoadResource(string language)
        {
            var filePath = Path.Combine(_languageDirectory, string.Format("{0}.xml", language));
            if (File.Exists(filePath))
            {
                ResourceCollection temp = null;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ResourceCollection));
                    temp = (ResourceCollection)serializer.Deserialize(fileStream);
                }
                _resources = temp.Items.ToDictionary(k => k.Key, v => v.Value);
                currentLanguage = language;
            }
        }

        public string GetResource(string key)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(key) && _resources.ContainsKey(key))
            {
                result = _resources[key];
            }
            return result;
        }
    }

    [Serializable]
    [XmlRoot("Resources")]
    public class ResourceCollection
    {
        [XmlElement("Resource")]
        public List<ResourceItem> Items;
    }

    public class ResourceItem
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}