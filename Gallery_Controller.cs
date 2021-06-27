using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gallery_Controller : MonoBehaviour
{
    public event OnResultPacket_string OnResultPacket_PickVideo;
    public event OnResultPacket_string OnResultPacket_Error;

    public void PlayVideo(string path)
    {
        Handheld.PlayFullScreenMovie("file://" + path);
    }
    public void PickVideo()
    {
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            Debug.Log("Video path: " + path);
            if (path != null)
            {
                OnResultPacket_PickVideo("file://" + path);
            }
            else
                OnResultPacket_Error("Gallery_Controller/PickVideo");

        }, "Select a video");

        Debug.Log("Permission result: " + permission);
    }
}
