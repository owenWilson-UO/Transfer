using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator animator;
    
    [SerializeField] PlayerMovement playerMovement;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Walking
        if (playerMovement.verticalMovement == 1) { animator.SetBool("isWalking", true); }
        else { animator.SetBool("isWalking", false); }

        if (playerMovement.verticalMovement == -1) { animator.SetBool("isBackWalking", true); }
        else { animator.SetBool("isBackWalking", false); }

        if (playerMovement.horizontalMovement == 1) { animator.SetBool("isRightWalking", true); }
        else { animator.SetBool("isRightWalking", false); }

        if (playerMovement.horizontalMovement == -1) { animator.SetBool("isLeftWalking", true); }
        else { animator.SetBool("isLeftWalking", false); }

        if (playerMovement.isSprinting) { animator.SetBool("isSprinting", true); }
        else { animator.SetBool("isSprinting", false); }

        if (playerMovement.isSliding) { animator.SetBool("isSliding", true); }
        else { animator.SetBool("isSliding", false); }

        if (playerMovement.isCrouching) { animator.SetBool("isCrouching", true); }
        else { animator.SetBool("isCrouching", false); }

        //if (!playerMovement.isGrounded) { animator.SetBool("isJumping", true); } 
        //else { animator.SetBool("isFalling", false); }
    }

    //void EndJumpAnimation()
    //{
    //    animator.SetBool("isJumping", false);
    //    animator.SetBool("isFalling", true);
    //}
}
