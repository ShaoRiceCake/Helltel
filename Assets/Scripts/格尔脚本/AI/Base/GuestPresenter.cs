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
    [RequireComponent(typeof(Animator))]
    public class GuestPresenter : MonoBehaviour
    {

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
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
    }
}