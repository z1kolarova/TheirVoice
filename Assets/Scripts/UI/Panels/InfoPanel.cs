using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] Button closePanelBtn;

    void Start()
    {
        closePanelBtn.onClick.AddListener(() => {
            gameObject.SetActive(false);
        });
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
