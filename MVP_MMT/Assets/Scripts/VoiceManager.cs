using System;
using System.Collections;
using Meta.WitAi.CallbackHandlers;
using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VoiceManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private TextMeshProUGUI transcriptionText;

    [Header("Voice Events")]
    [SerializeField] private UnityEvent<string> completeTranscription;

    [Header("Listening Configuration")]
    [SerializeField] private float listeningDuration = 5f; // Default duration in seconds.

    private Coroutine listeningCoroutine;

    /// <summary>
    /// Method to activate voice listening. Call this on button click.
    /// </summary>
    public void StartListening()
    {
        if (listeningCoroutine != null)
        {
            StopCoroutine(listeningCoroutine);
        }

        appVoiceExperience.Activate();
        listeningCoroutine = StartCoroutine(StopListeningAfterDuration());
    }

    /// <summary>
    /// Stop listening after the specified duration.
    /// </summary>
    private IEnumerator StopListeningAfterDuration()
    {
        yield return new WaitForSeconds(listeningDuration);
        appVoiceExperience.Deactivate();
    }

    public void StopListening()
    {
        appVoiceExperience.Deactivate();
    }

    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
    }

    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
    }

    private void OnPartialTranscription(string transcription)
    {
        transcriptionText.text = transcription;
    }

    private void OnFullTranscription(string transcription)
    {
        completeTranscription?.Invoke(transcription);
    }

    /// <summary>
    /// Updates the listening duration dynamically.
    /// </summary>
    /// <param name="duration">New duration in seconds.</param>
    public void SetListeningDuration(float duration)
    {
        listeningDuration = duration;
    }
}
