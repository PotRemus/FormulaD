using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FormuleD.Engines;
using FormuleD.Models.Contexts;

namespace FormuleD.Managers.Course.Player
{
    public class PlayerPanelManager : MonoBehaviour
    {
        public Transform playerPrefab;

        private Dictionary<string, Transform> _players;

        void Awake()
        {
            _players = new Dictionary<string, Transform>();
        }

        public void BuildPlayers(List<PlayerContext> players)
        {
            var currentPlayers = players.OrderBy(p => p.position).ToList();
            for (int i = 0; i < currentPlayers.Count; i++)
            {
                var player = currentPlayers[i];
                var playerTransform = this.CreatePlayer(i, player);
                _players.Add(player.name, playerTransform);
            }
        }

        public void SelectedPlayer(PlayerContext player)
        {
            foreach (var current in _players)
            {
                var button = current.Value.GetComponent<Button>();
                if (current.Key == player.name)
                {
                    current.Value.localPosition = new Vector3(current.Value.localPosition.x, 2f, 0);
                    button.colors = this.CreateColorBlock(new Color(1f, 1f, 1f, 1f));
                }
                else
                {
                    current.Value.localPosition = new Vector3(current.Value.localPosition.x, 0, 0);
                    button.colors = this.CreateColorBlock(new Color(200f / 255f, 200f / 255f, 200f / 255f, 128f / 255f));
                }
            }
        }

        public void DisablePlayer(PlayerContext player)
        {
            var item = _players[player.name];
            var button = item.GetComponent<Button>();
            button.interactable = false;
        }

        private ColorBlock CreateColorBlock(Color normal)
        {
            ColorBlock colors = new ColorBlock();

            colors.normalColor = normal;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 255f / 255f);
            colors.disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 128f / 255f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            return colors;
        }

        private Transform CreatePlayer(int index, PlayerContext player)
        {
            var playerTransform = Instantiate(playerPrefab);
            playerTransform.SetParent(this.transform);
            playerTransform.localPosition = new Vector3(10 + index * 70, 0, 0);
            playerTransform.localScale = Vector3.one;

            var image = playerTransform.FindChild("Image").GetComponent<Image>();
            image.color = player.GetColor();

            var name = playerTransform.FindChild("Text").GetComponent<Text>();
            name.text = player.name;

            var button = playerTransform.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                PlayerEngine.Instance.SelectedPlayerView(player);
            });

            return playerTransform;
        }
    }
}