using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    public void LoadScene_PlayEnrollmentData_Scene()
    {
        SceneManager.LoadScene("PlayEnrollment_Scene");
    }
    public void LoadScene_MenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
    public void LoadScene_Enrollment_Scene()
    {
        SceneManager.LoadScene("Enrollment_Scene");
    }
    
}
