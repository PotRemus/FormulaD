using UnityEngine;
using System.Collections;
using System;
using FormuleD.Models.Board;
using System.Collections.Generic;

namespace FormuleD.Models.Contexts
{
    [Serializable]
    public class HistoryContext
    {
        public HistoryContext()
        {
            path = new List<IndexDataSource>();
            aspirations = new List<List<IndexDataSource>>();
        }

        public List<IndexDataSource> path;
        public bool hasAspiration;
        public List<List<IndexDataSource>> aspirations;
        public int outOfBend;
        public int gear;
        public int de;

        public IEnumerable<IndexDataSource> GetFullPath()
        {
            foreach (var index in path)
            {
                yield return index;
            }
            foreach (var aspiration in aspirations)
            {
                foreach (var index in aspiration)
                {
                    yield return index;
                }
            }
        }
    }
}