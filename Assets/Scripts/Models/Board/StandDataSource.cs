using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace FormuleD.Models.Board
{
    [Serializable]
    public class StandDataSource
    {
        [XmlAttribute("player")]
        public int? playerIndex;

        [XmlElement("Target")]
        public IndexDataSource target;
    }
}