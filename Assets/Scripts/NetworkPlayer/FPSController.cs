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


    [Header("�����ƶ��ٶ�")]
    public float normalspeed = 3f;

    [Header("���X���ٶ�")]
    public float sensityX = 10f;

    [Header("���Y���ٶ�")]
    public float sensityY = 10f;

    [Header("��С��Ұ")]
    public float minfield = -80f;

    [Header("�����Ұ")]
    public float maxfield = 80f;

    [Header("��������")]
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
