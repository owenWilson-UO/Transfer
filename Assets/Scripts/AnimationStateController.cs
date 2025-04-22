using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator animator;
    
    [SerializeField] PlayerMovement playerMovement;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //Walking
        if (playerMovement.verticalMovement == 1) { animator.SetBool("isWalking", true); }
        else { animator.SetBool("isWalking", false); }

        //Back Walk
        if (playerMovement.verticalMovement == -1) { animator.SetBool("isBackWalking", true); }
        else { animator.SetBool("isBackWalking", false); }

        //Right Walk
        if (playerMovement.horizontalMovement == 1) { animator.SetBool("isRightWalking", true); }
        else { animator.SetBool("isRightWalking", false); }

        //Left Walk
        if (playerMovement.horizontalMovement == -1) { animator.SetBool("isLeftWalking", true); }
        else { animator.SetBool("isLeftWalking", false); }

        //Sprint
        if (playerMovement.isSprinting) { animator.SetBool("isSprinting", true); }
        else { animator.SetBool("isSprinting", false); }

        //Sliding
        if (playerMovement.isSliding) { animator.SetBool("isSliding", true); }
        else { animator.SetBool("isSliding", false); }

        //Crouching
        if (playerMovement.isCrouching) { animator.SetBool("isCrouching", true); }
        else { animator.SetBool("isCrouching", false); }
    }
}
