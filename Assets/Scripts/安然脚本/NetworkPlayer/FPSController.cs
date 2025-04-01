using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class FPSController : NetworkBehaviour
{


    [Header("正常移动速度")]
    public float normalspeed = 3f;

    [Header("鼠标X轴速度")]
    public float sensityX = 10f;

    [Header("鼠标Y轴速度")]
    public float sensityY = 10f;

    [Header("最小视野")]
    public float minfield = -80f;

    [Header("最大视野")]
    public float maxfield = 80f;

    [Header("玩家摄像机")]
    public Transform player_camera;

    private CharacterController controller;

    private float gravity = -15f;

    private float jumpHeight = 0.8f;

    private Vector3 velocity;

    private float xRotation;

    public MeshRenderer[] Nolook;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameManager.instance.OnStartGame.AddListener(() =>
        {
            GameManager.instance.isGameing = true;
            if (IsLocalPlayer)
            {
                foreach (var o in Nolook)
                {
                    o.enabled = false;
                }
            }
            else
            {
                player_camera.gameObject.SetActive(false);
            }
        });
    }

    void Update()
    {
        if (!GameManager.instance.isGameing) return;

        if (IsLocalPlayer)
        {
            LocalInput();
        }
    }

    
    void LocalInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        float vertical = Input.GetAxisRaw("Vertical");

        PlayerMove(horizontal,vertical);

        Jump();

        CameraControl();
    }

    void PlayerMove(float horizontal, float vertical)
    {
        Vector3 movedir = transform.forward * vertical + transform.right * horizontal;

        if(controller.enabled)
        controller.Move(movedir.normalized * normalspeed * Time.deltaTime);
    }

    void Jump()
    {
        velocity.y += gravity * Time.deltaTime;

        if(controller.enabled)
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void CameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensityX;

        float mouseY = Input.GetAxis("Mouse Y") * sensityY;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, minfield, maxfield);

        player_camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
