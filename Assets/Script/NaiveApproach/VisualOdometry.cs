using System;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Xfeatures2dModule;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;

namespace NaiveApproach
{
    public class VisualOdometry : MonoBehaviour
    {
        public int intervalEveryNthFrame = 5;
        public RenderTexture renderTexture;

        public RawImage prevImage;
        public RawImage matchImage;
        public RawImage rawImage;

        public int dy, dx, depth;

        private Mat _prevFrame;
        private MatOfKeyPoint _prevKeypoints;


        private void Update()
        {
            if (Time.frameCount % intervalEveryNthFrame == 0)
            {
                CV_OtherUpdate();
            }
        }


        public void CV_Update()
        {
            var rtAs2D = toTexture2D(this.renderTexture);
            Mat imgMat = new Mat(rtAs2D.height, rtAs2D.width, CvType.CV_8UC4);
            Utils.texture2DToMat(toTexture2D(this.renderTexture), imgMat);
            
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_RGBA2GRAY);
            var temp_prevFrame = imgMat.clone();

 
            
            var featureDetector = FastFeatureDetector.create();
            var keypoint = new MatOfKeyPoint();
            featureDetector.detect(imgMat, keypoint);

            var temp_prevKeypoints = new MatOfKeyPoint(keypoint.clone());
            
            Features2d.drawKeypoints(imgMat, keypoint, imgMat);
            
      
            
            Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(imgMat, texture);
            rawImage.texture = texture;


            if (_prevFrame != null)
            {
                Texture2D texturePrev = new Texture2D(_prevFrame.cols(), _prevFrame.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(_prevFrame, texturePrev);
                prevImage.texture = texturePrev;
                
                DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
                MatOfDMatch matches = new MatOfDMatch();

                matcher.match(keypoint, _prevKeypoints, matches);
                Features2d.drawMatches(imgMat, keypoint, _prevFrame, _prevKeypoints, matches, imgMat);

                
                Texture2D textureMatches = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(imgMat, textureMatches);
                matchImage.texture = textureMatches; 
            }

            _prevFrame = temp_prevFrame;
            _prevKeypoints = temp_prevKeypoints;
            
        }
        
        
        public void CV_OtherUpdate()
        {
            var rtAs2D = toTexture2D(this.renderTexture);
            Mat imgMat = new Mat(rtAs2D.height, rtAs2D.width, CvType.CV_8UC4);
            Utils.texture2DToMat(toTexture2D(this.renderTexture), imgMat);
            
            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_RGBA2GRAY);
            var temp_prevFrame = imgMat.clone();

            
            


            if (_prevFrame != null)
            {
                Texture2D texturePrev = new Texture2D(_prevFrame.cols(), _prevFrame.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(_prevFrame, texturePrev);
                prevImage.texture = texturePrev;
                
                ORB detector = ORB.create();
                ORB extractor = ORB.create();

                MatOfKeyPoint keypoints1 = new MatOfKeyPoint();
                Mat descriptors1 = new Mat();

                detector.detect(imgMat, keypoints1);
                extractor.compute(imgMat, keypoints1, descriptors1);

                MatOfKeyPoint keypoints2 = new MatOfKeyPoint();
                Mat descriptors2 = new Mat();

                detector.detect(_prevFrame, keypoints2);
                extractor.compute(_prevFrame, keypoints2, descriptors2);


                DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
                MatOfDMatch matches = new MatOfDMatch();

                matcher.match(descriptors1, descriptors2, matches);


                Mat resultImg = new Mat();

                Features2d.drawMatches(imgMat, keypoints1, _prevFrame, keypoints2, matches, resultImg);

                Texture2D textureMatch = new Texture2D(resultImg.cols(), resultImg.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(resultImg, textureMatch);
                matchImage.texture = textureMatch; 
                
                Features2d.drawKeypoints(imgMat, keypoints1, imgMat);
                
                Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(imgMat, texture);
                rawImage.texture = texture;
            }

            _prevFrame = temp_prevFrame;
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