using UnityEngine;

namespace DefaultNamespace
{
	public class UserInterfaceUtilities : MonoBehaviour {
		private static UserInterfaceUtilities instance;
		public static UserInterfaceUtilities I => instance;

		[SerializeField] GameObject crossHair;

		public void Start()
		{
			instance = this;
		}

		public bool IsCursorLocked()
		{
			return Cursor.lockState == CursorLockMode.Locked;
		}
		
		public void SetCursorUnlockState(bool uiActive) {
			bool shouldUIBeActive = uiActive || ConversationUI.I.InDialog;
			crossHair.gameObject.SetActive(!shouldUIBeActive);
			Cursor.lockState = shouldUIBeActive ? CursorLockMode.None : CursorLockMode.Locked;
		}
	}
}