using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UI_Messenger : MonoBehaviour
{
    public RegisteredVideo_Win registeredVideo_Win;
    List<string> videoPath;
    public GameObject messageWin;
    public Text messageText;
    public GameObject loadingWin;
    public Text loadingText;
    public void OnMessage(string str)
    {
        Debug.Log(str);
        messageWin.SetActive(true);
        messageText.text = str;
    }
    public void LoadingWin(bool state)
    {
        loadingWin.SetActive(state);
    }
    public void SetLoadingText(string str1, string str2 = "")
    {
        loadingText.text = str1 + str2;
    }
    public void SetVideoFile(string path)
    {
        registeredVideo_Win.gameObject.SetActive(true);
        registeredVideo_Win.Add_VideoPath(path);
    }
    public void ReSetVideoFile()
    {
        registeredVideo_Win.ResetVideoPathCase();
    }
}
