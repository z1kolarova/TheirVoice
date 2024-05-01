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
    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button howItWorksButton;
    [SerializeField] Button exitButton;

    [Header("Panels")]
    [SerializeField] HowItWorksPanel howItWorksPanel;

    void Start()
    {
        startButton.onClick.AddListener(() => {
            LoadCubeOfTruth();
        });

        howItWorksButton.onClick.AddListener(() => {
            howItWorksPanel.SetActive(true);
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
