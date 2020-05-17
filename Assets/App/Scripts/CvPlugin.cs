using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CvPlugin : MonoBehaviour
{
    private static ILogger logger = Debug.unityLogger;
    private static string kTAG = "RealsenseUnity";

    private IntPtr instance;

    [DllImport("FaceDetection", EntryPoint = "_CreateFaceDetector")]
    private static extern IntPtr _CreateFaceDetector();

    [DllImport("FaceDetection", EntryPoint = "_GetOpenCvVersion")]
    private static extern IntPtr _GetOpenCvVersion(IntPtr instance);

    [DllImport("FaceDetection", EntryPoint = "_GetPluginName")]
    private static extern IntPtr _GetPluginName(IntPtr instance);

    [DllImport("FaceDetection", EntryPoint = "_InitializeCascade")]
    private static extern int _InitializeCascade(IntPtr instance, string cascadeFileString);

    [DllImport("FaceDetection", EntryPoint = "_StringTest")]
    private static extern IntPtr _StringTest(IntPtr instance, string input);

    [DllImport("FaceDetection", EntryPoint = "_Detect")]
    private static extern void _Detect(IntPtr instance, ref Color32[] rawImage, int width, int height);


    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        instance = _CreateFaceDetector();
#endif
    }

    public string GetOpenCvVersion()
    {
        Debug.Log("Instance in GetOpenCvVersion : " + instance);
        string ver = Marshal.PtrToStringAnsi(_GetOpenCvVersion(instance));
        return ver;
    }
    public int InitializeCascade(string cascadeFileString)
    {
        return _InitializeCascade(instance, cascadeFileString);
    }

    public string StringTest(string input)
    {
        return Marshal.PtrToStringAnsi(_StringTest(instance, input));
    }

    public void Detect(ref Color32[] rawImage, int width, int height)
    {
        _Detect(instance, ref rawImage, width, height);
    }
}
