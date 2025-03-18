using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
public class ThreePath : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "newThreePath.mp4");
        Debug.Log(path);
        videoPlayer.url = path;
        videoPlayer.Play();
    }
}