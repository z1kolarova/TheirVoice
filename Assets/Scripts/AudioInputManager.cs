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
    [SerializeField] TMP_Dropdown microphoneSelectionDropdown;
    [SerializeField] int maxRecordingDuration = 5;

    private readonly string fileName = "output.wav";
    private readonly int recordingFrequency = 44100;

    private string selectedMicrophone;

    private AudioClip clip;
    private bool isRecording;
    private float time;
    private OpenAIApi openAIApi = new OpenAIApi(Environment.GetEnvironmentVariable("OPENAI_API_KEY_THEIR_VOICE", EnvironmentVariableTarget.User));

    private void Start()
    {
        instance = this;
        //foreach (var device in Microphone.devices)
        //{
        //    microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(device));
        //}
        recordButton.onClick.AddListener(RecordBtnAction);
    }

    private void StartRecording()
    {
        isRecording = true;
        //recordButton.enabled = false;

        //recordButton.onClick.RemoveListener(StartRecording);
        //clip = Microphone.Start(selectedMicrophone, false, maxRecordingDuration, recordingFrequency);

        //clip = Microphone.Start(microphoneSelectionDropdown.options[microphoneSelectionDropdown.value].text, false, maxRecordingDuration, recordingFrequency);


        Debug.Log("before clip");
        Debug.Log(selectedMicrophone);
        //microphoneSelectionDropdown.options.Count - 1
        clip = Microphone.Start(selectedMicrophone, false, maxRecordingDuration, recordingFrequency);

        Debug.Log("after clip");
    }

    private IEnumerator StopRecording()
    {
        Debug.Log("in stop recording");
        isRecording = false;
        Microphone.End(selectedMicrophone);

        byte[] data = SaveWav.Save(fileName, clip);

        yield return GetAndDisplayTranscription(data);

        //recordButton.enabled = true;

        //var request = new CreateAudioTranscriptionsRequest { 
        //    FileData = new FileData() { Data = data, Name = fileName },
        //    Model = "whisper-1",
        //    Language = "en",
        //};

        //var res = await openAIApi.CreateAudioTranscription(request);
        //outputTMP.text = res.Text;
    }

    private IEnumerator GetAndDisplayTranscription(byte[] data, string language = "en")
    {
        outputTMP.text = "audio processing in progress";
        AudioUtilsWhisper.GetTranscriptionThroughServer(data, language);
        yield return new WaitWhile(AudioUtilsWhisper.IsWaitingForResponse);
    }

    //private void Update()
    //{
    //    if (isRecording)
    //    {
    //        time += Time.deltaTime;
    //        if (time >= maxRecordingDuration)
    //        {
    //            Debug.Log("will stop recording");
    //            isRecording = false;
    //            StopRecording();
    //        }
    //    }
    //}

    private void RecordBtnAction()
    {
        Debug.Log("recordBtn pressed");
        Debug.Log(isRecording);
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

    public void SetRecordingButton(Button recordingButton)
    {
        recordButton = recordingButton;
    }

    public bool MicrophoneSelected => !string.IsNullOrEmpty(selectedMicrophone);

    public void SetSelectedMicrophone(string microphone)
    {
        selectedMicrophone = microphone;
    }
}
