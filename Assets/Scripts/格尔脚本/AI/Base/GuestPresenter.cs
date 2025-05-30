using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;
namespace Helltal.Gelercat
{
    /// <summary>
    /// 在这层同步表现层的逻辑
    /// </summary>

    public class GuestPresenter : MonoBehaviour
    {

        public Animator animator;

        private void Awake()
        {
            // animator = this.gameObject.GetComponent<Animator>();
            // if (animator == null)
            // {
            //     animator = this.gameObject.AddComponent<Animator>();
            // }
            if(animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    animator = gameObject.AddComponent<Animator>();
                }
            }
        }

        public void PlayAnimation(string animName)
        {
            animator.Play(animName); 
        }

        public void SetBool(string param, bool value)
        {
            animator.SetBool(param, value);
        }

        public void SetTrigger(string param)
        {
            animator.SetTrigger(param);
        }

        public void SetFloat(string param, float value)
        {
            animator.SetFloat(param, value);
        }

        // tools 应该不用同步
        public AnimatorStateInfo  GetCurrentAnimationState()
        {
            return animator.GetCurrentAnimatorStateInfo(0);
        }

        public float GetCurrentAnimationCliplength()
        {
            return animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        }

        public void SetAnimatiorSpeed(float speed)
        {
            animator.speed = speed;
        }
    }
}