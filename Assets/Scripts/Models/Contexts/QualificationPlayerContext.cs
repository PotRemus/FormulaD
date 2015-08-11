using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FormuleD.Models.Contexts
{
    [Serializable]
    public class QualificationPlayerContext
    {
        public QualificationPlayerContext()
        {
            turnHistories = new List<HistoryContext>();
        }
        public List<HistoryContext> turnHistories;
        public QualificationStateType state;
        public DateTime startDate;
        public DateTime endDate;
        public int outOfBend;
        public int total;
        public bool isDead;
    }

    public enum QualificationStateType
    {
        NoPlay,
        Playing,
        Completed
    }
}