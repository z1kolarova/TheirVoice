﻿using Assets.Classes;
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
    [SerializeField] TMP_Text endConvoAbilityDesc;

    void Start()
    {
        instance = this;
        revealPersonalityBtn.onClick.AddListener(() => { 
            personalityDesc.gameObject.SetActive(true);
            endConvoAbilityDesc.gameObject.SetActive(true);
        });
        transform.gameObject.SetActive(false);
    }

    public void GetAttributesForDisplay(string name, EndConvoAbility endConvoDesc)
    {
        personalityDesc.text = name;
        endConvoAbilityDesc.text = endConvoDesc switch {
            EndConvoAbility.Never => "can never end convo",
            EndConvoAbility.Sometimes => "can sometimes end convo",
            EndConvoAbility.Always => "can always end convo",
            _ => "unknown - please report seeing this"
        };
    }

    public void SetActive(bool value)
    {
        personalityDesc.gameObject.SetActive(false);
        endConvoAbilityDesc.gameObject.SetActive(false);
        transform.gameObject.SetActive(value);
    }
}