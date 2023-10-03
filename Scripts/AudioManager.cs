using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;


public class AudioManager : MonoBehaviour
{


    static int frame_length = 512;
    static int SliceBarlength = 8;


    public static float[] GetBPM(float[] original_wave,int freq)
    {
        if (original_wave == null)
        {
            Debug.Log("wave is null!!");
        }
        else
        {
            float count = 0;
            for (int i = 0;i < original_wave.Length;i++)
            {
                count += (float)Math.Abs(original_wave[i]);
            }

        }


        
        float[] frame_volume = new float[original_wave.Length/frame_length];
        System.Array.Fill(frame_volume, 0);
        //frame格納
        for (int i = 0; i < original_wave.Length / frame_length; i++)
        {
            for (int j = 0; j < frame_length; j++)
            {
                frame_volume[i] += (float)Math.Pow(original_wave[(frame_length * i) + j], 2);
            }
        }

        //差分求める
        float[] diff = new float[frame_volume.Length];
        System.Array.Fill(diff, 0);
        diff[0] = frame_volume[0];
        for (int i = 1; i < diff.Length; i++)
        {
            diff[i] = frame_volume[i] - frame_volume[i - 1];
            diff[i] = diff[i] < 0 ? 0 : diff[i];
        }


        //スコア算出
        float bpm;
        float[] result = new float[3];
        double[] scoreR = new double[3];
        System.Array.Fill <double>(scoreR,0);

        for (bpm = 60; bpm <= 200; bpm++)
        {
            double s = 0;
            double c = 0;



            for (int i = 0; i < diff.Length; i++)
            {
                double winsample = hanning(i, diff.Length);
                s += diff[i] * Math.Sin(2.0 * Math.PI * (double)(bpm / 60) * ((double)(i * frame_length) / freq)) * winsample;
                c += diff[i] * Math.Cos(2.0 * Math.PI * (double)(bpm / 60) * ((double)(i * frame_length) / freq)) * winsample;
            }

            
            double score = Math.Sqrt((s*s)+(c*c));


            if (scoreR[2] < score)
            {
                if (scoreR[0] < score)
                {
                    scoreR[2] = scoreR[1];
                    scoreR[1] = scoreR[0];
                    scoreR[0] = score;

                    result[2] = result[1];
                    result[1] = result[0];
                    result[0] = bpm;
                }
                else if (scoreR[1] < score)
                {
                    scoreR[2] = scoreR[1];
                    scoreR[1] = score;

                    result[2] = result[1];
                    result[1] = bpm;
                }
                else
                {
                    scoreR[2] = score;

                    result[2] = bpm;
                }
            }
        }

        System.Array.Sort(result);

        double ss = 0;
        double cs = 0;
        for (int i = 0; i < diff.Length; i++)
        {
            double winsample = hanning(i,diff.Length);
            ss += diff[i] * Math.Sin(2 * Math.PI * (result[1] / 60) * ((i * frame_length) / freq))*winsample;
            cs += diff[i] * Math.Cos(2 * Math.PI * (result[1] / 60) * ((i * frame_length) / freq))*winsample;
        }

        double theta = Math.Atan2(ss, cs);
        theta = theta < 0 ? theta + 2.0 * Math.PI : theta;
        double phase = theta / (2 * Math.PI * (bpm / 60));

        result[0] = result[1];
        result[1] = (float)phase;


        
        return result;
    }

    public static List<float[]> GetSlicedBPM(float[] original_wave, int freq)
    {
        float[] audiodata = GetBPM(original_wave, freq);
        int phase = (int)(audiodata[1] * (60 / audiodata[0]) * freq) ;
        int iterator = 0;

        List<float[]> result = new List<float[]>();


        PanelHandler.Loading(0.1f);


        for (int i = 0; i < (original_wave.Length - phase) / freq / SliceBarlength; i++)
        {

            ArraySegment<float> a = new ArraySegment<float>(original_wave, phase + (i * freq * SliceBarlength), freq * SliceBarlength - 1);
            float fetched_bpm = GetBPM(a.ToArray(), freq)[0];

            if (i != 0)
            {
                if (fetched_bpm == result[iterator][0])
                {
                    result[iterator][1]++;
                }
                else
                {
                    Debug.LogError("bpm is changed!! " + fetched_bpm);
                    iterator++;
                    float[] ram = { fetched_bpm, 1 };
                    result.Add(ram);
                }
            }
            else
            {
                 float[] ram = { fetched_bpm, 1 };
                 result.Add(ram);
            }
        }
        PanelHandler.Loading(0.9f);

        return result;
    }

    public static async UniTask<AudioClip> LoadClip(string uri)
    {
        AudioType at;
        switch (System.IO.Path.GetExtension(uri))
        {
            case "wav":
                at = AudioType.WAV;
                break;
            default:
                at = AudioType.MPEG;
                break;
        };

        var req = UnityWebRequestMultimedia.GetAudioClip(uri,at);
        ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = false;

        await req.SendWebRequest();

        
        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("error occurred :"+req.error);
            return null;
        }

        return DownloadHandlerAudioClip.GetContent(req);
    }

    private static double hanning(double n,double N)
    {
        return 0.5 * (1.0 - Math.Cos(n / N));
    }

    public static async UniTask<AudioClip> getMetroAudioFromBPMAsync(List<float[]> bpm, AudioClip audio)
    {
        AudioClip clickaudio = await AudioManager.LoadClip($"file://{Application.dataPath}/Resources/Click.mp3");
        float[] clickwave = new float[clickaudio.samples * clickaudio.channels];
        clickaudio.GetData(clickwave,0);

        float[] audiowave = new float[audio.samples * audio.channels];
        audio.GetData(audiowave, 0);

        float[] result = new float[(int)(audio.length * clickaudio.frequency * clickaudio.channels)];
        Array.Fill(result,0);

        PanelHandler.Loading(0.1f);
        float[] audiodata = GetBPM(audiowave, audio.frequency);
        int iterator = (int)(audiodata[1] * (60 / audiodata[0]) * clickaudio.frequency);


        for (int i = 0; i < bpm.Count;i++)
        {
            int c_iterator = iterator;
            float MaxSampleOfBPM = bpm[i][1] * clickaudio.frequency * SliceBarlength + c_iterator;

            while (iterator < MaxSampleOfBPM)
            {

                for (int s = 0; s < clickwave.Length; s++)
                {
                    result[iterator + s] = clickwave[s];
                }


                if (iterator + (clickwave.Length) + (int)(60 / bpm[i][0] * clickaudio.frequency) >= MaxSampleOfBPM)
                {
                    iterator = (int)MaxSampleOfBPM;
                    break;
                }
                else
                {
                    iterator += (int)(60 / bpm[i][0] * clickaudio.frequency);
                }
            }
        }

        PanelHandler.Loading(0.7f);

        AudioClip ac = AudioClip.Create("metro",result.Length,1,clickaudio.frequency,false);
        ac.SetData(result,0);

        PanelHandler.Loading(0.9f);

        return ac;
    }

    public static float GetNote(float[] wave)
    {
        float threshold = 0.1f;

        AudioSource audios = GameObject.Find("ScriptObject").GetComponent<AudioSource>();
        audios.clip = AudioClip.Create("todetect",wave.Length,1,44100,false);
        audios.clip.SetData(wave, 0);



        if (Math.Abs(wave.Max() - wave.Min()) <= threshold)
        {
            return 999;
        }

        float[] spectrum = new float[8192];
        audios.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        int maxIndex = Array.IndexOf(spectrum, spectrum.Max());
        float hertz = maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length;

        float note = -28;
        float currentdiff = (float)Math.Pow(440 * Math.Pow(2.0, -28 / 12),2);
        for (int i = -28;i < 24; i++)
        {
            if (currentdiff > (float)Math.Pow(440 * Math.Pow(2.0, i / 12),2))
            {
                note = i;
            }
        }

        return note;
    }

    public static List<float> GetAllMusicalScale(float[] wave)
    {
        int freq = 44100;

        List<float> result = new List<float>();

        for (int i = 0;i < wave.Length/(freq*0.1) - 1; i++)
        {
            Debug.LogWarning((int)(i * (wave.Length / (freq * 0.1))) + " / " + (int)((i * (wave.Length / (freq * 0.1)))+(int)(freq * 0.1 - 1)) + "->" + wave.Length);
            float[] ramwave = new ArraySegment<float>(wave,(int)(i*(freq*0.1)),(int)(freq*0.1-1)).ToArray();
            float rampitch = GetNote(ramwave);

            result.Add(rampitch);
        }

        return result;
    }
}
