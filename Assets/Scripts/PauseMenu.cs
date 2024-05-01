using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    [SerializeField] private Button backToGameBtn;
    [SerializeField] private Button backToMainMenuBtn;
    [SerializeField] private Button exitGameBtn;
    
    void Start()
    {
        backToGameBtn.onClick.AddListener(() => {
            TogglePauseMenu();
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
