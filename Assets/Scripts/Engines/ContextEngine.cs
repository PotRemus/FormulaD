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
                    if (!filePath.EndsWith(".meta"))
                    {
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(GameContext));
                            var gameContext = (GameContext)serializer.Deserialize(fileStream);
                            result.Add(gameContext);
                        }
                    }
                }
            }
            return result;
        }

        public void SaveContext()
        {
            var filePath = Path.Combine(_gameDirectory, gameContext.id);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GameContext));
                serializer.Serialize(fileStream, gameContext);
            }
        }

        public void LoadContext(string id)
        {
            var filePath = Path.Combine(_gameDirectory, id);
            if (File.Exists(filePath))
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GameContext));
                    gameContext = serializer.Deserialize(fileStream) as GameContext;
                }
            }
        }
    }
}