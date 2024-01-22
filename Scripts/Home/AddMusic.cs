using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativeGalleryNamespace;

public class AddMusic : MonoBehaviour
{
    [SerializeField]
    private GameObject MainPanel;
    [SerializeField]
    private GameObject solocheck;
    [SerializeField]
    private GameObject okescheck;
    [SerializeField]
    private GameObject scorecheck;
    [SerializeField]
    private GameObject titlecheck;
    [SerializeField]
    private InputField TitleField;

    private static MusicData Input_Data = new MusicData();



    private void Start()
    {
        ResetForm();
    }

    private void ResetForm()
    {
        Input_Data = new MusicData();
        solocheck.SetActive(false);
        okescheck.SetActive(false);
        scorecheck.SetActive(false);
        titlecheck.SetActive(false);
    }

    public void Submit()
    {
        
        if (Input_Data.title != null && Input_Data.SoloAudio_Path != null && Input_Data.Scores_Path != null)
        {
            Debug.Log("Submitting...");
            MusicDataContainer tmp = SaveDataUtility.Load();
            tmp.DataList.Add(Input_Data);
            SaveDataUtility.Save(tmp);
            ResetForm();
            PanelSwitcher.SwitchPanel(MainPanel);
        }
    }

    public void OnTitleChanged()
    {
        string String = TitleField.text;
        Input_Data.title = String;
        if (String != "" && String != null)
        {
            titlecheck.SetActive(true);
        }
        else
        {
            titlecheck.SetActive(false);
        }
        Debug.Log("title is " + String);
    }

    public void Add_SoloAudio()
    {
        GetVideoPath((path) =>
        {
            if (path != null)
            {
                Input_Data.SoloAudio_Path = path;
                solocheck.SetActive(true);
            }
        });

    }

    public void Add_OkesAudio()
    {
        GetVideoPath((path) =>
        {
            if (path != null)
            {
                Input_Data.OkesAudio_Path = path;
                okescheck.SetActive(true);
            }
        });


    }

    public void Add_Score()
    {
        GetImagesPath((path) =>
        {
            if (path != null)
            {
                Input_Data.Scores_Path = path;
                scorecheck.SetActive(true);
            }
        });
    }

    private void GetVideoPath(Action<string> onVideoSelected)
    {

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery() && !NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
            {
                Debug.Log("Media Path:" + path);

                onVideoSelected?.Invoke(path);
            }, "Select an video");

            Debug.Log("Permission result " + permission);

        }
    }

    private void GetImagesPath(Action<string[]> onImagesSelected)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            string[] tmp = { "/Users/kanayanaokyou/Pictures/IMG_1131.JPG" };
            Debug.Log("this is not android.");
        }

        if (NativeGallery.CanSelectMultipleMediaTypesFromGallery() && !NativeGallery.IsMediaPickerBusy())
        {
            NativeGallery.Permission permission = NativeGallery.GetImagesFromGallery((path) =>
            {
                onImagesSelected.Invoke(path);
            }, "Select an image");
        }
    }
}
