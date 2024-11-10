using TMPro;
using UnityEngine;

public class InfoModal : JustCloseModal
{
    public static InfoModal I => instance;
    static InfoModal instance;

    [SerializeField] TMP_Text titleLabel;
    [SerializeField] TMP_Text infoLabel;

    protected override void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void Display(string title, string info)
    {
        titleLabel.text = title;
        infoLabel.text = info;
        SetActive(true);
    }
}
