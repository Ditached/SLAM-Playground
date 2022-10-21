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
    public enum MatchingAlgos
    {
        FLANNBASED = 1,
        BRUTEFORCE = 2,
        BRUTEFORCE_L1 = 3,
        BRUTEFORCE_HAMMING = 4,
        BRUTEFORCE_HAMMINGLUT = 5,
        BRUTEFORCE_SL2 = 6
    }

    public class VisualOdometry : MonoBehaviour
    {
        public float distanceThreshold = 0f;
        public bool defaultOrbParameter = true;
        public int k = 1;
        public bool knn;
        public MatchingAlgos matchingAlgo = MatchingAlgos.BRUTEFORCE_HAMMING;
        public Texture2D ditachedLogo;
        public Mat ditachedLogoMat;

        public bool useDitachedLogoAsMarker = true;
        public bool useMarkerAsInputTexture = false;

        public int intervalEveryNthFrame = 5;
        public RenderTexture renderTexture;

        public RawImage prevImage;
        public RawImage matchImage;
        public RawImage rawImage;

        public int dy, dx, depth;

        private Mat _prevFrame;
        private MatOfKeyPoint _prevKeypoints;
        public int nfeatures;
        public float scaleFactor;
        public int nlevels;
        public int edgethreshold;

        private void Start()
        {
            ditachedLogoMat = new Mat(ditachedLogo.height, ditachedLogo.width, CvType.CV_8UC4);
            Utils.texture2DToMat(ditachedLogo, ditachedLogoMat);
            Imgproc.cvtColor(ditachedLogoMat, ditachedLogoMat, Imgproc.COLOR_RGBA2GRAY);
        }

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
                Texture2D texturePrev =
                    new Texture2D(_prevFrame.cols(), _prevFrame.rows(), TextureFormat.RGBA32, false);
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

            if (useMarkerAsInputTexture)
            {
                imgMat = new Mat(ditachedLogo.height, ditachedLogo.width, CvType.CV_8UC4);
                Utils.texture2DToMat(ditachedLogo, imgMat);
            }

            Imgproc.cvtColor(imgMat, imgMat, Imgproc.COLOR_RGBA2GRAY);
            var temp_prevFrame = imgMat.clone();


            if (_prevFrame != null || useDitachedLogoAsMarker)
            {
                Texture2D texturePrev = Texture2D.redTexture;

                if (!useDitachedLogoAsMarker)
                {
                    texturePrev = new Texture2D(_prevFrame.cols(), _prevFrame.rows(), TextureFormat.RGBA32, false);
                    Utils.matToTexture2D(_prevFrame, texturePrev);
                }

                if (useDitachedLogoAsMarker)
                {
                    texturePrev = ditachedLogo;
                    _prevFrame = ditachedLogoMat;
                }

                prevImage.texture = texturePrev;


                ORB detector = ORB.create();
                ORB extractor = ORB.create();


                if (!defaultOrbParameter)
                {
                    detector = ORB.create(nfeatures < 1 ? 1 : nfeatures, scaleFactor < 1 ? 1 : scaleFactor,
                        nlevels < 1 ? 1 : nlevels, edgethreshold < 1 ? 1 : edgethreshold);
                    extractor = ORB.create(nfeatures < 1 ? 1 : nfeatures, scaleFactor < 1 ? 1 : scaleFactor,
                        nlevels < 1 ? 1 : nlevels, edgethreshold < 1 ? 1 : edgethreshold);
                }

                MatOfKeyPoint keypoints1 = new MatOfKeyPoint();
                Mat descriptors1 = new Mat();
                descriptors1.convertTo(descriptors1, CvType.CV_32F);

                detector.detect(imgMat, keypoints1);
                extractor.compute(imgMat, keypoints1, descriptors1);


                MatOfKeyPoint keypoints2 = new MatOfKeyPoint();
                Mat descriptors2 = new Mat();
                descriptors2.convertTo(descriptors2, CvType.CV_32F);


                detector.detect(_prevFrame, keypoints2);
                extractor.compute(_prevFrame, keypoints2, descriptors2);


                Mat resultImg = new Mat();
                DescriptorMatcher matcher = DescriptorMatcher.create((int) matchingAlgo);

                if (!knn)
                {
                    FlannBasedMatcher flannBasedMatcher = FlannBasedMatcher.create();
                    //flannBasedMatcher.

                    MatOfDMatch matches = new MatOfDMatch();
                    //matcher.match(descriptors1, descriptors2, matches);
                    flannBasedMatcher.match(descriptors1, descriptors2, matches);


                    List<DMatch> goodMatches = new List<DMatch>();
                    Debug.Log(matches.toArray().Length);
                    foreach (var dMatch in matches.toArray())
                    {
                        if (dMatch.distance < distanceThreshold)
                        {
                            goodMatches.Add(dMatch);
                        }
                    }

                    matches.fromArray(goodMatches.ToArray());

                    Features2d.drawMatches(imgMat, keypoints1, _prevFrame, keypoints2, matches, resultImg);
                }
                else
                {
                    var listMatches = new List<MatOfDMatch>();
                    matcher.knnMatch(descriptors1, descriptors2, listMatches, k);

                    //Features2d.drawMatchesKnn(imgMat, keypoints1, _prevFrame, keypoints2, listMatches, resultImg);

                    MatOfDMatch matches = new MatOfDMatch();

                    List<DMatch> goodMatches = new List<DMatch>();
                    Debug.Log(matches.toArray().Length);

                    foreach (var matOfDMatch in listMatches)
                    {
                        foreach (var dMatch in matOfDMatch.toArray())
                        {
                            if (dMatch.distance < distanceThreshold)
                            {
                                goodMatches.Add(dMatch);
                            }
                        }
                    }

                    matches.fromArray(goodMatches.ToArray());
                    Features2d.drawMatches(imgMat, keypoints1, _prevFrame, keypoints2, matches, resultImg);
                    
                    
                    //Find4PointContours(yMat, contours);
 
                }


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
        
        private void Find4PointContours(Mat image, List<MatOfPoint> contours)
        {
            contours.Clear();
            List<MatOfPoint> tmp_contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(image, tmp_contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

            foreach (var cnt in tmp_contours)
            {
                MatOfInt hull = new MatOfInt();
                Imgproc.convexHull(cnt, hull, false);

                Point[] cnt_arr = cnt.toArray();
                int[] hull_arr = hull.toArray();
                Point[] pts = new Point[hull_arr.Length];
                for (int i = 0; i < hull_arr.Length; i++)
                {
                    pts[i] = cnt_arr[hull_arr[i]];
                }

                MatOfPoint2f ptsFC2 = new MatOfPoint2f(pts);
                MatOfPoint2f approxFC2 = new MatOfPoint2f();
                MatOfPoint approxSC2 = new MatOfPoint();

                double arclen = Imgproc.arcLength(ptsFC2, true);
                Imgproc.approxPolyDP(ptsFC2, approxFC2, 0.01 * arclen, true);
                approxFC2.convertTo(approxSC2, CvType.CV_32S);

                if (approxSC2.size().area() != 4)
                    continue;

                contours.Add(approxSC2);
            }
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