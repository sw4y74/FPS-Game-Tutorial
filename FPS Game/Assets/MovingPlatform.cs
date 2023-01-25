using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
    float timeElapsed = 0f;
    float timeToMove = 5f;

    private void Start() {
        startPos = transform.position;
        endPos = new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("OnTriggerStay");
        StartCoroutine(MoveUp());
    }

    // private void OnTriggerExit(Collider other) {
    //     Debug.Log("OnTriggerExit");
    //     StartCoroutine(MoveDown());    
    //     }

    IEnumerator MoveDown()
    {
        Vector3 currentPosition = transform.position;
        while (timeElapsed < timeToMove)
        {
            transform.position = Vector3.Lerp(currentPosition, startPos, (timeElapsed / timeToMove));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        timeElapsed = 0f;
        transform.position = startPos;
        yield return null;    
    }

    IEnumerator MoveUp()
    {
        var t = 0f;
        var start = transform.position;
        var target = new Vector3(transform.position.x, transform.position.y + 10f, transform.position.z);

        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;

            if (t > 1) t = 1;

            transform.position = Vector3.Lerp(start, target, t);

            yield return null;
        }
    }
}
