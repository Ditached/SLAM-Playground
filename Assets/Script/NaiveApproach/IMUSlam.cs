using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace NaiveApproach
{
    [Serializable]
    public struct IMUData
    {
        public long time;
        public float acclX, acclY, acclZ;
        public float gyroX, gyroY, gyroZ, gyroW;

        public IMUData(Vector3 accl, Quaternion gyro, long time)
        {
            acclX = accl.x;
            acclY = accl.y;
            acclZ = accl.z;
            gyroX = gyro.x;
            gyroY = gyro.y;
            gyroZ = gyro.z;
            gyroW = gyro.w;
            this.time = time;
        }
    }
    public class IMUSlam : MonoBehaviour
    {
        public List<IMUData> imuData = new List<IMUData>();
        public Transform gyroTransform;
        
        public float acclX;
        public float acclY;
        public float acclZ;

        public float gyro_acclX;
        public float gyro_acclY;
        public float gyro_acclZ;

        /*
         * CALIBRATE FOR NULLING OUT
         */
        
        public float gyroX;
        public float gyroY;
        public float gyroZ;

        public Transform toMove;
        public float multiplier = 1f;

        private Vector3 cachedPos;
        private Quaternion cachedRot;

        private Vector3 nullAccl = Vector3.zero;
        public TrailRenderer renderer;
        
        public bool useX, useY, useZ;

        private long startTime;

        private void Awake()
        {
            Application.targetFrameRate = 10000;
            
            Input.gyro.enabled = true;
            
            cachedPos = toMove.position;
            cachedRot = toMove.rotation;


            imuData = new List<IMUData>(100000);
        }

        private void Start()
        {
            startTime = DateTime.Now.Ticks;
            var updateInterval = Input.gyro.updateInterval;
            
            //InvokeRepeating(nameof(SensorUpdate), 0f, updateInterval);
            Input.gyro.updateInterval = 0.05f;
            Time.fixedDeltaTime = Input.gyro.updateInterval;
        }

        public void SetUseX(bool val) => useX = val;
        public void SetUseY(bool val) => useY = val;
        public void SetUseZ(bool val) => useZ = val;

        
        
        public void Calibrate()
        {
            StartCoroutine(Share());

            return;
            nullAccl = Input.acceleration;
            ResetMove();
        }

        public IEnumerator Share()
        {
            var json = JsonConvert.SerializeObject(imuData, Formatting.Indented);
            Debug.Log(json);
           
            
            var path = Path.Combine(Application.persistentDataPath, Time.frameCount.ToString() + "_text.json");
            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(json);
            writer.Close();
            
            Debug.Log(path);

            #if UNITY_ANDROID || UNITY_IOS
            yield return new WaitForSeconds(1f);
            
            new NativeShare().AddFile(path).SetTitle("Moin!").Share();
            #endif

            yield return true;
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

        public void FixedUpdate()
        {
            imuData.Add(new IMUData(Input.gyro.userAcceleration, Input.gyro.attitude, DateTime.Now.Ticks - startTime));
            return;

            Vector3 dir = Vector3.zero;

            // remap device acceleration axis to game coordinates:
            //  1) XY plane of the device is mapped onto XZ plane
            
            //  2) rotated 90 degrees around Y axis
            
            
            var accl = Input.acceleration;
            acclX = accl.x;
            acclY = accl.y;
            acclZ = accl.z;

            var accerleration = Input.gyro.userAcceleration;
            gyro_acclX = accerleration.x;
            gyro_acclY = accerleration.y;
            gyro_acclZ = accerleration.z; 
            
            dir.y = useY ? gyro_acclZ : 0f;
            dir.z = useZ ? gyro_acclY : 0f;
            dir.x = useX ? gyro_acclX : 0f;

            // clamp acceleration vector to unit sphere
            // if (dir.sqrMagnitude > 1)
            //     dir.Normalize();

            // Make it move 10 meters per second instead of 10 meters per frame...
            dir *= (multiplier * Time.deltaTime * Time.deltaTime);
            toMove.Translate(dir);
            
            var gyroAttitude = Input.gyro.attitude.eulerAngles;
            gyroX = gyroAttitude.x;
            gyroY = gyroAttitude.y;
            gyroZ = gyroAttitude.z;

            gyroTransform.rotation = GyroToUnity(Input.gyro.attitude);
            
            Debug.Log(DateTime.Now.Ticks - startTime);
            
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