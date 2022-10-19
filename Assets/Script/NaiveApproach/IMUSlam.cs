using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveApproach
{
    public class IMUSlam : MonoBehaviour
    {
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 0, b: 0, autoScale: true, group:0)]
        public float acclX;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 0, autoScale: true, group:0)]
        public float acclY;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 1, autoScale: true, group:0)]
        public float acclZ;
        
        /*
         * CALIBRATE FOR NULLING OUT
         */
        
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 1, b: 0, autoScale: true, group:1)]
        public float gyroX;
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 1, b: 0, autoScale: true, group:1)]
        public float gyroY;
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 1, b: 0, autoScale: true, group:1)]
        public float gyroZ;

        public Transform toMove;
        public float multiplier = 1f;

        private Vector3 cachedPos;
        private Quaternion cachedRot;

        private Vector3 nullAccl = Vector3.zero;

        private void Awake()
        {
            cachedPos = toMove.position;
            cachedRot = toMove.rotation;
        }

        public void Calibrate()
        {
            nullAccl = Input.acceleration;
            ResetMove();
        }

        public void ResetMove()
        {
            toMove.position = cachedPos;
            toMove.rotation = cachedRot;
        }

        public void SetMultiplier(float val)
        {
            multiplier = val;
        }

        public void Update()
        {
            var accerleration = Input.acceleration;
            acclX = accerleration.x;
            acclY = accerleration.y;
            acclZ = accerleration.z;
            
            
            var pos = toMove.position + (accerleration - nullAccl) * (multiplier * Time.deltaTime);
            toMove.position = pos;

            var gyro = Input.gyro.attitude.eulerAngles;
            gyroX = gyro.x;
            gyroY = gyro.y;
            gyroZ = gyro.z;
            toMove.rotation = Input.gyro.attitude;
        }
    }
}