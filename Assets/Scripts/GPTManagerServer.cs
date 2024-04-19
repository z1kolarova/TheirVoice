using Unity.Netcode;
using UnityEngine;

public class GPTManagerServer : NetworkBehaviour
{
    //private static Model _model = Model.ChatGPTTurbo;   //originally was ChatGPTTurbo
    //private static double _temperature = 0.5;           //originally was 0.1
    //private static int _maxTokens = 50;                 //originally was 50
    //private static double _frequencyPenalty = 0.4;      //originally was 0
    //private static double _presencePenalty = 0.4;       //originally was 0

    //private static OpenAIAPI api;
    //public static OpenAIAPI API
    //{
    //    get
    //    {
    //        if (api == null)
    //        {
    //            api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY_THEIR_VOICE", EnvironmentVariableTarget.User));
    //        }
    //        return api;
    //    }
    //}

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1);

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        NetworkManagerUI.I.WriteLineToOutput("initial random value is: " + randomNumber.Value);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (IsServer)
            {
                randomNumber.Value = UnityEngine.Random.Range(0, 100);
                NetworkManagerUI.I.WriteLineToOutput("T pressed on server");
                NetworkManagerUI.I.WriteLineToOutput("random value changed to: " + randomNumber.Value);
                TestServerRpc();
            }
            else if (IsClient)
            {
                NetworkManagerUI.I.WriteLineToOutput("T pressed on client");
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeAndPrintNetworkValue();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            JustPrintNetworkValue();
        }
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    Debug.Log("t pressed");
        //    TestServerRpc();
        //    Debug.Log("rpc should have happened");
        //}
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        //outputTMP.text += "I'm in\n";
        var msg = OwnerClientId.ToString();
        Debug.Log(msg);
        NetworkManagerUI.I.WriteLineToOutput(msg);
    }

    private void ChangeAndPrintNetworkValue()
    {
        NetworkManagerUI.I.WriteLineToOutput("The value currently is: " + randomNumber.Value + "and I am " + OwnerClientId);
        randomNumber.Value = UnityEngine.Random.Range(0, 100);
        NetworkManagerUI.I.WriteLineToOutput("random value changed to: " + randomNumber.Value + "because of " + OwnerClientId);
    }
    private void JustPrintNetworkValue()
    {
        NetworkManagerUI.I.WriteLineToOutput("The value is: " + randomNumber.Value + "and I am " + OwnerClientId);
    }

    //public async Task<ChatResult> ObtainResponse(IList<ChatMessage> messages)
    //{
    //    var chatResult = await API.Chat.CreateChatCompletionAsync(new ChatRequest()
    //    {
    //        Model = _model,
    //        Temperature = _temperature,
    //        MaxTokens = _maxTokens,
    //        Messages = messages,
    //        FrequencyPenalty = _frequencyPenalty,
    //        PresencePenalty = _presencePenalty
    //    });
    //    return chatResult;
    //}
}
