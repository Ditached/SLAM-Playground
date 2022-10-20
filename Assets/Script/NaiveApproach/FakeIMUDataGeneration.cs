using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace NaiveApproach
{
    public class FakeIMUDataGeneration : MonoBehaviour
    {
        public List<IMUData> imuDataList = new List<IMUData>();

        private long tickStep = 10000;

        public float[] steps = new[]
        {
            0f, 1f, -1f, 0f, 0f
        };

        public int valuesBetweenSteps = 50;

        [ContextMenu("Generate")]
        private void Generate()
        {
            imuDataList.Clear();
            for (var i = 0; i < steps.Length - 1; i++)
            {
                for(var j=0; j<valuesBetweenSteps; j++)
                {
                    
                    
                    var imuData = new IMUData
                    {
                        time = tickStep * (i * valuesBetweenSteps + j),
                        acclX = Mathf.Sin(Mathf.Lerp(steps[i], steps[i + 1], (float) j / valuesBetweenSteps)),
                        acclY = Mathf.Sin(Mathf.Lerp(steps[i], steps[i + 1], (float) j / valuesBetweenSteps)),
                        acclZ = Mathf.Tan(Mathf.Lerp(steps[i], steps[i + 1], (float) j / valuesBetweenSteps)),
                    };
                    
                    imuDataList.Add(imuData);
                }
            }
            
            
            Save();
        }
        
        public void Save()
        {
            var json = JsonConvert.SerializeObject(imuDataList, Formatting.Indented);
            
            var path = Path.Combine(Application.persistentDataPath, "FakeData.json");
            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(json);
            writer.Close();
            
            Debug.Log(path);
        }
    }
}