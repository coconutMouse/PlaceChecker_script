using UnityEngine;
using Firebase;
using System.IO;
using System.Threading.Tasks;

public class FirebaseStorage_Manager : MonoBehaviour
{
    public event OnResultPacket_string OnResultPacket_Error;
    public event OnResultPacket OnResultPacket_Save;
    public event OnResultPacket_string OnResultPacket_DownloadImg;
    public event OnResultPacket_string OnResultPacket_DownloadVideo;

    public event OnResultPacket_string OnResultPacket_loadProgress;

    public string fireBaseStorageURL;

    public void UploadTexture2D(string name, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(fireBaseStorageURL + "/Img");
        Firebase.Storage.StorageReference rivers_ref = storage_ref.Child(name + ".png");

        Task<Firebase.Storage.StorageMetadata> task = rivers_ref.PutBytesAsync(bytes, null, new Firebase.Storage.StorageProgress<Firebase.Storage.UploadState>(state =>
        {
            string str = string.Format(" {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount);
            OnResultPacket_loadProgress(str);
        }), System.Threading.CancellationToken.None, null);

        task.ContinueWith(resultTask => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                OnResultPacket_Error("FirebaseStorage_Manager/UploadTexture2D");
            }
            else
            {
                OnResultPacket_Save();
            }
        });
    }

    public void UploadVideo(string name, string path)
    {
        Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(fireBaseStorageURL + "/Video");
        Firebase.Storage.StorageReference rivers_ref = storage_ref.Child(name + ".mp4");

        Task<Firebase.Storage.StorageMetadata> task = rivers_ref.PutFileAsync(path, null, new Firebase.Storage.StorageProgress<Firebase.Storage.UploadState>(state => 
        {
            string str = string.Format(" {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount);
            OnResultPacket_loadProgress(str);
        }), System.Threading.CancellationToken.None, null);

        task.ContinueWith(resultTask => {
            if (resultTask.IsFaulted || resultTask.IsCanceled)
            {
                Debug.Log(resultTask.Exception.ToString());
                OnResultPacket_Error("FirebaseStorage_Manager/UploadVideo");
            }
            else
            {
                OnResultPacket_Save();
            }
        });
    }
    
    public void DownloadTexture2D(string name, string deviceDataPath)
    {
        string imgURL = string.Format(name + ".png");
        Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(fireBaseStorageURL + "/Img");
        Firebase.Storage.StorageReference island_ref = storage_ref.Child(imgURL);
        string pngPath = Path.Combine(deviceDataPath, imgURL);

        Task task = island_ref.GetFileAsync(pngPath, new Firebase.Storage.StorageProgress<Firebase.Storage.DownloadState>((Firebase.Storage.DownloadState state) => {
            OnResultPacket_loadProgress(string.Format(" {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount));
            }), System.Threading.CancellationToken.None);

        task.ContinueWith(resultTask => {
            if (resultTask.IsFaulted || resultTask.IsCanceled)
            {
                Debug.Log(resultTask.Exception.ToString());
                OnResultPacket_Error("FirebaseStorage_Manager/DownloadTexture2D");
            }
            else
            {
                if (File.Exists(pngPath))
                {
                    OnResultPacket_DownloadImg(name);
                }
                else
                {
                    OnResultPacket_Error("FirebaseStorage_Manager/DownloadImg/LoadPNG == null");
                }
            }
        });
      
    }

    public void DownloadVideo(string name, string deviceDataPath)
    {
        string videoURL = string.Format(name + ".mp4");

        Firebase.Storage.FirebaseStorage storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(fireBaseStorageURL + "/Video");
        Firebase.Storage.StorageReference island_ref = storage_ref.Child(videoURL);
        string videoPath = Path.Combine(deviceDataPath, videoURL);

        Task task = island_ref.GetFileAsync(videoPath, new Firebase.Storage.StorageProgress<Firebase.Storage.DownloadState>((Firebase.Storage.DownloadState state) => {
            OnResultPacket_loadProgress(string.Format(" {0} of {1} bytes transferred.", state.BytesTransferred, state.TotalByteCount));
        }), System.Threading.CancellationToken.None);

        task.ContinueWith(resultTask => {
            if (resultTask.IsFaulted || resultTask.IsCanceled)
            {
                Debug.Log(resultTask.Exception.ToString());
                OnResultPacket_Error("FirebaseStorage_Manager/DownloadVideo");
            }
            else
            {
                Debug.Log("Finished downloading...");
                if (File.Exists(videoPath))
                {
                    OnResultPacket_DownloadVideo(videoPath);
                }
                else
                {
                    OnResultPacket_Error("FirebaseStorage_Manager/DownloadVideo/loadVideo == null");
                }
            }
        });
    }

}
