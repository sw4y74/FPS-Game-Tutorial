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

    //knockback
    [SerializeField] private float knockbackZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Update()
    {
        returnSpeed = transform.root.gameObject.GetComponent<PlayerController>().CurrentlyEquippedItem().gun.returnSpeed;
        snappiness = transform.root.gameObject.GetComponent<PlayerController>().CurrentlyEquippedItem().gun.snappiness;
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);
        currentPosition = Vector3.Slerp(currentPosition, targetPosition, snappiness * Time.fixedDeltaTime);
        currentPosition.x = 0f;
        transform.localRotation = Quaternion.Euler(currentRotation);
        transform.localPosition = currentPosition;
    }

    public void KickbackFire(float knockbackZ, float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        targetPosition += new Vector3(0f, 0f, Random.Range(-knockbackZ - -(knockbackZ/5), -knockbackZ));
    }
}
