using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    public enum AnimTypes
    {
        RUN, SHOOT, DIE
    };

    [SerializeField] private Animator animator;
    [SerializeField] private AnimTypes AnimationName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (AnimationName)
        {
            case AnimTypes.RUN:
                animator.SetFloat("Speed", 1f);
                break;
            case AnimTypes.SHOOT:
                animator.SetBool("IsShooting", true);
                break;
            case AnimTypes.DIE:
                animator.SetBool("IsDead", true);
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        animator.SetFloat("Speed", 0);
        animator.SetBool("IsShooting", false);
        animator.SetBool("IsDead", false);
    }
}
