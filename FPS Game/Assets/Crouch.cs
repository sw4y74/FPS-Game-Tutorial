using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : MonoBehaviour
{
    public bool isCrouching = false;
    [SerializeField] Transform cameraRecoil;
    PlayerController pc;

    void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    public void CrouchToggler()
    {
        if (!isCrouching && Input.GetKey(KeyCode.LeftControl))
        {
            //crouch
            GetComponent<CharacterController>().height = 1.2f;
            GetComponent<CharacterController>().center = new Vector3(GetComponent<CharacterController>().center.x, -0.3f, GetComponent<CharacterController>().center.z);

            pc.ChangePlayerSpeed(pc.walkSpeed * 0.5f);

            Vector3 posA = new Vector3(0f, 0.766f, 0f);
            Vector3 posB = new Vector3(0f, 0.310f, 0f);
            StartCoroutine(LerpPosition(posA, posB));

            //cameraRecoil.position = new Vector3(cameraRecoil.position.x, 0.466f, cameraRecoil.position.z);
            isCrouching = true;
            GetComponent<PlayerAnimController>().CrouchAnimationToggle(isCrouching);
        }


        if (isCrouching && !Input.GetKey(KeyCode.LeftControl)) 
        {
            var cantStandup = Physics.Raycast(transform.position, Vector3.up, 2f);

            if (!cantStandup)
            {
                //standup
                GetComponent<CharacterController>().height = 1.8f;
                GetComponent<CharacterController>().center = new Vector3(GetComponent<CharacterController>().center.x, 0f, GetComponent<CharacterController>().center.z);

                if (pc.CurrentlyEquippedItem().GetComponent<SniperScope>())
                {
                    if (pc.CurrentlyEquippedItem().GetComponent<SniperScope>().scopeOn) pc.ChangePlayerSpeed(pc.originalWalkSpeed * 0.5f);
                    else pc.ChangePlayerSpeed(pc.originalWalkSpeed * 0.9f);
                }
                else pc.ChangePlayerSpeed(pc.originalWalkSpeed);


                Vector3 posA = new Vector3(0f, 0.310f, 0f);
                Vector3 posB = new Vector3(0f, 0.766f, 0f);
                StartCoroutine(LerpPosition(posA, posB));

                isCrouching = false;
                GetComponent<PlayerAnimController>().CrouchAnimationToggle(isCrouching);
            }

        }
    }

    IEnumerator LerpPosition(Vector3 a, Vector3 b)
    {
        float timeElapsed = 0;
        float lerpDuration = 0.3f;

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
