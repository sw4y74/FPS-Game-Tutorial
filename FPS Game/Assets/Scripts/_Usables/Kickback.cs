using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickback : MonoBehaviour
{
    //rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    //positions
    private Vector3 currentPosition;
    private Vector3 targetPosition;

    //hipfire
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    //knockback
    [SerializeField] private float knockbackZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);
        currentPosition = Vector3.Slerp(currentPosition, targetPosition, snappiness * Time.fixedDeltaTime);
        currentPosition.x = 0f;
        transform.localRotation = Quaternion.Euler(currentRotation);
        transform.localPosition = currentPosition;
    }

    public void KickbackFire(float knockbackZ)
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        targetPosition += new Vector3(0f, 0f, Random.Range(-knockbackZ - -(knockbackZ/5), -knockbackZ));
    }
}
