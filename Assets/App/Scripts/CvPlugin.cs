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
    private static extern IntPtr _Detect(IntPtr instance, IntPtr input, IntPtr processed, int width, int height);


    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        //instance = _CreateFaceDetector();
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

    public void Detect(Texture2D input, ref Texture2D processed, int width, int height)
    {
        Color32[] inputPixelData = input.GetPixels32();
        Color32[] processedPixelData = processed.GetPixels32();

        GCHandle inputPixelHandle = GCHandle.Alloc(inputPixelData, GCHandleType.Pinned);
        IntPtr inputPixelPtr = inputPixelHandle.AddrOfPinnedObject();

        GCHandle processedPixelHandle = GCHandle.Alloc(processedPixelData, GCHandleType.Pinned);
        IntPtr processedPixelPtr = processedPixelHandle.AddrOfPinnedObject();

        _Detect(instance, inputPixelPtr, processedPixelPtr, input.width, input.height);
    }
}
