using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathCam : MonoBehaviour
{
    [SerializeField] GameObject deathCamElements;
    [SerializeField] TextMeshProUGUI deathCamKiller;
    [SerializeField] Image deathCamOverlay;

    public void DisplayDeathInfo(float timeout, string killerName)
    {
        StartCoroutine(DisplayDeathInfoRoutine(timeout, killerName));
    }

    IEnumerator DisplayDeathInfoRoutine(float timeout, string killerName)
    {
        deathCamElements.SetActive(true);
        deathCamKiller.text = "Killed by "+killerName;

        yield return new WaitForSeconds(timeout);

        deathCamElements.SetActive(false);
    }
}
