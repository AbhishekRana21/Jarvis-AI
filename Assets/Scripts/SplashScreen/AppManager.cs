using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    // Info
    public static bool voice;
    public static bool greeted;
    public static string username;

    // Keys
    public static string jarvisAIspeechAPI;
    public static string jarvisAIopenAIAPI;

    // Messages
    public static bool newChat;
    public static RequestMessage[] messages;

    // Start is called before the first frame update
    void Start()
    {
        username = "Username Here";
        jarvisAIopenAIAPI = "OpenAI API Key Here";
        jarvisAIspeechAPI = "Speech API Key Here";

        if (PlayerPrefs.HasKey("voice"))
        {
            if (PlayerPrefs.GetInt("voice") == 1)
            {
                voice = true;
            }
            else
            {
                voice = false;
            }
        }
        else
        {
            voice = true;
            PlayerPrefs.SetInt("voice", 1);
        }

        if (PlayerPrefs.HasKey("username"))
        {
            username = PlayerPrefs.GetString("username");
        }
        else
        {
            PlayerPrefs.SetString("username", username);
        }

        greeted = false;
        newChat = true;

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Invoke("LoadChatScene", 5);
        }
    }

    private void LoadChatScene()
    {
        SceneManager.LoadScene("Chat");
    }
}