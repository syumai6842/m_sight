using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewAllMusic : MonoBehaviour
{

    [SerializeField] Button sb;
    static Button SampleButton;
    [SerializeField] RectTransform rtf;
    static RectTransform ContentTF;

    private void Awake()
    {
        SampleButton = sb;
        ContentTF = rtf;
    }

    // Start is called before the first frame update
    void Start()
    {
        ListAllMusics();
        GameObject.Find("HomeEventSystem").SetActive(true);
    }

    public static void ListAllMusics()
    {
        if (ContentTF.childCount != 0)
        {
            foreach (Transform child in ContentTF)
            {
                Destroy(child.gameObject);
            }
        }
        


        MusicDataContainer mdw = new MusicDataContainer();
        mdw.DataList = MusicDataManager.Load().DataList;

        foreach (MusicData md in mdw.DataList)
        {
            Button obj = Instantiate(SampleButton, ContentTF);
            obj.GetComponentInChildren<Text>().text = md.musicname;
            string buttontext = md.musicname;
            obj.onClick.AddListener(() =>
            {
                PlaymdContainer.playmdname = mdw.DataList.Where(mdi => mdi.musicname == buttontext).ToList()[0].musicname;

                GameObject.Find("HomeEventSystem").SetActive(false);

                SceneManager.LoadScene("PlayScene");
            });

            Button[] obj_2 = obj.GetComponentsInChildren<Button>();

            obj_2[2].onClick.AddListener(() =>
            {
                GameObject.Find("ScriptObject").GetComponent<GetMusicVideo>().InputData(mdw.DataList.Where(mdi => mdi.musicname == buttontext).ToList()[0]);
            });

            obj_2[1].onClick.AddListener(() =>
            {
                MusicDataContainer mdw = new MusicDataContainer();
                mdw.DataList = MusicDataManager.Load().DataList;

                mdw.DataList = mdw.DataList.Where(mdi => mdi.musicname != buttontext).ToList();

                MusicDataManager.Save(mdw);

                for (int i = 0; File.Exists($"{Application.dataPath}/Resources/{buttontext}_score{i}"); i++)
                {
                    File.Delete($"{Application.dataPath}/Resources/{buttontext}_score{i}");
                }

                ListAllMusics();
            });



        }
    }

}