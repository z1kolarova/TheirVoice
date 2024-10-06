using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{    
    public static PauseMenu I => instance;
    static PauseMenu instance;

    [Header("Buttons")]
    [SerializeField] private Button backToGameBtn;
    [SerializeField] private Button howToBtn;
    [SerializeField] private Button audioInputSettingsBtn;
    [SerializeField] private Button backToMainMenuBtn;
    [SerializeField] private Button exitGameBtn;

    [Header("Modals")]
    [SerializeField] private InfoModal howToModal;
    [SerializeField] private AudioInputSettingsModal audioInputSettingsModal;

    public bool IsActive => gameObject.activeSelf;

    void Start()
    {
        instance = this;

        backToGameBtn.onClick.AddListener(() => {
            TogglePauseMenu();
        });

        audioInputSettingsBtn.onClick.AddListener(() => {
            audioInputSettingsModal.SetActive(true);
        });

        howToBtn.onClick.AddListener(() => {
            howToModal.SetActive(true);
        });

        backToMainMenuBtn.onClick.AddListener(() =>
        {
            DisconnectEverythingAndReturnToMainMenu();
        });

        exitGameBtn.onClick.AddListener(() => {
            DisconnectAndCloseApp();
        });
    }

    public void TogglePauseMenu()
    {
        audioInputSettingsModal.SetActive(false);
        howToModal.SetActive(false);
	    gameObject.SetActive(!gameObject.activeSelf);
	    UserInterfaceUtilities.I.SetCursorUnlockState(gameObject.activeSelf);
    }
    public void DisconnectEverythingAndReturnToMainMenu()
    {
        //await ClientSideManager.I.DisconectFromEverything();
        //TODO: disposing of spawned entities?
        SceneManager.LoadScene(sceneName: "Scenes/MainMenu");
    }

    public async void DisconnectAndCloseApp() 
    {
        await ClientSideManager.I.DisconnectAndCloseApp();
    }
}
