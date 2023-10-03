using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MusicData
{
    public string musicname;

    public float[] audio1_wave;
    public float[] audio2_wave;
    public float[] metroaudio_wave;

    public List<float[]> BPM;
    public List<float> Scale;
}
