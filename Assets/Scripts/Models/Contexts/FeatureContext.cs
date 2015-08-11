using UnityEngine;
using System.Collections;
using System;

namespace FormuleD.Models.Contexts
{
    [Serializable]
    public class FeatureContext
    {
        public int tire;
        public int brake;
        public int gearbox;
        public int body;
        public int motor;
        public int handling;
        public int outOfBend;

        public FeatureContext Clone()
        {
            FeatureContext result = new FeatureContext();
            result.tire = this.tire;
            result.brake = this.brake;
            result.gearbox = this.gearbox;
            result.body = this.body;
            result.motor = this.motor;
            result.handling = this.handling;
            result.outOfBend = this.outOfBend;
            return result;
        }
    }
}