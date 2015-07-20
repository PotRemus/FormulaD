using UnityEngine;
using System.Collections;
using FormuleD.Models.Contexts;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FormuleD.Engines
{
    public class ContextEngine : MonoBehaviour
    {
        public GameContext gameContext;

        public static ContextEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of ContextEngine!");
            }
            Instance = this;
        }


        private string _gameDirectory = @"Assets\Resources\Games";
        public List<GameContext> GetGameContexts()
        {
            List<GameContext> result = new List<GameContext>();
            if (Directory.Exists(_gameDirectory))
            {
                foreach (var filePath in Directory.GetFiles(_gameDirectory))
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(GameContext));
                        var gameContext = (GameContext)serializer.Deserialize(fileStream);
                        result.Add(gameContext);
                    }
                }
            }
            return result;
        }
    }
}