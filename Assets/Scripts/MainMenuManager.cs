using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    
    void Start()
    {
        startButton.onClick.AddListener(() => {
            SceneManager.LoadScene(sceneName:"Scenes/CubeOfTruth");
        });
        settingsButton.onClick.AddListener(() => {
            // todo open settings
        });
    }
}
