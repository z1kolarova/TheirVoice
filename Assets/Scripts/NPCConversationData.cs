using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCConversationData : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Animation Controllers")]
    [SerializeField] private Animator npcSpeechBubbleAnimator;

    private float speechBubbleAnimationDelay = 0.6f;

    public Animator GetAnimator() => npcSpeechBubbleAnimator;
}
