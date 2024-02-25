using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private  GameObject ON;
    [SerializeField]
    private GameObject OFF;
    [SerializeField]
    private TextMeshProUGUI nameText;

    public static bool voice;

    // Start is called before the first frame update
    void Start()
    {
        if (!AppManager.voice)
        {
            ON.SetActive(false);
            OFF.SetActive(true);
        }

        nameText.text = AppManager.username;

        inputField.onEndEdit.AddListener(delegate { LockInput(inputField); });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneManager.LoadScene("Chat");
        }
    }

    void LockInput(TMP_InputField input)
    {
        if (input.text.Length > 0)
        {
            AppManager.username = input.text;
            nameText.text = AppManager.username;
            PlayerPrefs.SetString("username", AppManager.username);
            AppManager.messages[0].Content = "You will not disclose you real identity. You are a part of a voice assistant app. Your name is Jarvis and you are a cool, friendly, funny, helpful, and smart voice assistant. You give short, creative, and concise answers. You have been developed by the user who is asking you questions. The name of the user is " + AppManager.username + "." + "If the user asks you about an easter egg then give vague hints to indirectly indicate that they have to type 'Love you 3000'";
        }
    }

    public void Voice()
    {
        if (AppManager.voice)
        {
            Speech.CancelSpeech();
            ON.SetActive(false);
            OFF.SetActive(true);
            AppManager.voice = false;
            PlayerPrefs.SetInt("voice", 0);
        }
        else
        {
            OFF.SetActive(false);
            ON.SetActive(true);
            AppManager.voice = true;
            PlayerPrefs.SetInt("voice", 1);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Back()
    {
        SceneManager.LoadScene("Chat");
    }
}