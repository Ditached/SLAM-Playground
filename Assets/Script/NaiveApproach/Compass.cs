using System;
using UnityEngine;

namespace NaiveApproach
{
    public class Compass : MonoBehaviour
    {
        public Transform arrow;
        public Transform magneticArrow;
        public Transform raw;
        
        public void Start()
        {
            Input.compass.enabled = true;
            Input.location.Start();
        }

        public void Update()
        {
            arrow.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
            magneticArrow.rotation =  Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
            raw.rotation = Quaternion.Euler(Input.compass.rawVector);
        }
    }
}