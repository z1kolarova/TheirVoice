using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button howItWorksButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;

    [Header("Panels")]
    [SerializeField] InfoPanel howItWorksPanel;
    [SerializeField] InfoPanel creditsPanel;

    void Start()
    {
        startButton.onClick.AddListener(() => {
            startButton.enabled = false;
            LoadCubeOfTruth();
        });

        howItWorksButton.onClick.AddListener(() => {
            howItWorksPanel.SetActive(true);
        });

        creditsButton.onClick.AddListener(() => {
            creditsPanel.SetActive(true);
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
