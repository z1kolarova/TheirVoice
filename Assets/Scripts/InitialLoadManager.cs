using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLoadManager : MonoBehaviour
{
    [SerializeField] private bool buildServer = false;

    void Start()
    {
        if (buildServer)
        {
            SceneManager.LoadScene(sceneName: "Scenes/Server");
        }
        else
        {
            SceneManager.LoadScene(sceneName: "Scenes/MainMenu");
        }
    }
}
