using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using System.Diagnostics;

namespace FormuleD.Models.Board
{
    [Serializable]
    public class BoardItemDataSource
    {
        [XmlAttribute("order")]
        public int order;

        [XmlElement("Index")]
        public IndexDataSource index;

        [XmlElement("Position")]
        public PositionDataSource position;

        [XmlArray("Targets")]
        [XmlArrayItem("Target")]
        public List<IndexDataSource> targets;
    }

    [Serializable]
    public class PositionDataSource
    {
        [XmlAttribute("x")]
        public float x;
        [XmlAttribute("y")]
        public float y;
    }

    [DebuggerDisplay("c={column}, r={row}")]
    [Serializable]
    public class IndexDataSource
    {
        [XmlAttribute("row")]
        public int row;
        [XmlAttribute("column")]
        public int column;
        [XmlAttribute("enable")]
        public bool enable;
        public bool Equals(IndexDataSource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.row == this.row && other.column == this.column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(IndexDataSource)) return false;
            return Equals((IndexDataSource)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = column * 1000 + row;
                return result;
            }
        }
    }
}



