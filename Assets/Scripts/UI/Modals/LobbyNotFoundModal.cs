using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNotFoundModal : JustCloseModal
{
    [SerializeField] TMP_InputField codeInputField;
    [SerializeField] Button retryPublicLobbyJoinBtn;
    [SerializeField] Button tryPrivateJoinBtn;

    protected override void Start()
    {
        codeInputField.text = ClientSideManager.I.PrivateLobbyCode;

        retryPublicLobbyJoinBtn.onClick.AddListener(() =>
        {
            _ = ClientSideManager.I.JoinPublicLobbyAndRelay();
            gameObject.SetActive(false);
        });

        tryPrivateJoinBtn.onClick.AddListener(() =>
        {
            _ = ClientSideManager.I.JoinPrivateLobbyAndRelay(codeInputField.text.ToUpper());
            gameObject.SetActive(false);
        });

        base.Start();
    }
}
