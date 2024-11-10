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
    [SerializeField] InfoModal howItWorksModal;
    [SerializeField] InfoModal creditsModal;

    void Start()
    {
        startButton.onClick.AddListener(() => {
            startButton.enabled = false;
            LoadCubeOfTruth();
        });

        howItWorksButton.onClick.AddListener(() => {
            howItWorksModal.SetActive(true);
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
