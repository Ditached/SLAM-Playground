using System;
using System.Collections.Generic;
using KalmanDemo;
using Newtonsoft.Json;
using UnityEngine;

namespace NaiveApproach
{
    public class IMUReplay : MonoBehaviour
    {
        public bool useKalman = false;
        
        public float multiplier = 5f;
        public Transform target;
        public float replayRate;
        public TextAsset jsonFile;
        public List<IMUData> imuData;
        
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 0, b: 0, autoScale: false, group:0)] 
        public float acclX;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 0, autoScale: false, group:0)] 
        public float acclY;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 0, b: 1, autoScale: false, group:0)] 
        public float acclZ;
        
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 0, b: 0, autoScale: false, group:3)] 
        public float kal_acclX;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 0, autoScale: false, group:3)] 
        public float kal_acclY;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 0, b: 1, autoScale: false, group:3)] 
        public float kal_acclZ;
        
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 0, b: 0, autoScale: true, group:2)] 
        public float posX;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 0, autoScale: true, group:2)] 
        public float posY;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 0, b: 1, autoScale: true, group:2)] 
        public float posZ;
        
        [DebugGUIGraph(min: -1, max: 1, r: 1, g: 0, b: 0, autoScale: false, group:1)] 
        public float velX;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 1, b: 0, autoScale: false, group:1)] 
        public float velY;
        [DebugGUIGraph(min: -1, max: 1, r: 0, g: 0, b: 1, autoScale: false, group:1)] 
        public float velZ;

        private int index;
        
        private Vector3 velocity = Vector3.zero;
        public List<Vector3> velocities = new List<Vector3>();

        private Vector3 position = Vector3.zero;
        public List<Vector3> positions = new List<Vector3>();
        
        public List<Vector3> kalmanVelocities = new List<Vector3>();

        public float aQ = 1E-06f;
        public float aR = 0.01f;
        private KalmanFilterVector3 filter = new KalmanFilterVector3();

        private Kalman kalman = new Kalman();
        
        
        private void Awake()
        {
            filter = new KalmanFilterVector3(aQ, aR);
            imuData = JsonConvert.DeserializeObject<List<IMUData>>(jsonFile.text);

            for (var i = 0; i < imuData.Count - 1; i++)
            {
                CalcPositionFor(imuData[i], imuData[i + 1]);
            }

            Time.fixedDeltaTime = replayRate;
        }

        private void CalcPositionFor(IMUData a, IMUData b)
        {
            var timeDiff = b.time - a.time;
            
            Debug.Log(timeDiff / TimeSpan.TicksPerMillisecond + "ms");


            if (!useKalman)
            {
                var acclInMsSquared = 0.10197162129779f * new Vector3(a.acclX, a.acclY, a.acclZ);
                velocity += acclInMsSquared * (float) timeDiff / (float) TimeSpan.TicksPerSecond * multiplier;
                
                var deltaTime = (float) timeDiff / (float) TimeSpan.TicksPerSecond;
                position += velocity * deltaTime + 0.5f * acclInMsSquared * deltaTime * deltaTime;
            }
            else
            {
                var acclData = new Vector3(a.acclX, a.acclY, a.acclZ);
                
                Debug.Log(Vector3.SqrMagnitude(acclData));
                
                acclData = filter.Update(acclData);
                kalmanVelocities.Add(acclData * 10);

                var deltaTime = (float) timeDiff / (float) TimeSpan.TicksPerSecond;
                velocity += acclData * deltaTime * multiplier * 0.10197162129779f;
                
                position += velocity * deltaTime + 0.5f * acclData * deltaTime * deltaTime;

                // if (Vector3.SqrMagnitude(acclData) < 0.01f)
                //     filter.Update(new Vector3(0, 0, 0));
            }

            positions.Add(position);
            velocities.Add(velocity);
        }

        private void FixedUpdate()
        {
            if (index == 1)
            {
                if (target.TryGetComponent(out TrailRenderer trail))
                {
                    Debug.Log("Resetting trail");
                    trail.Clear();
                }
            }
            var current = imuData[index];
            
            acclX = current.acclX;
            acclY = current.acclY;
            acclZ = current.acclZ;
            
            velX = velocities[index].x;
            velY = velocities[index].y;
            velZ = velocities[index].z;
            
            posX = positions[index].x;
            posY = positions[index].y;
            posZ = positions[index].z;

            if (useKalman)
            {
                kal_acclX = kalmanVelocities[index].x;
                kal_acclY = kalmanVelocities[index].y;
                kal_acclZ = kalmanVelocities[index].z;
            }


            target.localPosition = positions[index];
            
            index++;

            if (index >= velocities.Count)
            {
                filter.Reset();
                if (target.TryGetComponent(out TrailRenderer trail))
                {
                    Debug.Log("Resetting trail");
                    trail.Clear();
                }
                index = 0;
            }
        }

        public void Filter()
        {
            
            
        }


    }
}