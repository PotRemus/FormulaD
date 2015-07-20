using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System;

namespace FormuleD.Models.Board
{
    [Serializable]
    public class BendDataSource
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("stop")]
        public int stop;
        [XmlAttribute("min")]
        public int min;
        [XmlAttribute("max")]
        public int max;
        [XmlAttribute("bestColumn")]
        public int bestColumn;
        [XmlElement("Icon")]
        public PositionDataSource icon;
        [XmlElement("Target")]
        public List<IndexDataSource> Targets;
    }
}