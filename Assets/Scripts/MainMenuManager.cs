using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitButton;

    void Start()
    {
        startButton.onClick.AddListener(() => {
            LoadCubeOfTruth();
        });
        settingsButton.onClick.AddListener(() => {
            // todo open settings
        });
        exitButton.onClick.AddListener(() => {
            ExitSimulator();
        });
    }

    private void LoadCubeOfTruth()
    {
        SceneManager.LoadScene(sceneName: "Scenes/CubeOfTruth");
    }

    private void ExitSimulator() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
    }
}
