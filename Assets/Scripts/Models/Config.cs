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
            public static Color bleu = new Color(52f / 255f, 152f / 255f, 219f / 255f, 1f);
            public static Color yellow = new Color(241f / 255f, 196f / 255f, 15f / 255f, 1f);
            public static Color green = new Color(46f / 255f, 204f / 255f, 113f / 255f, 1f);
            public static Color purple = new Color(155f / 255f, 89f / 255f, 182f / 255f, 1f);
            //public static Color orange = new Color(221f / 255f, 84f / 255f, 0f, 1f);
            public static Color gray = new Color(153f / 255f, 153f / 255f, 153f / 255f, 1f);
        }

        public static class TrophyColor
        {
            public static Color gold = new Color(233f / 255f, 168f / 255f, 37f / 255f, 1f);
            public static Color silver = new Color(172f / 255f, 172f / 255f, 172f / 255f, 1f);
            public static Color bronze = new Color(205f / 255f, 127f / 255f, 50f / 255f, 1f);
        }
    }
}