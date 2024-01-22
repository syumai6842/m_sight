using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MusicScrollView : ScrollRect
{

    public void Refresh_MusicList()
    {
        GameObject PanelToSwitch = GameObject.Find("MusicElement");
        List<MusicData> musiclist = SaveDataUtility.Load().DataList;
        Debug.Log("refreshing " + musiclist.Count + "musics");

        if (Application.isPlaying)
        {
            foreach (Transform obj in this.content)
            {
                Destroy(obj.gameObject);
            }
        }


        foreach (MusicData value in musiclist)
        {
            GameObject obj = Instantiate(PanelToSwitch,this.content);
            obj.GetComponentInChildren<Text>().text = value.title;
            obj.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                MusicData CurrentValue = value;
                Debug.Log("music playing...");
                FindObjectsOfType<PlayMusic>(true)[0].InitPlay(CurrentValue);
            });
        }

    }

    protected override void Start()
    {
        Refresh_MusicList();
    }
}
