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
            //path = new List<IndexDataSource>();
            //aspirations = new List<List<IndexDataSource>>();

            paths = new List<List<IndexDataSource>>();
        }

        public List<List<IndexDataSource>> paths;
        public int standMovement;
        public int outOfBend;
        public int gear;
        public int de;
    }
}