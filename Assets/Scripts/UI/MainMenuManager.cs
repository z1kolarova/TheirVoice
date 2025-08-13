using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{    
    public static MainMenuManager I => instance;
    static MainMenuManager instance;

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button howItWorksButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;

    [Header("Modals")]
    [SerializeField] LanguageSelectionModal languageSelectionModal;
    [SerializeField] JustCloseModal howItWorksModal;
    [SerializeField] JustCloseModal creditsModal;

    private void Awake()
    {
        instance = this;
    }

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

        SetStartButtonInteractable(ClientSideManager.I.HasAllNeededConnections 
            && ClientDataManager.I.Languages?.Count > 0);
    }

    private void LoadCubeOfTruth()
    {
        SceneManager.LoadScene(sceneName: "Scenes/CubeOfTruth");
    }

    private async void ExitSimulator()
    {
        await ClientSideManager.I.DisconnectAndCloseApp();
    }

    public void SetStartButtonInteractable(bool interactable)
    {
        startButton.interactable = interactable;
    }

    private IEnumerator DisplayLanguageSelectionModal()
    {
        if (ClientDataManager.I.Languages == null)
        {
            if (!ClientDataUtils.AvailableLangsRequester.IsCurrentlyWaiting())
            {
                ClientDataUtils.AvailableLangsRequester.RequestData();
            }
            yield return new WaitWhile(ClientDataUtils.AvailableLangsRequester.IsCurrentlyWaiting);
        }

        languageSelectionModal.SetActive(true);
    }

    public void ProceedToLanguageSelectionDialogue()
    {
        StartCoroutine(DisplayLanguageSelectionModal());
    }
}
