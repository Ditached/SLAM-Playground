using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveApproach
{
    public class IMUSlam : MonoBehaviour
    {
        public Transform gyroTransform;
        
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
        public TrailRenderer renderer;
        
        public bool useX, useY, useZ;

        private void Awake()
        {
            Input.gyro.enabled = true;
            
            cachedPos = toMove.position;
            cachedRot = toMove.rotation;
        }

        public void SetUseX(bool val) => useX = val;
        public void SetUseY(bool val) => useY = val;
        public void SetUseZ(bool val) => useZ = val;

        
        public void Calibrate()
        {
            nullAccl = Input.acceleration;
            ResetMove();
        }

        public void ResetMove()
        {
            renderer.Clear();
            toMove.position = cachedPos;
            toMove.rotation = cachedRot;
        }

        public void SetMultiplier(float val)
        {
            multiplier = val;
        }

        public void Update()
        {
            Vector3 dir = Vector3.zero;

            // remap device acceleration axis to game coordinates:
            //  1) XY plane of the device is mapped onto XZ plane
            //  2) rotated 90 degrees around Y axis

            var accerleration = Input.gyro.userAcceleration;
            acclX = accerleration.x;
            acclY = accerleration.y;
            acclZ = accerleration.z; 
            
            // accerleration -= nullAccl;
            //
            // dir.x = -accerleration.y;
            // dir.z = accerleration.x;

            dir.y = useY ? acclZ : 0f;
            dir.z = useZ ? acclY : 0f;
            dir.x = useX ? acclX : 0f;

            // clamp acceleration vector to unit sphere
            // if (dir.sqrMagnitude > 1)
            //     dir.Normalize();

            // Make it move 10 meters per second instead of 10 meters per frame...
            dir = dir * (multiplier * Time.deltaTime * Time.deltaTime);
            toMove.Translate(dir);
            
            var gyroAttitude = Input.gyro.attitude.eulerAngles;
            gyroX = gyroAttitude.x;
            gyroY = gyroAttitude.y;
            gyroZ = gyroAttitude.z;

            gyroTransform.rotation = GyroToUnity(Input.gyro.attitude);
        }
        
        void GyroModifyCamera()
        {
            transform.rotation = GyroToUnity(Input.gyro.attitude);
        }

        private static Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }
    }
}