using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class MusicDataManager
{
    public static string GetPath() { return Application.persistentDataPath + "/MusicData" + ".json"; }

    public static void Save(MusicDataContainer musicdatacontainer)
    {
        string jsondata = JsonUtility.ToJson(musicdatacontainer);

        using (StreamWriter sw = new StreamWriter(GetPath(),false))
        {
            try{
                sw.Write(jsondata);
            }catch (Exception e)
            {
                Debug.LogError(e);
            }
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
            Save(new MusicDataContainer());
        }

        return result;
    }
}
