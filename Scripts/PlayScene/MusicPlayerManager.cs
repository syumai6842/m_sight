using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;   
using UnityEngine.SceneManagement;

using NRKernal;
using System;

public class MusicPlayerManager : MonoBehaviour
{
    [SerializeField]
    AudioSource audio1;
    [SerializeField]
    AudioSource audio2;
    [SerializeField]
    AudioSource metroaudio;

    [SerializeField]
    RectTransform scoreviewTF;
    [SerializeField]
    GameObject scoreobj;

    [SerializeField]
    ScrollRect scorescrollrect;

    [SerializeField]
    Text MusicnameTitle;

    [SerializeField]
    GameObject AV_UI;

    [SerializeField]
    AudioSource micaudiosource;
    [SerializeField]
    GameObject WrongLine;

    [SerializeField]
    GameObject scoreText;
    [SerializeField]
    GameObject ReactionColor;

    MusicData playmd;

    bool isplaying = true;
    float AudioLength;

    AudioClip audio1clip;
    AudioClip audio2clip;
    AudioClip MetroAudioclip;

    List<Sprite> score = new List<Sprite>();
    List<GameObject> Lines = new List<GameObject>();
    List<float> Lines_time = new List<float>();

    // Start is called before the first frame update
    async void Start()
    {
        GameObject.Find("NRCameraRig").GetComponentInChildren<AudioListener>().enabled = true;
        GameObject.Find("NRCameraRig").GetComponentInChildren<Camera>().enabled = true;

        MusicDataContainer mdw = new MusicDataContainer();
        mdw.DataList = MusicDataManager.Load().DataList;

        Debug.LogWarning("Saved playmd musicname is " + PlaymdContainer.playmdname);

        if (mdw.DataList.Where(mdi => mdi.musicname == PlaymdContainer.playmdname).ToList().Count != 0)
        {
            playmd = mdw.DataList.Where(mdi => mdi.musicname == PlaymdContainer.playmdname).ToList()[0];
        }
        else
        {
            Debug.LogWarning("there is no same named data");
            AudioLength = 0;
            Quit();
        }

        Debug.Log("loaded music name is " + playmd.musicname);



        if (playmd.audio1_wave == null)
        {
            Debug.LogWarning("audio1 is null");
            AudioLength = 0;
            Quit();
        }
        else
        {
            audio1clip = AudioClip.Create("audio1clip", playmd.audio1_wave.Length, 1, 44100, false);
            audio1clip.SetData(playmd.audio1_wave, 0);
            audio2clip = AudioClip.Create("audio2clip", playmd.audio2_wave.Length, 1, 44100, false);
            audio2clip.SetData(playmd.audio2_wave, 0);
            MetroAudioclip = AudioClip.Create("metroclip", playmd.metroaudio_wave.Length, 1, 44100, false);
            MetroAudioclip.SetData(playmd.metroaudio_wave, 0);


            Debug.LogWarning("file " + $"{Application.dataPath}/Resources/{playmd.musicname}_score0.jpg" + " is " + File.Exists($"{Application.dataPath}/Resources/{playmd.musicname}_score0").ToString());
            for (int i = 0; File.Exists($"{Application.dataPath}/Resources/{playmd.musicname}_score{i}.jpg"); i++)
            {
                Sprite ram = await GetMusicVideo.LoadScore($"file://{Application.dataPath}/Resources/{playmd.musicname}_score{i}.jpg");
                score.Add(ram);
                Debug.Log($"{i+1} times loaded score image");
            }


            MusicnameTitle.text = playmd.musicname;

            audio1.clip = audio1clip;
            audio2.clip = audio2clip;
            metroaudio.clip = MetroAudioclip;
            AudioLength = audio1clip.length;



            if (score != null) {
                foreach (Sprite value in score)
                {
                    GameObject obj = Instantiate(scoreobj, scoreviewTF);
                    obj.GetComponentInChildren<Image>().sprite = value;
                    float scoreaspect = value.rect.width / value.rect.height;
                    obj.GetComponentInChildren<AspectRatioFitter>().aspectRatio = scoreaspect;
                    ((RectTransform)obj.transform).sizeDelta = scoreviewTF.sizeDelta;
                }
            }
            scorescrollrect.GetComponentInChildren<ContentSizeFitter>().SetLayoutVertical();
            scorescrollrect.SetLayoutVertical();
        }
        
        micaudiosource.clip = Microphone.Start(null,true,1,44100);
        while (!(Microphone.GetPosition("") > 0)) { }

        micaudiosource.Play();
        StartCoroutine(JudgeMusicalScale());

        GameObject.Find("PlayEventSystem").SetActive(true);
    }

    private void Update()
    {
        if (isplaying && audio1.clip != null)
        {
            scorescrollrect.verticalNormalizedPosition = 1 - (audio1.time / audio1.clip.length);
        }
    }

    private IEnumerator JudgeMusicalScale()
    {
        float current;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            float[] ramwave = new float[4410];
            micaudiosource.GetOutputData(ramwave, 0);
            current = AudioManager.GetNote(ramwave);
            if (isplaying && current != 999)
            {
                int index = (int)(audio1.time / 0.1);
                int diff_timing = 10;
                int diff_scale = 10;
                for (int count = (int)(index + 5 < (audio1.clip.length*10) ? index + 5: audio1.clip.length * 10); count < 5 || count < 0;count--)
                {
                    if (Mathf.Abs(playmd.Scale[index+count] - current) < 4 && Mathf.Abs(count) < Mathf.Abs(diff_timing))
                    {
                        diff_timing = count;
                        diff_scale = (int)Mathf.Abs(playmd.Scale[index + count] - current);
                    }
                }

                if (diff_timing == 10)
                {
                    AddLine(0,index);
                  
                }else if (diff_timing > 2 || diff_scale < 2)
                {
                    AddLine(1,index);
                }
                else
                {
                    AddLine(2, index);
                }
            }
        }
    }

    public void AddLine(int sucref,int index)
    {
        GameObject obj = Instantiate(WrongLine, (RectTransform)AV_UI.GetComponentInChildren<Slider>().gameObject.transform);
        obj.transform.localPosition = new Vector3((obj.transform.parent.transform.localScale.x / audio1.clip.length / 10) * index, 0, 0);
        switch (sucref)
        {
            case 0:
                obj.GetComponent<Image>().color = Color.red;
                break;
            case 1:
                obj.GetComponent<Image>().color = Color.blue;
                break;
            case 2:
                obj.GetComponent<Image>().color = Color.green;
                break;
        }
        Lines.Add(obj);
        Lines_time.Add(audio1.time);
    }

    public void Reaction(int sucref)
    {
        Animator animator;
        animator = ReactionColor.GetComponent<Animator>();
        Image textcomp = ReactionColor.GetComponent<Image>();

        switch (sucref)
        {
            case 0:
                textcomp.color = Color.red;
                animator.Play("Fade");
                break;

            case 1:
                textcomp.color = Color.blue;
                animator.Play("Fade");
                break;

            case 2:
                textcomp.color = Color.green;
                animator.Play("Fade");
                break;

        }
    }

    public void Play(Button pausebutton)
    {

        GameObject.Find("Play").SetActive(false);
        pausebutton.gameObject.SetActive(true);

        audio1.Play();
        audio2.Play();
        metroaudio.Play();
        isplaying = true;
    }

    public void RePlay()
    {
        audio1.time = audio1.time - 5;
        audio2.time = audio2.time - 5;
        metroaudio.time = metroaudio.time - 5;
    }

    public void Forward()
    {
        audio1.time = audio1.time + 5;
        audio2.time = audio2.time + 5;
        metroaudio.time = metroaudio.time + 5;
    }

    public void Previous()
    {
        AV_UI.GetComponentInChildren<Slider>().value = 0;
    }

    public void OnSeakBarChanged(Slider slider)
    {
        float value = slider.value;
        audio1.time = audio1.clip.length * value;
        audio2.time = audio2.clip.length * value;
        metroaudio.time = metroaudio.clip.length * value;

        Lines_time = (List<float>)Lines_time.Where(s =>
        {
            return audio1.time > s;
        }).ToList();
        foreach (GameObject lineobj in Lines.GetRange(Lines_time.Count(),Lines.Count()-Lines_time.Count()))
        {
            Destroy(lineobj);
        }
        Lines = Lines.GetRange(0, Lines_time.Count());
    }


    public void Pause(Button playbutton)
    {
        playbutton.gameObject.SetActive(true);
        GameObject.Find("Pause").SetActive(false);
        audio1.Pause();
        audio2.Pause();
        metroaudio.Pause();
        isplaying = false;
    }

    public void Quit()
    {
        GameObject.Find("NRCameraRig").GetComponentInChildren<AudioListener>().enabled = false;
        GameObject.Find("NRCameraRig").GetComponentInChildren<Camera>().enabled = false;
        GameObject.Find("PlayEventSystem").SetActive(false);


        audio1clip.UnloadAudioData();
        audio2clip.UnloadAudioData();
        MetroAudioclip.UnloadAudioData();

        PlaymdContainer.playmdname = null;
        SceneManager.LoadScene("MenuScene");
        
    }
}