using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class Dot : MonoBehaviour
{
    public float moveSpeed = 10f;

    [NonSerialized]
    public int x = -1, y = -1;
    [NonSerialized]
    public SpriteRenderer spriteRenderer;

    Animator animator;
    Coroutine moveCoroutine;

    protected void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public bool IsReady { get => moveCoroutine == null && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1; }
    
    public void PlaySpawnAnimation()
    {
        animator.SetTrigger("Spawn");
    }

    public void PlayCollapseAnimation()
    {
        animator.SetTrigger("Collapse");
    }

    public void MoveTowards(Vector3 position)
    {
        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveTowardsCoroutine(position));
    }

    IEnumerator MoveTowardsCoroutine(Vector3 position)
    {
        while(transform.position != position)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, Time.deltaTime * moveSpeed);
            yield return null;
        }
        moveCoroutine = null;
    }
}
