using Assets.ThirdParty.CalvinRien;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioInputManager : MonoBehaviour
{
    public static AudioInputManager I => instance;
    static AudioInputManager instance;

    [SerializeField] Button recordButton;
    [SerializeField] Image recordButtonBg;
    [SerializeField] TMP_InputField outputTMP;
    [SerializeField] int maxRecordingDuration = 60;

    private readonly string fileName = "output.wav";
    private readonly int recordingFrequency = 44100;

    private const string startRecordingBtnText = "Start using microphone";
    private const string stopRecordingBtnText = "Stop recording";

    private string selectedMicrophone;
    public string SelectedMicrophone { get { return selectedMicrophone; } }

    private AudioClip clip;
    private AudioClip trimmedClip;
    private int startedClipCounter;
    private bool isRecording;

    private void Start()
    {
        instance = this;
        recordButton.onClick.AddListener(RecordBtnAction);
        startedClipCounter = 0;
        isRecording = false;
    }

    private IEnumerator StartRecording()
    {
        ToggleRecordingState();
        clip = Microphone.Start(selectedMicrophone, false, maxRecordingDuration, recordingFrequency);
        startedClipCounter++;
        var rememberCounter = startedClipCounter;
        yield return new WaitForSeconds(maxRecordingDuration - 1);

        if (isRecording && startedClipCounter == rememberCounter) 
        { 
            recordButton.enabled = false;
            yield return StartCoroutine(StopRecording());
            recordButton.enabled = true;
        }
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
            StartCoroutine(StartRecording());
        }
    }

    private IEnumerator StopRecording()
    {
        EnsureRecordingStops();

        byte[] data = SaveWav.Save(fileName, trimmedClip);

        yield return GetAndDisplayTranscription(data);
    }

    private IEnumerator GetAndDisplayTranscription(byte[] data, string language = "en")
    {
        outputTMP.text = "...audio processing in progress...";
        AudioUtilsWhisper.GetTranscriptionThroughServer(data, language);
        yield return new WaitWhile(AudioUtilsWhisper.IsWaitingForResponse);
    }

    public void ToggleRecordingState()
    {
        isRecording = !isRecording;
        recordButtonBg.color = isRecording ? Color.red : Color.white;
        recordButton.GetComponentInChildren<TextMeshProUGUI>().text = isRecording ? stopRecordingBtnText : startRecordingBtnText;
    }

    public void EnsureRecordingStops()
    {
        if (isRecording)
        {
            ToggleRecordingState();
            var lastSample = Microphone.GetPosition(selectedMicrophone);
            Microphone.End(selectedMicrophone);

            float[] samples = new float[lastSample];
            clip.GetData(samples, 0);

            trimmedClip = AudioClip.Create(fileName, lastSample, clip.channels, clip.frequency, false);
            trimmedClip.SetData(samples, 0);
        }
    }

    public bool HasMicrophoneSelected => !string.IsNullOrWhiteSpace(selectedMicrophone);

    public void SetSelectedMicrophone(string microphone)
    {
        selectedMicrophone = microphone;
    }
}
