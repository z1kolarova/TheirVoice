using UnityEngine;

namespace Assets.Scripts
{
    public class UserInterfaceUtilities : MonoBehaviour
    {
        private static UserInterfaceUtilities instance;
        public static UserInterfaceUtilities I => instance;

        [SerializeField] GameObject crossHair;
        [SerializeField] ConversationManager conversationManager;
        [SerializeField] PauseMenu pauseMenu;

        public void Start()
        {
            instance = this;
        }

        public bool IsCursorLocked()
        {
            return Cursor.lockState == CursorLockMode.Locked;
        }

        public void SetCursorUnlockState(bool activatingUI)
        {
            bool shouldUIBeActive = activatingUI || pauseMenu.IsActive || conversationManager.IsInDialogue;
            crossHair.gameObject.SetActive(!shouldUIBeActive);
            Cursor.lockState = shouldUIBeActive ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}