using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    [SerializeField] private Button exitGameBtn;
    [SerializeField] private Button backToGameBtn;
    
    void Start()
    {
        exitGameBtn.onClick.AddListener(() => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
        });
        
        backToGameBtn.onClick.AddListener(() => {
	        TogglePauseMenu();
        });
    }

    public void TogglePauseMenu()
    {
	    gameObject.SetActive(!gameObject.activeSelf);
	    UserInterfaceUtilities.I.SetCursorUnlockState(gameObject.activeSelf);
    }
}
