using System;
using System.IO;
using System.Collections;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;
using System.Threading.Tasks;

public class Speech : MonoBehaviour
{
    static string serviceKey = AppManager.jarvisAIspeechAPI;
    static string speechRegion = "centralindia";
    public static SpeechSynthesizer speechSynthesizer;

    // Add a variable to store the current speech task.
    private static TaskCompletionSource<int> speechTaskCompletionSource;

    static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                // Debug.Log($"Speech synthesized for text: [{text}]");
                break;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                Debug.Log($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Debug.Log($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Debug.Log($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    Debug.Log($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
            default:
                break;
        }
    }

    public void callSS(string message)
    {
        StartCoroutine(SynthesizeSpeech(message));
    }

    IEnumerator SynthesizeSpeech(string message)
    {
        var speechConfig = SpeechConfig.FromSubscription(serviceKey, speechRegion);
        speechConfig.SpeechSynthesisVoiceName = "en-US-GuyNeural";

        using (speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            // Get text from the console and synthesize to the default speaker.
            string text = message;

            // Store the task completion source to allow cancelling.
            speechTaskCompletionSource = new TaskCompletionSource<int>();

            var task = speechSynthesizer.SpeakTextAsync(text);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // Wait for the speech task to complete or be cancelled.
            yield return new WaitUntil(() => task.IsCompleted || speechTaskCompletionSource.Task.IsCompleted);

            // If the speech task was completed, check for the result.
            if (task.IsCompleted)
            {
                var speechSynthesisResult = task.Result;
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }

        yield return null;
    }

    // Add a method to cancel the currently playing speech.
    public static void CancelSpeech()
    {
        if (speechSynthesizer != null && speechTaskCompletionSource != null && !speechTaskCompletionSource.Task.IsCompleted)
        {
            speechSynthesizer.StopSpeakingAsync();
            speechTaskCompletionSource.SetResult(0);
        }
    }
}