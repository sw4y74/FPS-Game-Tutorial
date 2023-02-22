using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MonoBehaviour
{
    public bool isCrouching = false;
    [SerializeField] Transform cameraRecoil;
    [SerializeField] LayerMask layerToIgnore;
    float standPositionY;
    float crouchPositionY;

    private void Start() {
        standPositionY = cameraRecoil.localPosition.y;
        crouchPositionY = standPositionY - 1.4f;
    }

    public void CrouchToggler()
    {
        if (!isCrouching && Input.GetKey(KeyCode.LeftControl))
        {
            //crouch
            // GetComponent<CharacterController>().height = 1.2f;
            // GetComponent<CharacterController>().center = new Vector3(GetComponent<CharacterController>().center.x, -0.3f, GetComponent<CharacterController>().center.z);

            Vector3 posA = new Vector3(0f, standPositionY, 0f);
            Vector3 posB = new Vector3(0f, crouchPositionY, 0f);
            StartCoroutine(LerpPosition(posA, posB));

            isCrouching = true;
            GetComponent<PlayerAnimController>().CrouchAnimationToggle(isCrouching);
        }


        if (isCrouching && !Input.GetKey(KeyCode.LeftControl)) 
        {

            if (!Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, GetComponent<PlayerMovement>().playerCollider.height+0.05f, ~layerToIgnore))
            {
                    //standup
                    // GetComponent<CharacterController>().height = 1.9f;
                    // GetComponent<CharacterController>().center = new Vector3(GetComponent<CharacterController>().center.x, 0f, GetComponent<CharacterController>().center.z);

                    Vector3 posA = new Vector3(0f, crouchPositionY, 0f);
                    Vector3 posB = new Vector3(0f, standPositionY, 0f);
                    StartCoroutine(LerpPosition(posA, posB));

                    isCrouching = false;
                    GetComponent<PlayerAnimController>().CrouchAnimationToggle(isCrouching);
            }

        }
    }

    IEnumerator LerpPosition(Vector3 a, Vector3 b)
    {
        float timeElapsed = 0;
        float lerpDuration = 0.15f;

        while (timeElapsed < lerpDuration)
        {
            cameraRecoil.localPosition = Vector3.Lerp(a, b, (timeElapsed / lerpDuration));
            timeElapsed += Time.deltaTime;

            // Yield here
            yield return null;
        }
        // Make sure we got there
        cameraRecoil.localPosition = b;
        yield return null;
    }
}
