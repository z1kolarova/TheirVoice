using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour 
{
    [Header("Buttons")]
    [SerializeField] private Button backToGameBtn;
    [SerializeField] private Button howToBtn;
    [SerializeField] private Button audioInputSettingsBtn;
    [SerializeField] private Button backToMainMenuBtn;
    [SerializeField] private Button exitGameBtn;

    [Header("Panels")]
    [SerializeField] private InfoPanel howToPanel;
    [SerializeField] private AudioInputSettingsPanel audioInputSettingsPanel;

    void Start()
    {
        backToGameBtn.onClick.AddListener(() => {
            TogglePauseMenu();
        });

        audioInputSettingsBtn.onClick.AddListener(() => {
            audioInputSettingsPanel.SetActive(true);
        });

        howToBtn.onClick.AddListener(() => {
            howToPanel.SetActive(true);
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
        audioInputSettingsPanel.SetActive(false);
        howToPanel.SetActive(false);
	    gameObject.SetActive(!gameObject.activeSelf);
	    UserInterfaceUtilities.I.SetCursorUnlockState(gameObject.activeSelf);
    }
    public async void DisconnectEverythingAndReturnToMainMenu()
    {
        await TestLobby.I.DisconectFromEverything();
        SceneManager.LoadScene(sceneName: "Scenes/MainMenu");
    }

    public async void DisconnectAndCloseApp() {
        await TestLobby.I.DisconectFromEverything();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
