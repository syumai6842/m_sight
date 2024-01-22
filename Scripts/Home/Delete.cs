using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delete : MonoBehaviour
{
    [SerializeField]
    MusicScrollView msv;

    public void AllDelete()
    {
        MusicDataContainer tmp = new MusicDataContainer();
        tmp.DataList = new List<MusicData>();
        SaveDataUtility.Save(tmp);
        msv.Refresh_MusicList();
    }

    
}
