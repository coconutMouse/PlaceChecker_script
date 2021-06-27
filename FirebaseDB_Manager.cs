using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class FirebaseDB_Manager : MonoBehaviour
{
    public event OnResultPacket_string OnResultPacket_Error;
    public event OnResultPacket_string OnResultPacket_AddDB_Name; 
    public event OnResultPacket_ListString OnResultPacket_LoadDB_AllName;

    public void AddDB_Name(string deviceID)
    {
        string imgName = "";
        FirebaseDatabase.DefaultInstance.RootReference.Child("DeviceID/" + deviceID + "/").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.Log(task.Result);
                OnResultPacket_Error("FirebaseDB_Manager/AddDB_Name");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if(snapshot.Value == null)
                {
                    UserImgName ImgCase = new UserImgName();
                    imgName = deviceID + ImgCase.name.Count.ToString();
                    ImgCase.Add(imgName);
                    string json = JsonUtility.ToJson(ImgCase);

                    DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
                    reference.Child("DeviceID").Child(deviceID).SetRawJsonValueAsync(json);
                }
                else
                {
                    UserImgName ImgCase = JsonUtility.FromJson<UserImgName>(snapshot.GetRawJsonValue());
                    imgName = deviceID + ImgCase.name.Count.ToString();
                    ImgCase.Add(imgName);
                    string json = JsonUtility.ToJson(ImgCase);

                    DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
                    reference.Child("DeviceID").Child(deviceID).SetRawJsonValueAsync(json);
                }
                OnResultPacket_AddDB_Name(imgName);
            }
        });
    }

    public void LoadDB_AllName()
    {
        List<string> all_nameCase = new List<string>();
        FirebaseDatabase.DefaultInstance.RootReference.Child("DeviceID/").GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.Log(task.Result);
                OnResultPacket_Error("FirebaseDB_Manager/LoadDB_AllName");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
            
                if (snapshot.Value == null)
                {
                    OnResultPacket_Error("FirebaseDB_Manager/LoadDB_AllName/Value == null");
                }
                else
                {
                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        UserImgName ImgCase = JsonUtility.FromJson<UserImgName>(data.GetRawJsonValue());
                        all_nameCase.AddRange(ImgCase.name);
                    }
                    OnResultPacket_LoadDB_AllName(all_nameCase);
                }
            }
        });
    }

    void ReMoveDataAll()
    {
        FirebaseDatabase.DefaultInstance.RootReference.RemoveValueAsync();
    }
}

public class UserImgName
{
    public List<string> name;

    public UserImgName()
    {
        name = new List<string>();
    }

    public void Add(string _name)
    {
        this.name.Add(_name);
    }
}

