using UnityEngine;
using System.Collections;

namespace FormuleD.Models
{
    public static class Config
    {
        public static class BoardColor
        {
            public static Color lineColor = Color.black;
            public static Color turnColor = new Color(220f / 255f, 23f / 255f, 23f / 255f, 1f);
        }

        public static class PlayerColor
        {
            public static Color bleu = Color.blue;
            public static Color yellow = Color.yellow;
            public static Color green = Color.green;
        }
    }
}