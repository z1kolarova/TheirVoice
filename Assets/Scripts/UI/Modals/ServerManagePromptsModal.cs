using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerManagePromptsModal : MonoBehaviour
{
    [SerializeField] Button closeBtn;
    [SerializeField] TMP_InputField codeInputField;
    [SerializeField] Button retryPublicLobbyJoinBtn;
    [SerializeField] Button tryPrivateJoinBtn;

    void Start()
    {
        codeInputField.text = ClientSideManager.I.PrivateLobbyCode;

        closeBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        retryPublicLobbyJoinBtn.onClick.AddListener(() =>
        {
            ClientSideManager.I.JoinPublicLobbyAndRelay();
            gameObject.SetActive(false);
        });

        tryPrivateJoinBtn.onClick.AddListener(() =>
        {
            ClientSideManager.I.JoinPrivateLobbyAndRelay(codeInputField.text.ToUpper());
            gameObject.SetActive(false);
        });
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
