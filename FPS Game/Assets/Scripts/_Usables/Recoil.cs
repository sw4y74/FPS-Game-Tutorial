﻿using UnityEngine;

public class Recoil : MonoBehaviour
{
    //rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    //hipfire
    // [SerializeField] private float recoilX;
    // [SerializeField] private float recoilY;
    // [SerializeField] private float recoilZ;

    //settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ) {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
