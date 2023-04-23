﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameStateExecute : MonoBehaviour
{
    public PhotonView PV;

    public GameObject StartButton, AimJoystick, ResponePanel, ResultPanel;
    public Button ReGameButton;
    public Text ResultText;

    WaitForSeconds waitForSecond = new WaitForSeconds(1f);


    private void Start()
    { 
        init();
        GameStateManager.Instance.LobbyStateAction += OnLobbyState;
        GameStateManager.Instance.ReadyStateAction += OnReadyState;
        GameStateManager.Instance.FightStateAction += OnFightState;
        GameStateManager.Instance.ResultStateAction += OnResultState;
        GameObject.Find("Canvas").transform.Find("gameStartBTN").gameObject.GetComponent<Button>().onClick.AddListener(ChangeReadyState);
    }

    private void init()
    {
        PV = GetComponent<PhotonView>();

        StartButton = GameObject.Find("Canvas").transform.Find("gameStartBTN").gameObject;
        AimJoystick = GameObject.Find("Canvas").transform.Find("Aim_Joystick").gameObject;
        ResponePanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        ResultPanel = GameObject.Find("Canvas").transform.Find("ResultPanel").gameObject;
        ReGameButton = ResultPanel.transform.Find("regameBTN").gameObject.GetComponent<Button>();
        ResultText = ResultPanel.transform.Find("resultText").gameObject.GetComponent<Text>();
    }

    private void OnLobbyState()
    {
        if (PhotonNetwork.IsMasterClient)
            StartButton.SetActive(true);
    }

    private void OnReadyState()
    {
        if (ReGameButton.IsActive())
            ReGameButton.onClick.Invoke();

        if (PhotonNetwork.IsMasterClient)
            StartButton.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ReadyCoroutine());
    }

    private void OnFightState()
    {
        AimJoystick.SetActive(true);
    }

    private void OnResultState()
    {
        AimJoystick.SetActive(false);
        ResponePanel.SetActive(false);
        ResultPanel.SetActive(true);
        ResultText.text = "경기 결과\n" + GameManager.Instance.RangkingLogText.text;

    }

    public void ChangeReadyState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Ready);
        }
    }

    IEnumerator ReadyCoroutine()
    {
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "3", 200);
        yield return waitForSecond;
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "2", 200);
        yield return waitForSecond;
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "1", 200);
        yield return waitForSecond;
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "게임 시작!", 200);


        PV.RPC("ChangeGameStateForAllUser", RpcTarget.AllBuffered, GameStateManager.GameState.Fight);

        yield return waitForSecond;
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "", 100);
    }
}
