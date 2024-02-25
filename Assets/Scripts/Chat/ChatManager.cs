using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject Message;
    public GameObject Content;
    private GameObject _textHelper;
    private int _countHelper;
    [SerializeField] GameObject OutputText;
    [SerializeField] GameObject Buttons;
    [SerializeField] GameObject Options;

    public delegate void responseReceieved();
    public static event responseReceieved info;
    public static bool disableButtons;
    private GameObject M;

    //ClearText
    [SerializeField]
    private TextMeshProUGUI clearText;

    //Cancel
    [SerializeField]
    private GameObject clearButton;
    [SerializeField]
    private GameObject stopButton;

    //LY3T
    [SerializeField]
    private GameObject ly3t;
    private string check;

    //Greet
    [SerializeField]
    private GameObject greeting;


    //Event Handling
    public static void ExecuteEvent()
    {
        if (info != null)
            info();
    }

    public void sendMessage()
    {
        Speech.CancelSpeech();
        OutputText.SetActive(false);
        getMessage("You:\n" + inputField.text, false);
        GPTClient.SendChatRequest(inputField.text);
        check = inputField.text.ToLower();
        inputField.text = "";
        getMessage("Loading response", true);
        StartCoroutine(TextAnimator());

        clearButton.SetActive(false);
        stopButton.SetActive(true);

        if (check == "i love you 3000" || check == "i love you 3000." || check == "love you 3000" || check == "love you 3000.")
        {
            EasterEgg();
        }
    }

    public void sendVoiceMessage(string text)
    {
        getMessage("You:\n" + text, false);
        getMessage("Loading response", true);
        StartCoroutine(TextAnimator());

        clearButton.SetActive(false);
        stopButton.SetActive(true);
    }

    public void Start()
    {
        inputField.onEndEdit.AddListener(delegate { LockInput(inputField); });
        _countHelper = 0;
        disableButtons = false;

        if (!AppManager.newChat)
        {
            foreach (RequestMessage m in AppManager.messages)
            {
                if (m.Role == "user")
                {
                    M = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
                    M.transform.localPosition = new Vector3(M.transform.localPosition.x, M.transform.localPosition.y, 0);
                    M.GetComponent<Message>().myMessage.text = "You:\n" + m.Content;
                    M.GetComponent<Image>().color = new Color32(0, 70, 70, 255);
                }
                else if (m.Role == "assistant")
                {
                    M = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
                    M.transform.localPosition = new Vector3(M.transform.localPosition.x, M.transform.localPosition.y, 0);
                    M.GetComponent<Message>().myMessage.text = "Jarvis:\n" + m.Content;
                }
            }
        }

        if (!AppManager.greeted)
        {
            greeting.SetActive(true);
            AppManager.greeted = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void activate()
    {
        if (!disableButtons)
        {
            greeting.SetActive(false);
            inputField.ActivateInputField();
        }
    }


    void LockInput(TMP_InputField input)
    {
        if (input.text.Length > 0)
        {
            sendMessage();
        }
    }


    public void getMessage(string recievedMessage, bool response)
    {
        if (!response)
        {
            M = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
            M.transform.localPosition = new Vector3(M.transform.localPosition.x, M.transform.localPosition.y, 0);
            M.GetComponent<Message>().myMessage.text = recievedMessage;
            M.GetComponent<Image>().color = new Color32(0, 70, 70, 255);
        }
        else
        {
            _textHelper = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
            _textHelper.transform.localPosition = new Vector3(_textHelper.transform.localPosition.x, _textHelper.transform.localPosition.y, 0);
            _textHelper.GetComponent<Message>().myMessage.text = recievedMessage;
        }
    }

    public void setMessage(string recievedMessage)
    {
        _countHelper++;
        Destroy(_textHelper);
        _textHelper = Instantiate(Message, Vector3.zero, Quaternion.identity, Content.transform);
        _textHelper.transform.localPosition = new Vector3(_textHelper.transform.localPosition.x, _textHelper.transform.localPosition.y, 0);
        _textHelper.GetComponent<Message>().myMessage.text = "Jarvis:\n" + recievedMessage;

        RequestMessage newMessage = new RequestMessage()
        {
            Content = recievedMessage,
            Role = "assistant"
        };

        AppManager.messages = AppManager.messages.ToList().Append(newMessage).ToArray();

        ExecuteEvent();
        disableButtons = false;
        clearButton.SetActive(true);
        stopButton.SetActive(false);
    }

    private IEnumerator TextAnimator()
    {
        var check = _countHelper;
        var dots = 0;
        while (_countHelper == check)
        {
            switch (dots)
            {
                case 0:
                    _textHelper.GetComponent<Message>().myMessage.text = "Loading response.";
                    dots++;
                    break;
                case 1:
                    _textHelper.GetComponent<Message>().myMessage.text = "Loading response..";
                    dots++;
                    break;
                case 2:
                    _textHelper.GetComponent<Message>().myMessage.text = "Loading response...";
                    dots++;
                    break;
                case 3:
                    _textHelper.GetComponent<Message>().myMessage.text = "Loading response....";
                    dots++;
                    break;
                case 4:
                    _textHelper.GetComponent<Message>().myMessage.text = "Loading response";
                    dots = 0;
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        StopCoroutine(TextAnimator());
    }


    public static void ShareText(string message)
    {
        var lines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        new NativeShare()
        .SetText(string.Join(Environment.NewLine, lines.Skip(1)))
        .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
        .Share();
    }

    public void OpenSettings()
    {
        if (!disableButtons)
        {
            Speech.CancelSpeech();
            SceneManager.LoadScene("Settings");
        }
    }

    public void Clear()
    {
        if (!disableButtons && AppManager.messages.Length > 1)
        {
            AppManager.newChat = true;
            AppManager.messages = new RequestMessage[0];
            Speech.CancelSpeech();
            SceneManager.LoadScene("Chat");
        }
    }

    private void EasterEgg()
    {
        ly3t.GetComponent<Animator>().SetBool("fly", true);
        Invoke("EasterEggReset", 3);
    }

    public void StopTurn()
    {
        GPTClient.CancelRequest();
        _countHelper++;
        Destroy(_textHelper);
        Destroy(M);
        AppManager.messages = AppManager.messages.Take(AppManager.messages.Count() - 1).ToArray();
        foreach (RequestMessage m in AppManager.messages)
        {
            Debug.Log(m.Role + ": " + m.Content);
        }
        disableButtons = false;
        clearButton.SetActive(true);
        stopButton.SetActive(false);
    }
}