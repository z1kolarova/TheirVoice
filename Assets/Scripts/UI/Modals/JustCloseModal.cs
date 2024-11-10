using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class JustCloseModal : MonoBehaviour
{
    [SerializeField] protected Button closeModalBtn;

    protected virtual void Awake()
    { }

    protected virtual void Start()
    {
        closeModalBtn.onClick.AddListener(() => {
            gameObject.SetActive(false);
        });
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
        //if (value)
        //{
        //    UserInterfaceUtilities.I?.SetCursorUnlockState(true);
        //}
    }

    public virtual void Display()
    {
        SetActive(true);
    }

    public virtual void Hide()
    {
        SetActive(false);
    }
}
