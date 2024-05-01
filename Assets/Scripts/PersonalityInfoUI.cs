using Assets.Classes;
using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersonalityInfoUI : MonoBehaviour
{
    public static PersonalityInfoUI I => instance;
    static PersonalityInfoUI instance;

    [SerializeField] Button revealPersonalityBtn;
    [SerializeField] TMP_Text personalityDesc;

    void Start()
    {
        instance = this;
        revealPersonalityBtn.onClick.AddListener(() => { 
            personalityDesc.gameObject.SetActive(true);
        });
        transform.gameObject.SetActive(false);
    }

    public void DisplayAsPersonalityDesc(string desc)
    {
        personalityDesc.text = desc;
    }

    public void SetActive(bool value)
    {
        personalityDesc.gameObject.SetActive(false);
        transform.gameObject.SetActive(value);
    }
}
