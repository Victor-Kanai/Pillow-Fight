using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    Animator animator;

    public GameObject enemy;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (enemy.GetComponent<EnemyBehavior>().actualState.ToString() != "Idle" && !animator.GetBool("Attack"))
        {
            animator.SetBool("Move", true);
        }

        if(enemy.GetComponent<EnemyBehavior>().actualState.ToString() == "Idle")
        {
            animator.SetBool("Move", false);
        }
    }

    public void AttackAnim()
    {
        animator.SetBool("Attack", true);

        StartCoroutine(AttackDelay());
    }

    IEnumerator AttackDelay()
    {

        yield return new WaitForSeconds(.5f);

        animator.SetBool("Attack", false);
    }
}
