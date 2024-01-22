using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class AudioManager : MonoBehaviour
{
    public static async UniTask<AudioClip> LoadClip(string target_uri)
    {
        string uri = "file://" + target_uri;
        AudioType at;
        switch (System.IO.Path.GetExtension(uri))
        {
            case "wav":
                at = AudioType.WAV;
                break;
            default:
                at = AudioType.MPEG;
                break;
        };

        var req = UnityWebRequestMultimedia.GetAudioClip(uri, at);
        ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = false;

        await req.SendWebRequest();


        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("error occurred :" + req.error);
            return null;
        }

        return DownloadHandlerAudioClip.GetContent(req);
    }

    public async static UniTask<Sprite> LoadScore(string target_uri)
    {
        string uri = "file://" + target_uri;

        var req = UnityWebRequestTexture.GetTexture(uri);

        await req.SendWebRequest();


        if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("error occurred :" + req.error);
            return null;
        }

        var tex = ((DownloadHandlerTexture)req.downloadHandler).texture;
        Sprite result = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        Debug.Log("Score loaded : "+ result.texture.width);
        return result;
    }
}
