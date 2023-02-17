using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] TMP_Text FFAGameOverText;

    public IEnumerator SetFFAGameOverTextRoutine(string text)
    {
        FFAGameOverText.text = text;
        FFAGameOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        FFAGameOverText.gameObject.SetActive(false);
    }
}
