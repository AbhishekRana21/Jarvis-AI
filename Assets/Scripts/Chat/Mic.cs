using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using TMPro;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class Mic : MonoBehaviour
{
    private bool micPermissionGranted = false;
    public TextMeshProUGUI outputText;
    public Button recoButton;
    SpeechRecognizer recognizer;
    SpeechConfig config;
    AudioConfig audioInput;
    PushAudioInputStream pushStream;

    private object threadLocker = new object();
    private bool recognitionStarted = false;
    private string message;
    int lastSample = 0;
    AudioSource audioSource;

    public static ChatManager chat;
    private bool start;

    [SerializeField]
    private GameObject Chats;
    [SerializeField]
    private GameObject OutputText;
    private bool change;

    [SerializeField]
    private GameObject MicButton;
    [SerializeField]
    private GameObject MicPic;
    private bool recOn;
    private bool stopRec;

    private string serviceKey;

    [SerializeField]
    private GameObject greeting;


#if PLATFORM_ANDROID || PLATFORM_IOS
    private Microphone mic;
#endif

    private byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
    {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length)
        {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }
        byte[] bytes = dataStream.ToArray();
        dataStream.Dispose();
        return bytes;
    }

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (threadLocker)
        {
            message = e.Result.Text;
            Debug.Log("RecognizingHandler: " + message);
        }
    }

    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (threadLocker)
        {
            message = e.Result.Text;
            
            Debug.Log("RecognizedHandler: " + message);
            stopRec = true;
        }
    }

    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        lock (threadLocker)
        {
            message = e.ErrorDetails.ToString();
            Debug.Log("CanceledHandler: " + message);
        }
    }

    public void ShiftText()
    {
        if (!start)
        {
            start = true;
        }
        else
        {
            start = false;
        }
    }

    public void Recorder()
    {
        if (!recOn && !ChatManager.disableButtons)
        {
            Speech.CancelSpeech();
            StartReco();
            greeting.SetActive(false);
            MicPic.SetActive(true);
            recOn = true;
        }
        else
        {
            StopReco();
            MicPic.SetActive(false);
            recOn = false;
        }
    }

    public async void StartReco()
    {
        if (!Microphone.IsRecording(Microphone.devices[0]))
        {
            Chats.SetActive(false);
            message = "Listening....";
            OutputText.SetActive(true);
            change = true;
            Debug.Log("Microphone.Start: " + Microphone.devices[0]);
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
            Debug.Log("audioSource.clip channels: " + audioSource.clip.channels);
            Debug.Log("audioSource.clip frequency: " + audioSource.clip.frequency);
        }

        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        lock (threadLocker)
        {
            recognitionStarted = true;
            Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
        }
    }

    public async void StopReco()
    {


        if (recognitionStarted)
        {

            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);

            if (Microphone.IsRecording(Microphone.devices[0]))
            {
                Debug.Log("Microphone.End: " + Microphone.devices[0]);
                Microphone.End(null);
                lastSample = 0;
            }

            lock (threadLocker)
            {
                recognitionStarted = false;
                change = false;
                Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
                if (outputText.text != "Listening....")
                {
                    Invoke("SendAfterInterval", 2f);
                }
                else
                {
                    OutputText.SetActive(false);
                    Chats.SetActive(true);
                }
            }
        }
    }


    private void SendAfterInterval()
    {
        OutputText.SetActive(false);
        Chats.SetActive(true);
        GPTClient.SendChatRequest(outputText.text);
        Debug.Log("Request Sent");
        chat.sendVoiceMessage(outputText.text);
    }

    void Start()
    {
        start = false;
        change = false;
        recOn = false;
        stopRec = false;
        serviceKey = AppManager.jarvisAIspeechAPI;
        chat = GameObject.Find("ChatManager").GetComponent<ChatManager>();
        if (outputText == null)
        {
            Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else if (recoButton == null)
        {
            message = "recoButton property is null! Assign a UI Button to it.";
            Debug.LogError(message);
        }
        else
        {
            // Continue with normal initialization, Text and Button objects are present.
#if PLATFORM_ANDROID
            message = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else
            micPermissionGranted = true;
            message = "Click button to recognize speech";
#endif
            config = SpeechConfig.FromSubscription(serviceKey, "centralindia");
            pushStream = AudioInputStream.CreatePushStream();
            audioInput = AudioConfig.FromStreamInput(pushStream);
            recognizer = new SpeechRecognizer(config, audioInput);
            recognizer.Recognizing += RecognizingHandler;
            recognizer.Recognized += RecognizedHandler;
            recognizer.Canceled += CanceledHandler;

            foreach (var device in Microphone.devices)
            {
                // Debug.Log("DeviceName: " + device);
            }
            audioSource = GameObject.Find("MyAudioSource").GetComponent<AudioSource>();
        }
    }

    void Disable()
    {
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Recognized -= RecognizedHandler;
        recognizer.Canceled -= CanceledHandler;
        pushStream.Close();
        recognizer.Dispose();
    }

    void FixedUpdate()
    {

#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;
            message = "Listening....";
        }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#endif
        lock (threadLocker)
        {
            if (recoButton != null)
            {
                recoButton.interactable = micPermissionGranted;
            }
            if (outputText != null && change)
            {
                outputText.text = message;
            }
        }

        if (Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == true)
        {
            int pos = Microphone.GetPosition(Microphone.devices[0]);
            int diff = pos - lastSample;

            if (diff > 0)
            {
                float[] samples = new float[diff * audioSource.clip.channels];
                audioSource.clip.GetData(samples, lastSample);
                byte[] ba = ConvertAudioClipDataToInt16ByteArray(samples);
                if (ba.Length != 0)
                {
                    Debug.Log("pushStream.Write pos:" + Microphone.GetPosition(Microphone.devices[0]).ToString() + " length: " + ba.Length.ToString());
                    pushStream.Write(ba);
                }
            }
            lastSample = pos;
        }
        else if (!Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == false)
        {
            // Instructions if any
        }

        if (stopRec)
        {
            Recorder();
            stopRec = false;
        }
    }
}