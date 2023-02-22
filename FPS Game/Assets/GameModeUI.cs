using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameModeUI : MonoBehaviour, IOnEventCallback
{
    [SerializeField] TMP_Text FFAGameOverText;
    [SerializeField] TMP_Text FFATimerText;
    bool startTimer = false;
    bool timerCompleted = false;
    double timerIncrementValue;
    double startTime;
    double timer;
    ExitGames.Client.Photon.Hashtable timerValue;
    public Action OnTimerCompleted;

    public IEnumerator SetFFAGameOverTextRoutine(string text, float delay)
    {
        FFAGameOverText.text = text;
        FFAGameOverText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay-1);
        FFAGameOverText.transform.parent.gameObject.SetActive(false);
    }

    public void FFA_StartTimer(double _timer = 240) {
        timer = _timer;
        FFATimerText.transform.parent.gameObject.SetActive(true);
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
         {
             timerValue = new ExitGames.Client.Photon.Hashtable();
             startTime = PhotonNetwork.Time;
             startTimer = true;
             timerValue.Add("StartTime", startTime);
             PhotonNetwork.CurrentRoom.SetCustomProperties(timerValue);
         }
         else
         {
            StartCoroutine(FFA_ClientGetTimerValue());
         }
    }

    IEnumerator FFA_ClientGetTimerValue() {
        yield return new WaitUntil(() => PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("StartTime"));
        startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        startTimer = true;
    }

    void FFA_Timer()
    {
        if (!startTimer) return;

        timerIncrementValue = PhotonNetwork.Time - startTime;

        if (timerIncrementValue >= timer && !timerCompleted)
        {
            timerCompleted = true;
            //Timer Completed
            //Do What Ever You What to Do Here
            OnTimerCompleted?.Invoke();
        }
        else if (timerCompleted) FFATimerText.text = FormatTimer(0);
        else FFATimerText.text = FormatTimer(timer - timerIncrementValue);
    }

    public static string FormatTimer(double time) {
        int minutes = (int)Math.Floor(time / 60F);
        int seconds = (int)Math.Floor(time - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        return niceTime;
    }

    void Update()
    {
        FFA_Timer();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 51) {

        }
    }
}
