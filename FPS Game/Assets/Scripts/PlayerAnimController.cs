using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;

    void Update()
    {
        
    }

    public void MovementAnimation(float movementX, float movementY)
    {
        playerAnimator.SetFloat("directionX", movementX);
        playerAnimator.SetFloat("directionY", movementY);
    }

    public void CrouchAnimationToggle(bool toggle)
    {
        playerAnimator.SetBool("isCrouching", toggle);
    }
}
