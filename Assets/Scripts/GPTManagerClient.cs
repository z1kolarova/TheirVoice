using System;
using Unity.Netcode;
using UnityEngine;

public class GPTManagerClient : NetworkBehaviour
{
    private NetworkVariable<int> testingValue = new NetworkVariable<int>(-1);

    public override void OnNetworkSpawn()
    {
        testingValue.OnValueChanged += ReactToValueChange;
        TestValueChangeLogic();
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManagerUI.I.WriteLineToOutput("initial testing value is: " + testingValue.Value);
    }

    private void ReactToValueChange(int previousValue, int newValue)
    {
        NetworkManagerUI.I.WriteLineToOutput($"The value changed from {previousValue} to {newValue} on the server.");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestValueChangeLogic();
        }
        //else if (Input.GetKeyDown(KeyCode.R))
        //{
        //    ChangeAndPrintNetworkValue();
        //}
        //else if (Input.GetKeyDown(KeyCode.J))
        //{
        //    JustPrintNetworkValue();
        //}
    }

    public void TestValueChangeLogic()
    {
        if (IsServer)
        {
            //testingValue.Value = UnityEngine.Random.Range(0, 100);
            //NetworkManagerUI.I.WriteLineToOutput("T pressed on server");
            //NetworkManagerUI.I.WriteLineToOutput("random value changed to: " + testingValue.Value);
            //TestServerRpc();

            NetworkManagerUI.I.WriteLineToOutput($"I'll just change the value myself. Now it's: {testingValue.Value}");
            testingValue.Value = UnityEngine.Random.Range(0, 100);
            NetworkManagerUI.I.WriteLineToOutput($"And I changed it to: {testingValue.Value}");
        }
        else if (IsClient)
        {
            //NetworkManagerUI.I.WriteLineToOutput("T pressed on client");
            TestServerRpc();
        }
    }

    [ServerRpc]
    private void TestServerRpc()
    {
        var msg = "sentFromClient!" + OwnerClientId.ToString();
        Debug.Log(msg);
        NetworkManagerUI.I.WriteLineToOutput(msg);
        testingValue.Value = UnityEngine.Random.Range(0, 100);
    }

    //private void ChangeAndPrintNetworkValue()
    //{
    //    NetworkManagerUI.I.WriteLineToOutput("The value currently is: " + testingValue.Value + "and I am " + OwnerClientId);
    //    testingValue.Value = UnityEngine.Random.Range(0, 100);
    //    NetworkManagerUI.I.WriteLineToOutput("random value changed to: " + testingValue.Value + "because of " + OwnerClientId);
    //}
    //private void JustPrintNetworkValue()
    //{
    //    NetworkManagerUI.I.WriteLineToOutput("The value is: " + testingValue.Value + "and I am " + OwnerClientId);
    //}
}
