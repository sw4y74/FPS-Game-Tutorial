using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitmarker : MonoBehaviour
{
    public GameObject hitmarker;
    public GameObject hitmarkerHS;

    void Start()
    {
        hitmarker.SetActive(false);
        hitmarkerHS.SetActive(false);
    }

    public void ShowHit()
    {
        HitEnable();
        Invoke("HitDisable", 0.1f);
    }

    public void ShowHitHS()
    {
        HitEnableHS();
        Invoke("HitDisableHS", 0.1f);
    }

    void HitEnable()
    {
        hitmarker.SetActive(true);
    }

    void HitDisable()
    {
        hitmarker.SetActive(false);
    }

    void HitEnableHS()
    {
        hitmarkerHS.SetActive(true);
    }

    void HitDisableHS()
    {
        hitmarkerHS.SetActive(false);
    }
}
