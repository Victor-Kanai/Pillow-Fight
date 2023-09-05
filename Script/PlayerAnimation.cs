using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    PlayerBehavior player;
    Animator animator;

    void Start()
    {
        player = FindObjectOfType<PlayerBehavior>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player.isFalling)
        {
            animator.SetBool("Move", false);

            animator.SetTrigger("Fall");
        }

        if (!player.isFalling && !player.isPlayingOnMobile)
        {
            if (player.direction.magnitude > .1f)
            {
                animator.SetBool("Move", true);
            }

            if (Input.GetButton("Fire1") && !player.isThrow)
            {
                animator.SetTrigger("attack");
            }

            if (Input.GetButton("Fire2") && player.isThrow)
            {
                animator.SetTrigger("throw");
            }

            if (Input.GetKey(KeyCode.E) && !player.isDashing)
            {
                animator.SetTrigger("dash");
            }
        }

        if(!player.isFalling && player.isPlayingOnMobile)
        {
            if (player.direction.magnitude > .1f)
            {
                animator.SetBool("Move", true);
            }

            if(player.hasAttackedOnMobile && !player.isThrow)
            {
                animator.SetTrigger("attack");
            }

            if (player.hasThrownOnMobile && player.isThrow)
            {
                animator.SetTrigger("throw");
            }

            if (player.hasDashedOnMobile && !player.isDashing)
            {
                animator.SetTrigger("dash");
            }
        }

        if (player.direction.magnitude < .1f)
        {
            animator.SetBool("Move", false);
        }

        if (player.hasAttackedOnMobile || player.hasDashedOnMobile || player.hasThrownOnMobile)
        {
            StartCoroutine(ResetSkillsOnMobile());
        }
    }

    IEnumerator ResetSkillsOnMobile()
    {
        yield return new WaitForSeconds(1);

        player.hasAttackedOnMobile = false;
        player.hasDashedOnMobile = false;
        player.hasThrownOnMobile = false;
    }
}
