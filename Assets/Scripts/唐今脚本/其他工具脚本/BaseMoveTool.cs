using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class BaseMoveTool : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    public float jumpForce = 5f; // 跳跃力
    private Rigidbody _rb; // 刚体组件
    private ObiActor _obiActor;
    private void Start()
    {
        // 获取刚体组件
        _rb = GetComponent<Rigidbody>();
        _obiActor =  GetComponent<ObiActor>();
    }

    private void Update()
    {
        // 检测跳跃输入
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        // 检测WASD按键
        var movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) // 向前移动
        {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S)) // 向后移动
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A)) // 向左移动
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D)) // 向右移动
        {
            movement += Vector3.right;
        }

        // 移动物体
        if (movement != Vector3.zero)
        {
            if (!_obiActor)
            {
                _rb.AddForce(movement.normalized * moveSpeed, ForceMode.Acceleration);
            }
            else
            {
                _obiActor.AddForce(movement.normalized * moveSpeed, ForceMode.Acceleration);
            }
        }
    }

    private void Jump()
    {
        // 施加一个向上的冲量
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}