using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVMarkerLessAR;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
public class PatternChecker : MonoBehaviour
{
    Mat grayMat;
    Mat patternMat;
    Pattern pattern;
    PatternDetector patternDetector;
    PatternTrackingInfo patternTrackingInfo;
    public void Set_PatternDetector(Texture2D texture)
    {
        patternMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(texture, patternMat);

        Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_BGR2RGB);

        pattern = new Pattern();
        patternTrackingInfo = new PatternTrackingInfo();
        patternDetector = new PatternDetector(null, null, null, true);

        patternDetector.buildPatternFromImage(patternMat, pattern);
        patternDetector.train(pattern);
    }


    public bool CheckPattern(string imgPach)
    {
        Mat rgbaMat = Imgcodecs.imread(imgPach);
        grayMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC1);
        Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_BGR2RGB);

        bool patternFound = patternDetector.findPattern(rgbaMat, patternTrackingInfo);

        System.IO.File.Delete(imgPach);
        return patternFound;
    }

}
