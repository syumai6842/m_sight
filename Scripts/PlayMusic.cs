using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class PlayMusic : MonoBehaviour
{
    [SerializeField]
    GameObject MainPanel;
    [SerializeField]
    Text MusicTitleUI;
    [SerializeField]
    VideoPlayer Solo_audio;
    [SerializeField]
    VideoPlayer Okes_audio;
    [SerializeField]
    GameObject scoreobj;
    [SerializeField]
    ScrollRect score_view;
    [SerializeField]
    GameObject UI_Panel;


    float PlayedRate = 0;

    Text CountDown;
    Slider seekbar;
    GameObject PauseButton;
    GameObject PlayButton;
    bool isSeekbarControlled = false;

    private void CountDownText()
    {
        float current_time = Time.time;

        int maxsecond = int.Parse(CountDown.text);
        CountDown.text = (maxsecond - 1).ToString();
        if (maxsecond - 1 <= 0)
        {
            CancelInvoke();
            CountDown.gameObject.SetActive(false);
            PauseButton.SetActive(true);
            PlayButton.SetActive(false);
            PlayAudios();
        }
    }

    private void Start()
    {
        seekbar = UI_Panel.GetComponentInChildren<Slider>();
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>(true))
        {
            if (obj.name == "Play")
            {
                PlayButton = obj;
            }
            else if (obj.name == "Pause")
            {
                PauseButton = obj;
            }else if (obj.name == "CountDown")
            {
                CountDown = obj.GetComponent<Text>();
            }
        }

        CountDown.gameObject.SetActive(false);
        seekbar.value = 0;
        Pause();
        
    }

    public async void InitPlay(MusicData PlayingMusic)
    {
        MusicTitleUI.text = PlayingMusic.title;

        Solo_audio.url = "file://" + PlayingMusic.SoloAudio_Path;
        Okes_audio.url = "file://" + PlayingMusic.OkesAudio_Path;


        foreach (Transform obj in score_view.content.gameObject.transform)
        {
            if (obj == score_view.content.gameObject.transform) continue;
            Destroy(obj.gameObject);
        }

        foreach (string value_path in PlayingMusic.Scores_Path)
        {
            Debug.Log("will load "+ value_path);
            Sprite value = await AudioManager.LoadScore(value_path);
            GameObject obj = Instantiate(scoreobj, score_view.content.gameObject.transform);
            obj.GetComponentInChildren<Image>().sprite = value;
            float scoreaspect = value.rect.width / value.rect.height;
            obj.GetComponentInChildren<AspectRatioFitter>().aspectRatio = scoreaspect;
            ((RectTransform)obj.transform).sizeDelta = ((RectTransform)score_view.transform).sizeDelta;
        }

        PanelSwitcher.SwitchPanel(this.gameObject);
    }

    private void UpdateAudioFrame(long frame)
    {
        Solo_audio.frame = frame;
        float PlayedRate = (float)frame / (float)Solo_audio.frameCount;
        seekbar.value = PlayedRate;
        score_view.verticalNormalizedPosition = 1 - PlayedRate;
        if (Okes_audio.url != "file://" && Okes_audio.url != null)
        {
            Okes_audio.frame = frame;
        }
    }

    private void PauseAudios()
    {
        Solo_audio.Pause();
        if (Okes_audio.url != "file://" && Okes_audio.url != null)
        {
            Okes_audio.Pause(); ;
        }
    }

    private void PlayAudios()
    {
        Solo_audio.Play();
        if (Okes_audio.url != "file://" && Okes_audio.url != null)
        {
            Okes_audio.Play();
        }
    }


    private void Update()
    {
        if (Solo_audio.isPlaying)
        {
            PlayedRate = (float)Solo_audio.frame/(float)Solo_audio.frameCount;
            isSeekbarControlled = true;
            UI_Panel.GetComponentInChildren<Slider>().value = PlayedRate;
            isSeekbarControlled = false;
            score_view.verticalNormalizedPosition = 1 - PlayedRate;
        }
    }

    public void Play()
    {
        Debug.Log("play");
        PauseAudios();
        CountDown.gameObject.SetActive(true);
        CountDown.text = 4.ToString();
        InvokeRepeating("CountDownText",0f,1f);
    }

    public void Pause()
    {
        Debug.Log("pause");
        PauseAudios();
        PauseButton.SetActive(false);
        PlayButton.SetActive(true);
    }

    public void Skip()
    {
        Debug.Log("skip");
        long tmp_frame = (long)(Solo_audio.frame + Solo_audio.frameRate * 5);
        isSeekbarControlled = true;
        UpdateAudioFrame((tmp_frame >= (long)Solo_audio.frameCount) ? (long)Solo_audio.frameCount : tmp_frame);
        isSeekbarControlled = false;
        Pause();
    }

    public void Previous()
    {
        Debug.Log("previous");
        long tmp_frame = (long)(Solo_audio.frame - Solo_audio.frameRate * 5);
        isSeekbarControlled = true;
        UpdateAudioFrame((tmp_frame <= (long)Solo_audio.frameCount) ? 0 : tmp_frame);
        isSeekbarControlled = false;
        Pause();
    }

    public void Begin()
    {
        Debug.Log("begin");
        UpdateAudioFrame(0);
        Pause();
    }

    public void OnSeekBarChanged()
    {
        if (!isSeekbarControlled)
        {
            Pause();
            UpdateAudioFrame((long)(seekbar.value * Solo_audio.frameCount));
            Debug.Log(seekbar.value * Solo_audio.frameCount + "->" + seekbar.value);
            score_view.verticalNormalizedPosition = seekbar.value;
        }
    }



    public void BackToHome()
    {
        PauseAudios();

        PanelSwitcher.SwitchPanel(MainPanel);
    }
}
