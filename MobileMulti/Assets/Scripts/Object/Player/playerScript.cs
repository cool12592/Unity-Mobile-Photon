﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.Collections; // NativeArray
using Cinemachine;

public class playerScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Text NickNameText;
    public bool isActive = true;

    [SerializeField]
    private GameObject moveJoystick;
    [SerializeField]
    private GameObject aimJoystick;
    private Button attackButton;

    // Start is called before the first frame update
    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName.ToString() : PV.Owner.NickName.ToString();
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        if (PV.IsMine)
        {   
            GameManager.Instance.myplayer = gameObject;
            GameObject.Find("ObjectPoolParent").transform.GetChild(0).gameObject.SetActive(true);
            InitCamera();
            InitJoystick();
            
        }
    }

    void InitCamera()
    {
        // 2D 카메라
        var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;
    }

    void InitJoystick()
    {
        moveJoystick = GameObject.Find("Canvas").transform.Find("Move_Joystick").gameObject;
        moveJoystick.SetActive(true);
        moveJoystick.GetComponent<JoyStickScript>().MyPlayer = gameObject;


        aimJoystick = GameObject.Find("Canvas").transform.Find("Aim_Joystick").gameObject;
        aimJoystick.GetComponent<JoyStickScript>().MyPlayer = gameObject;


        attackButton = aimJoystick.transform.Find("attack_BTN").gameObject.GetComponent<Button>();
        attackButton.onClick.AddListener(GetComponent<PlayerAttack>().Attack);
    }

    private void Start()
    {
        GameStateManager.Instance.ReadyStateAction += OnReadyState;
        GameStateManager.Instance.FightStateAction += OnFightState;
        GameStateManager.Instance.ResultStateAction += OnResultState;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.ReadyStateAction -= OnReadyState;
        GameStateManager.Instance.FightStateAction -= OnFightState;
        GameStateManager.Instance.ResultStateAction -= OnResultState;
    }

    private void OnReadyState()
    {
        isActive = false; //행동 못 하게

        if (PV.IsMine)
        {
            ChangeRandomPosition();
            GetComponent<PlayerMovement>().DashInit();
        }
    }

    private void OnFightState()
    {
        isActive = true; 
    }

    private void OnResultState()
    {
        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    private void ChangeRandomPosition()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-5f, 5f), 0);
        GetComponent<Animator>().SetBool("walk", false);
    }
}
