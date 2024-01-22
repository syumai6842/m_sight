using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDataUtility : MonoBehaviour
{
    public static string GetPath() { return Application.persistentDataPath + "/MusicData" + ".json"; }

    public static void Save(MusicDataContainer cont)
    {
        string jsondata = JsonUtility.ToJson(cont);
        using (StreamWriter sw = new StreamWriter(GetPath(), false))
        {
            sw.Write(jsondata);
        }

    }

    public static MusicDataContainer Load()
    {
        MusicDataContainer result = new MusicDataContainer();
        if (File.Exists(GetPath()))
        {
            try
            {
                using (FileStream fs = new FileStream(GetPath(), FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string ram = sr.ReadToEnd();
                    result = JsonUtility.FromJson<MusicDataContainer>(ram);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        else
        {
            MusicDataContainer mdc = new MusicDataContainer();
            mdc.DataList = new List<MusicData>();
            Save(mdc);
        }

        return result;
    }
}
