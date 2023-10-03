using UnityEngine;

public class FFmpeg
{
    public static int Execute(string command)
    {
        if (Application.platform == RuntimePlatform.Android) {
            using (AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.mobileffmpeg.Config"))
            {
                AndroidJavaObject paramVal = new AndroidJavaClass("com.arthenica.mobileffmpeg.Signal").GetStatic<AndroidJavaObject>("SIGXCPU");
                configClass.CallStatic("ignoreSignal", new object[] { paramVal });

                using (AndroidJavaClass ffmpegbody = new AndroidJavaClass("com.arthenica.mobileffmpeg.FFmpeg"))
                {
                    int code = ffmpegbody.CallStatic<int>("execute", new object[] { command });
                    return code;
                }
            }
        }
        else
        {
            return 0;
        }
    }

    public static int Cancel()
    {
        if (Application.platform == RuntimePlatform.Android) {
            using (AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.mobileffmpeg.Config"))
            {
                using (AndroidJavaClass ffmpegbody = new AndroidJavaClass("com.arthenica.mobileffmpeg.FFmpeg"))
                {
                    int code = ffmpegbody.CallStatic<int>("cancel");
                    return code;
                }
            }
        }
        else
        {
            return 0;
        }
    }
}