
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;

public class THH_ChatManager : UdonSharpBehaviour
{
    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public string CHAT_MESSAGE = "";
    private string last_ChatMessage = "";

    [HideInInspector]
    public string MESSAGE = "SAMPLETEXT";
    
    private bool wasOwner;
    private bool reclaiming;
    private float chatDelay = 1f;
    private float sendEnd;
    private bool messageSending;
    private bool locked;
    private int messageCounter;

    public THH_CanvasLogManager canvasLogManager;
    public InputField inputField;

    public void InputFieldEndEdit()
    {
        string inputString = inputField.text.Trim();
        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }
        SendChatMessage(inputString);
    }

    // == API ==================================================================================================//
    public void SendChatMessage(string message)
    {
        MESSAGE = inputField.text;
        if (ProcessMessage())
        {
            OnMessageSentSuccess();
        }
        else
        {
            OnMessageSentFail();
        }
    }

    void OnMessageSentSuccess()
    {
        inputField.text = string.Empty;
    }

    void OnMessageSentFail()
    {

    }

    void OnMessageReceived(string message)
    {
        Debug.Log($"<color=green>[THH_ChatManager]</color> Chat message received: '{message}'");
        canvasLogManager.Log(message);
    }

    //==================================================================================================//

    public void Start()
    {
        if (Networking.IsMaster)
        {
            wasOwner = true;
        }
    }

    public void Update()
    {
        bool isOwner = Networking.IsOwner(gameObject);
        if (isOwner ^ wasOwner) 
        { 
            wasOwner = !wasOwner; 
            if (isOwner)
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> You became owner of the ChatManager.");
                if (reclaiming)
                { 
                    reclaiming = false;
                    return;
                }
                sendEnd = Time.timeSinceLevelLoad + chatDelay;
                messageSending = true;
            }
            else
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> You lost ownership of the ChatManager.");
                locked = false;
            }
        }

        if (messageSending)
        {
            if (Time.timeSinceLevelLoad > sendEnd)
            {
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageCounter.ToString("X")}";
                messageCounter++;
                messageSending = false;
            }
        }

        if (CHAT_MESSAGE != last_ChatMessage && !string.IsNullOrEmpty(CHAT_MESSAGE))
        {
            string[] M = CHAT_MESSAGE.Split('|');
            string messageSender = VRCPlayerApi.GetPlayerById(Int32.Parse(M[0])).displayName;
            string messageContent = M[1];
            string message = $"<<color=lime>{messageSender}</color>>: {messageContent}";
            OnMessageReceived(message);

            // Reclaim ownership as master after message has been received
            if (Networking.IsMaster && !Networking.GetOwner(gameObject).isLocal)
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> Reclaiming ownership as master");
                reclaiming = true;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            last_ChatMessage = CHAT_MESSAGE;
        }
    }

    bool ProcessMessage()
    {
        if (locked)
        {
            canvasLogManager.Log($"<color=red>You are sending messages too quickly</color>");
            return false;
        }
        if (Networking.GetOwner(gameObject).isMaster)
        {
            if (Networking.IsMaster)
            {
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageCounter.ToString("X")}";
                messageCounter++;
                return true;
            }
            else
            {
                locked = true;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                return true;
            }
        }
        else
        {
            Debug.Log($"<color=green>[THH_ChatManager]</color> ChatManager is currently occupied by someone else, retry later");
            canvasLogManager.Log($"<color=red>Your message could not be delivered, retry in a bit</color>");
            return false;
        }
    }
}