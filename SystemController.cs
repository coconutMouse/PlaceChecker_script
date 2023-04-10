using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MarkerLessARExample;

public delegate void OnResultPacket();
public delegate void OnResultPacket_string(string str);
public delegate void OnResultPacket_ListString(List<string> str);

public class SystemController : Singleton<SystemController>
{
    public RawImage rawImage;
    public CapturePattern capturePattern;
    public Gallery_Controller gallery_Controller;
    public FirebaseDB_Manager firebaseDB_Manager;
    public FirebaseStorage_Manager firebaseStorage_Manager;
    public PatternChecker patternChecker;
    public UI_Messenger ui_Messenger;
    public string deviceID;
    public string deviceDataPath;

    private static Queue<Packet> dataQueue = new Queue<Packet>();
    private object lockObject = new object();
    private object lockObject_Twxt = new object();
    private string videoPath = "";
    private int PacketCheckCount = 0;
    private List<string> imgNamePatternTrue;
    private bool running;
    private string loadingTitleText;
    private static string loadingeRsultText;

    public void ButtonClick_Capture()
    {
        capturePattern.OnCaptureButtonClick();  //rawImage = data
    }
    public void ButtonClick_Save()
    {
        if (rawImage.texture == null)
            return;
        running = true;
        ReSetDataQueue();
        loadingTitleText = "loading...";
        ui_Messenger.LoadingWin(true);
        ui_Messenger.SetLoadingText(loadingTitleText);
        gallery_Controller.PickVideo();
    }
    public void ButtonClick_Load()
    {
        if (rawImage.texture == null)
            return;
        running = true;
        ReSetDataQueue();
        loadingTitleText = "loading...";
        ui_Messenger.LoadingWin(true);
        ui_Messenger.SetLoadingText(loadingTitleText);
        firebaseDB_Manager.LoadDB_AllName();
    }
    public void ReSetDataQueue()
    {
        dataQueue.Clear();
    }

    private void Start()
    {
        imgNamePatternTrue = new List<string>();
        deviceID = SystemInfo.deviceUniqueIdentifier;
        deviceDataPath = Application.persistentDataPath;

        gallery_Controller.OnResultPacket_Error += AddPacket_Error;
        gallery_Controller.OnResultPacket_PickVideo += AddPacket_VideoPath;

        firebaseDB_Manager.OnResultPacket_Error += AddPacket_Error;
        firebaseDB_Manager.OnResultPacket_AddDB_Name += AddPacket_SaveStorage;
        firebaseDB_Manager.OnResultPacket_LoadDB_AllName += AddPacket_SetImgNameCase;

        firebaseStorage_Manager.OnResultPacket_Error += AddPacket_Error;
        firebaseStorage_Manager.OnResultPacket_Save += AddPacket_CheckSaveStorage;
        firebaseStorage_Manager.OnResultPacket_DownloadImg += AddPacket_CheckImgPattern;
        firebaseStorage_Manager.OnResultPacket_DownloadVideo += AddPacket_CheckLoadVideo;

        firebaseStorage_Manager.OnResultPacket_loadProgress += AddPacket_loadProgress;

    }
    private void Update()
    {
        DataProcessing();
    }
    private void DataProcessing()
    {
        if (running == false)
            return;
        if (dataQueue.Count == 0)
            return;
          
        Packet packet;
        lock (lockObject)
        {
            packet = dataQueue.Dequeue();
        }
        switch (packet.id)
        {
            case PacketIDCode.Error:
                {
                    string str = ((String_Packet)packet).str;
                    ui_Messenger.OnMessage("Error : " + str);
                    running = false;
                    ui_Messenger.LoadingWin(false);
                }
                break;
            case PacketIDCode.SetVideoPath:
                {
                    videoPath = ((String_Packet)packet).str;
                    firebaseDB_Manager.AddDB_Name(deviceID);
                }
                break;
            case PacketIDCode.SaveStorage:
                {
                    loadingTitleText = "SaveData : ";
                    string imgName = ((String_Packet)packet).str;
                    firebaseStorage_Manager.UploadTexture2D(imgName, (Texture2D)rawImage.texture);
                    firebaseStorage_Manager.UploadVideo(imgName, videoPath);
                    PacketCheckCount = 2;
                }
                break;
            case PacketIDCode.CheckSaveStorage:
                {
                    PacketCheckCount--;
                    if (PacketCheckCount <= 0)
                    {
                        ui_Messenger.OnMessage("Finish");
                        ui_Messenger.LoadingWin(false);
                        running = false;
                    }
                }
                break;
            case PacketIDCode.SetImgNameCase:
                {
                    loadingTitleText = "Load Data CheckImg Pattern : ";
                    List<string> imgNameCase = ((ListString_Packet)packet).str_L;
                    foreach (string str in imgNameCase)
                        firebaseStorage_Manager.DownloadTexture2D(str, deviceDataPath);
                    PacketCheckCount = imgNameCase.Count;
                    patternChecker.Set_PatternDetector((Texture2D)rawImage.texture);
                }
                break;
            case PacketIDCode.CheckImgPattern:
                {
                    string imgName = ((String_Packet)packet).str;
                    string imgPath = Path.Combine(deviceDataPath, string.Format(imgName + ".png"));
                    bool check = patternChecker.CheckPattern(imgPath);
                    if (check)
                        imgNamePatternTrue.Add(imgName);

                    PacketCheckCount--;
                    if (PacketCheckCount <= 0)
                    {
                        if(imgNamePatternTrue.Count == 0)
                        {
                            ui_Messenger.OnMessage("There is no same image data.");
                            ui_Messenger.LoadingWin(false);
                            running = false;
                            break;
                        }
                        loadingTitleText = "Load Video Data : ";
                        foreach (string str in imgNamePatternTrue)
                            firebaseStorage_Manager.DownloadVideo(str, deviceDataPath);
                        PacketCheckCount = imgNamePatternTrue.Count;
                        ui_Messenger.ReSetVideoFile();
                    }
                }
                break;
            case PacketIDCode.CheckLoadVideo:
                {
                    string videoPath = ((String_Packet)packet).str;
                    ui_Messenger.SetVideoFile(videoPath);
                    PacketCheckCount--;
                    if (PacketCheckCount <= 0)
                    {
                        ui_Messenger.LoadingWin(false);
                        running = false;
                    }
                }
                break;
        }
    }

    private void AddPacket_Error(string str)
    {
        Packet packet = (Packet)(new String_Packet(PacketIDCode.Error, str));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_VideoPath(string path)
    {
        Packet packet = (Packet)(new String_Packet(PacketIDCode.SetVideoPath, path));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_SaveStorage(string path)
    {
        Packet packet = (Packet)(new String_Packet(PacketIDCode.SaveStorage, path));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_CheckSaveStorage()
    {
        Packet packet = new Packet(PacketIDCode.CheckSaveStorage);
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_SetImgNameCase(List<string> nameCase)
    {
        Packet packet = (Packet)(new ListString_Packet(PacketIDCode.SetImgNameCase, nameCase));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_CheckImgPattern(string name)
    {
        Packet packet = (Packet)(new String_Packet(PacketIDCode.CheckImgPattern, name));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_CheckLoadVideo(string path)
    {
        Packet packet = (Packet)(new String_Packet(PacketIDCode.CheckLoadVideo, path));
        lock (lockObject)
        {
            dataQueue.Enqueue(packet);
        }
    }
    private void AddPacket_loadProgress(string str)
    {
        ui_Messenger.SetLoadingText(loadingTitleText, str);
    }
}

public enum PacketIDCode
{
    Error,
    SetVideoPath, SaveStorage, CheckSaveStorage,
    SetImgNameCase, CheckImgPattern, CheckLoadVideo
}

public class Packet
{
    public Packet(PacketIDCode _id)
    {
        this.id = _id;
    }
    public PacketIDCode id;
}
public class String_Packet : Packet
{
    public String_Packet(PacketIDCode _id, string _str) : base(_id)
    {
        this.str = _str;
    }
    public string str;
}
public class ListString_Packet : Packet
{
    public ListString_Packet(PacketIDCode _id, List<string> _str_L) : base(_id)
    {
        this.str_L = _str_L;
    }
    public List<string> str_L;
}