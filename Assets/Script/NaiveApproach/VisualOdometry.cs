using System;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;

namespace NaiveApproach
{
    public class VisualOdometry : MonoBehaviour
    {
        public RenderTexture renderTexture;

        public RawImage rawImage;

        public void Update()
        {
            var rtAs2D = toTexture2D(this.renderTexture);

            Mat imgMat = new Mat(rtAs2D.height, rtAs2D.width, CvType.CV_8UC4);
            Utils.texture2DToMat(toTexture2D(this.renderTexture), imgMat);
            
            
            
            
            
            Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(imgMat, texture);
            
            
            rawImage.texture = texture;
        }
        
        Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
        
    }
}