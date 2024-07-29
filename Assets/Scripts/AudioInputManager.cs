using Assets.ThirdParty.CalvinRien;
using OpenAI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioInputManager : MonoBehaviour
{
    public static AudioInputManager I => instance;
    static AudioInputManager instance;

    [SerializeField] Button recordButton;
    [SerializeField] TMP_InputField outputTMP;
    [SerializeField] int maxRecordingDuration = 5;

    private readonly string fileName = "output.wav";
    private readonly int recordingFrequency = 44100;

    private const string startRecordingBtnText = "Start using microphone";
    private const string stopRecordingBtnText = "Stop recording";

    private string selectedMicrophone;
    public string SelectedMicrophone { get { return selectedMicrophone; } }

    private AudioClip clip;
    private bool isRecording;
    private float time;
    private OpenAIApi openAIApi = new OpenAIApi(Environment.GetEnvironmentVariable("OPENAI_API_KEY_THEIR_VOICE", EnvironmentVariableTarget.User));

    private void Start()
    {
        instance = this;
        recordButton.onClick.AddListener(RecordBtnAction);
        isRecording = false;
    }

    private void StartRecording()
    {
        ToggleRecordingState();
        clip = Microphone.Start(selectedMicrophone, false, maxRecordingDuration, recordingFrequency);
    }

    private IEnumerator StopRecording()
    {
        Debug.Log("in stop recording");
        EnsureRecordingStops();

        byte[] data = SaveWav.Save(fileName, clip);

        yield return GetAndDisplayTranscription(data);
    }

    private IEnumerator GetAndDisplayTranscription(byte[] data, string language = "en")
    {
        outputTMP.text = "audio processing in progress";
        AudioUtilsWhisper.GetTranscriptionThroughServer(data, language);
        yield return new WaitWhile(AudioUtilsWhisper.IsWaitingForResponse);
    }

    private void RecordBtnAction()
    {
        Debug.Log("recordBtn pressed");
        if (isRecording)
        {
            Debug.Log("before StopRecording");
            StartCoroutine(StopRecording());
        }
        else
        {
            Debug.Log("before StartRecording");
            StartRecording();
        }
    }

    public void ToggleRecordingState()
    {
        isRecording = !isRecording;
        recordButton.GetComponentInChildren<TextMeshProUGUI>().text = isRecording ? stopRecordingBtnText : startRecordingBtnText;
    }

    public void EnsureRecordingStops()
    {
        if (isRecording)
        {
            ToggleRecordingState();
            Microphone.End(selectedMicrophone);
        }
    }

    public bool HasMicrophoneSelected => !string.IsNullOrWhiteSpace(selectedMicrophone);

    public void SetSelectedMicrophone(string microphone)
    {
        selectedMicrophone = microphone;
    }
}
