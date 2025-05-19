using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class AnimNode_Ballon_pratol : StateMachineBehaviour
{

    private MonoBehaviour GetOwner(Animator animator)
    {
        // 尝试获取你希望作为 owner 的组件，比如 BallonPatrolController 或其它
        return animator.GetComponent<MonoBehaviour>(); // 或更具体的脚本类型
    }
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var owner = GetOwner(animator);
        AudioManager.Instance.Play("气球巡逻", animator.transform.position, loop: true,owner: owner);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioManager.Instance.Stop("气球巡逻", GetOwner(animator));
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
