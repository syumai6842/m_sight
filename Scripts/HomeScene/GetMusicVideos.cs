using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using System.Security.Policy;

public class GetMusicVideo : MonoBehaviour
{
    MusicData md;
    [SerializeField]
    Text message;

    [SerializeField]
    InputField musicnamefield;

    [SerializeField]
    GameObject MetroEditButton;
    [SerializeField]
    GameObject MetroEditPanel;

    [SerializeField]
    RectTransform BPMSliderTF;
    [SerializeField]
    Slider BPMHandle;
    [SerializeField]
    InputField BPMPinNum;

    AudioClip audio1;
    AudioClip audio2;
    AudioClip metroaudio;


    static int barcount = 0;


    List<Sprite> score;
    



    private void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        init();
    }

    private void init()
    {
        MetroEditPanel.SetActive(false);
        MetroEditButton.SetActive(false);

        md = new MusicData();
        message.text = "";
        musicnamefield.text = "";
    }

    public void InputData(MusicData data)
    {
        md = data;
        musicnamefield.text = md.musicname;
        if (md.BPM == null)
        {
            MetroEditButton.SetActive(false);
        }
        else
        {
            MetroEditButton.SetActive(true);
        }

        PanelHandler.ToAddPanel();
    }


    public void SaveAll()
    {
        md.musicname = musicnamefield.text;
        if (md.musicname != "" && md.audio1_wave != null)
        {

            MusicDataContainer mdw = new MusicDataContainer();
            mdw.DataList = MusicDataManager.Load().DataList;

            mdw.DataList = mdw.DataList.Where(mdi => mdi.musicname != md.musicname).ToList();

            MusicDataManager.Save(mdw);

            for (int i = 0; File.Exists($"{Application.dataPath}/Resources/{md.musicname}_score{i}"); i++)
            {
                File.Delete($"{Application.dataPath}/Resources/{md.musicname}_score{i}");
            }



            if (md.audio2_wave == null)
            {
                md.audio2_wave = new float[audio1.samples];
                Array.Fill(md.audio2_wave,0);
            }
            md.audio2_wave = ArrangeClipLength(audio1,md.audio2_wave);
            if (md.metroaudio_wave == null)
            {
                md.metroaudio_wave = new float[audio1.samples];
                Array.Fill(md.metroaudio_wave, 0);
            }
            md.metroaudio_wave = ArrangeClipLength(audio1,md.metroaudio_wave);


            for (int i = 0; i < score.Count; i++)
            {
                byte[] bytesdata = score[i].texture.EncodeToJPG();
                File.WriteAllBytes($"{Application.dataPath}/Resources/{md.musicname}_score{i}.jpg",bytesdata);
            }


            //musicdata processing
            mdw.DataList = MusicDataManager.Load().DataList;


            mdw.DataList.Add(md);
            MusicDataManager.Save(mdw);

            ViewAllMusic.ListAllMusics();

            PanelHandler.ToSelectPanel();

            init();
        }
        else if (md.audio1_wave == null)
        {
            message.text = "演奏音源を選択してください";
        }
        else
        {
            message.text = "曲名を入力してください";
        }
    }

    public async void InputA1()
    {
        md.BPM = null;
        MetroEditButton.SetActive(false);

        string url = GetVideoPath();
        FFmpeg.Execute($"ffmpeg -i {url} -ar 44100 -q:a 0 -map a {Application.dataPath}/Resources/Tmp.mp3");
        audio1 = await AudioManager.LoadClip($"file://{Application.dataPath}/Resources/Tmp.mp3");
        md.audio1_wave = new float[audio1.samples];
        audio1.GetData(md.audio1_wave, 0);


        md.Scale = AudioManager.GetAllMusicalScale(md.audio1_wave);

        if (Application.platform == RuntimePlatform.Android)
        {
            File.Delete($"{Application.dataPath}/Resources/Tmp.mp3");
        }
    }

    public async void InputA2()
    {
        string url = GetVideoPath();
        FFmpeg.Execute($"ffmpeg -i {url} -ar 44100 -q:a 0 -map a {Application.dataPath}/Resources/Tmp.mp3");
        audio2 = await AudioManager.LoadClip($"file://{Application.dataPath}/Resources/Tmp.mp3");
        md.audio2_wave = new float[audio2.samples];
        audio2.GetData(md.audio2_wave, 0);

        if (Application.platform == RuntimePlatform.Android)
        {
            File.Delete($"{Application.dataPath}/Resources/Tmp.mp3");
        }
    }

    public async void UseMetro()
    {
        await PanelHandler.ToLoading();


        if (md.BPM == null)
        {
            if (md.audio1_wave != null)
            {


                float[] wave = md.audio1_wave;


                List<float[]> BPMData = AudioManager.GetSlicedBPM(wave, audio1.frequency);

                foreach (float[] value in BPMData)
                {
                    Debug.Log(value[1] + " : " + value[0] + "BPM");
                }

                md.BPM = BPMData;
                metroaudio = await AudioManager.getMetroAudioFromBPMAsync(BPMData,audio1);
                md.metroaudio_wave = new float[metroaudio.samples];
                Array.Fill(md.metroaudio_wave, 0);
                metroaudio.GetData(md.metroaudio_wave, 0);

                barcount = 0;
                foreach (float[] value in BPMData)
                {
                    barcount += (int)value[1];
                }

                MetroEditButton.SetActive(true);

                message.text = "BPM解析に成功しました";
            }
            else
            {
                message.text = "演奏音源を選択してください";
            }
        }
        else
        {
            md.BPM = null;
            md.metroaudio_wave = null;
            MetroEditButton.SetActive(false);
        }
        PanelHandler.Loading(1.0f);
    }

    public async void InputScore()
    {
        score = new List<Sprite>();

        string[] path = { "/Users/kanayanaokyou/Downloads/gakufu.png" };

        if (Application.platform == RuntimePlatform.Android)
        {
            path = await GetAllImagePath();
        }

        for (int i = 0; i < path.Length; i++)
        {
            path[i] = "file://" + path[i];
        }

        if (path != null)
        {
            foreach (string s in path)
            {
                score.Add(await LoadScore(s));
            }
        }

        message.text = "楽譜のインプットを完了";
    }

    public async static UniTask<Sprite> LoadScore(string uri)
    {
        var req = UnityWebRequestTexture.GetTexture(uri);

        await req.SendWebRequest();


        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("error occurred :" + req.error);
            return null;
        }

        var tex = ((DownloadHandlerTexture)req.downloadHandler).texture;
        Sprite result = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        return result;
    }


    //メトロノーム設定

    public int SelectedPinIndex;
    List<Button> BPMPins = new List<Button>();
    List<Slider> BPMHandles = new List<Slider>();

    public void EditMetro()
    {
        MetroEditPanel.SetActive(true);

        if (BPMSliderTF.childCount != 0)
        {
            foreach (Transform value in BPMSliderTF)
            {
                GameObject.Destroy(value.gameObject);
            }
        }



        int maxbars = 0;
        foreach (float[] value in md.BPM)
        {
            maxbars += (int)value[1];
        }

        int iterator = 0;
        for (int i = 0; i < md.BPM.Count; i++)
        {
            Slider obj = Instantiate(BPMHandle,BPMSliderTF);
            obj.maxValue = maxbars;
            obj.value = iterator;
            iterator += (int)md.BPM[i][1];
            obj.GetComponentInChildren<Text>().text = md.BPM[i][0].ToString();
            int a = i;
            obj.onValueChanged.AddListener((float value) =>
            {
                SelectedPinIndex = a;
            });
            BPMHandles.Add(obj);
        }

    }

    public void RemoveBPMHandle()
    {
        if (BPMHandles[SelectedPinIndex] != null)
        {
            Destroy(BPMHandles[SelectedPinIndex].gameObject);
            BPMHandles[SelectedPinIndex] = null;
        }
    }

    public void AddBPMHandle()
    {
        Slider obj = Instantiate<Slider>(BPMHandle, BPMSliderTF);
        obj.maxValue = barcount-1;
        obj.value = barcount-1;
        obj.GetComponentInChildren<Text>().text = BPMPinNum.text;
        int a = BPMHandles.Count;
        obj.onValueChanged.AddListener((float value) =>
        {
            SelectedPinIndex = a;
            Debug.Log("selected pin is " + SelectedPinIndex + " locate is " + value);
        });
        BPMHandles.Add(obj);
    }

    public async void SubmitBPMEdit()
    {
        for (int i = 0; i < BPMHandles.Count; i++)
        {
            if (BPMHandles[i] == null)
            {
                BPMHandles.RemoveAt(i);
            }
        }


        BPMHandles = BPMHandles.OrderBy(d => int.Parse(d.GetComponentInChildren<Text>().text)).ToList<Slider>();
        List<float[]> result = new List<float[]>();

        int previous_start = barcount;
        foreach (Slider slidervalue in BPMHandles)
        {
            float[] ram = {float.Parse(slidervalue.GetComponentInChildren<Text>().text), previous_start - slidervalue.value};
            result.Add(ram);
            previous_start = (int)slidervalue.value;
        }
        result.Reverse();


        md.BPM = result;

        foreach (float[] value in md.BPM)
        {
            Debug.Log(value[1] + " : " + value[0] + "BPM");
        }

        metroaudio = await AudioManager.getMetroAudioFromBPMAsync(result,audio1);
        metroaudio.GetData(md.metroaudio_wave, 0);

        MetroEditPanel.SetActive(false);
    }



    public void QuitBPMEdit()
    {
        MetroEditPanel.SetActive(false);
    }







    //ここからlib使う処理
    private string GetMediaPath()
    {
        string videopath = "Empty Path";

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery())
        {
            NativeGallery.Permission permission = NativeGallery.GetMixedMediaFromGallery((path) =>
            {
                Debug.Log("Media Path:" + path);

                if(path != null)
                {
                    switch (NativeGallery.GetMediaTypeOfFile(path))
                    {
                        case NativeGallery.MediaType.Image:Debug.Log("picked Image");break;
                        case NativeGallery.MediaType.Video:Debug.Log("picked Video");break;
                        default:Debug.Log("Picked something else");break;
                    }
                }
                videopath = path;
            },NativeGallery.MediaType.Image | NativeGallery.MediaType.Video,"Select an Image or video");

            Debug.Log("Permission result"+permission);
            
        }

        return videopath;
    }

    private string GetVideoPath()
    {
        string videopath = null;

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery())
        {
            NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
            {
                Debug.Log("Media Path:" + path);

                videopath = path;
            }, "Select an video");

            Debug.Log("Permission result" + permission);

        }

        return videopath;
    }

    private string GetImagePath()
    {
        string imagepath = null;

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery())
        {
            NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
                Debug.Log("Media Path:" + path);

                imagepath = path;
            }, "Select an image");

            Debug.Log("Permission result" + permission);

        }

        return imagepath;
    }

    private async UniTask<string[]> GetAllImagePath()
    {
        string[] imagepath = null;

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery())
        {
            NativeGallery.Permission permission = NativeGallery.GetImagesFromGallery((path) =>
            {
                Debug.Log("Media Path:" + path);

                imagepath = path;
            }, "Select images");

            Debug.Log("Permission result" + permission);

        }

        return imagepath;
    }

    private static float[] ArrangeClipLength(AudioClip ac_origin, float[] target_wave)
    {
        float[] result_wave = new float[ac_origin.samples];

        
        for (int i = 0; i < ac_origin.samples; i++)
        {
            if (i < target_wave.Length)
            {
                result_wave[i] = target_wave[i];
            }
            else
            {
                result_wave[i] = 0;
            }
        }
        return result_wave;
    }

}