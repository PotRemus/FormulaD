using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace FormuleD.Models.Board
{
    [Serializable]
    [XmlRoot("Map")]
    public class BoardDataSource
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("coef")]
        public float coef;

        [XmlAttribute("preview")]
        public string preview;

        [XmlArray("Cases")]
        [XmlArrayItem("Case")]
        public List<BoardItemDataSource> cases;

        [XmlArray("Bends")]
        [XmlArrayItem("Bend")]
        public List<BendDataSource> bends;

        [XmlArray("Starts")]
        [XmlArrayItem("Index")]
        public List<IndexDataSource> starts;

        [XmlArray("Stands")]
        [XmlArrayItem("Stand")]
        public List<StandDataSource> stands;
    }
}