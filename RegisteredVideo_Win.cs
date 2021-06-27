using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RegisteredVideo_Win : MonoBehaviour
{
    List<string> videoPathCase = new List<string>();
    public GameObject videoButtonCase;
    public Gallery_Controller Gallery_Controller;

    public void Add_VideoPath(string path)
    {
        videoPathCase.Add(path);
        SettingVideoButton();
    }
    public void ResetVideoPathCase()
    {
        videoPathCase.Clear();
        SettingVideoButton();
    }
    void SettingVideoButton()
    {
        int count = videoPathCase.Count;
        foreach (Transform obj in videoButtonCase.transform)
        {
            if(count > 0)
            {
                count--;
                obj.gameObject.SetActive(true);
            }
            else
                obj.gameObject.SetActive(false);
        }
    }
    public void OnClickVideoButton(int num)
    {
        if (videoPathCase.Count > num)
            Gallery_Controller.PlayVideo(videoPathCase[num]);
    }
}
