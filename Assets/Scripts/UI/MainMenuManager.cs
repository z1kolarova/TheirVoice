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

    [Header("Modals")]
    [SerializeField] LobbyNotFoundModal lobbyNotFoundModal; //is moved to ClientSideManager, can be removed from here
    [SerializeField] JustCloseModal howItWorksModal;
    [SerializeField] JustCloseModal creditsModal;

    void Start()
    {
        startButton.onClick.AddListener(() => {
            startButton.enabled = false;
            LoadCubeOfTruth();
        });

        howItWorksButton.onClick.AddListener(() => {
            HowItWorksModal.I.Display();
        });

        creditsButton.onClick.AddListener(() => {
            creditsModal.SetActive(true);
        });

        exitButton.onClick.AddListener(() => {
            ExitSimulator();
        });
    }

    private void LoadCubeOfTruth()
    {
        SceneManager.LoadScene(sceneName: "Scenes/CubeOfTruth");
    }

    private async void ExitSimulator()
    {
        await ClientSideManager.I.DisconnectAndCloseApp();
    }
}
