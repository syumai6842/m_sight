using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using NRKernal.Record;
using NRKernal;
using static NRKernal.NRExamples.VideoCapture2LocalExample;
using UnityEngine.Rendering;
using System.Linq;
using System.IO;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField]
    GameObject MainPanel;

    [SerializeField]
    AudioSource AudioS;
    static AudioSource EffectSource;

    RecordAir record = null;

    private void Start()
    {
        EffectSource = AudioS;

        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.GetComponent<EventSystem>() != null)
            {
                obj.SetActive(true);
                Debug.Log("make a eventsystem true");
            }
        }
        SwitchPanel(MainPanel);

        record = this.gameObject.AddComponent<RecordAir>();
        record.StartRecord();
    }

    public static void SwitchPanel(GameObject Target_Panel)
    {
        Debug.Log("switching panel...");
        GameObject CanvasObj = GameObject.Find("Canvas").gameObject;
        foreach (Transform child in CanvasObj.transform)
        {
            if (child.gameObject == CanvasObj.gameObject) continue;
            child.gameObject.SetActive(false);
        }
        Target_Panel.SetActive(true);

        EffectSource.PlayOneShot(EffectSource.clip);
    }

    public void StopRecord()
    {
        record.StopVideoCapture();
    }

    private void OnApplicationQuit()
    {
        record.StopVideoCapture();
    }
}