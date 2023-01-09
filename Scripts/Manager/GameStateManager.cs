using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System;
public class GameStateManager : MonoBehaviour
{
    public PhotonView PV;
    private static GameStateManager instance = null;
    
    public enum GameState {Nope, Lobby, Ready, Fight, Result};
    public GameState NowGameState { get; private set; }

    public event Action LobbyStateAction;
    public event Action ReadyStateAction;
    public event Action FightStateAction;
    public event Action ResultStateAction;


    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static GameStateManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NowGameState = GameState.Nope;
        GameObject.Find("Canvas").transform.Find("gameStartBTN").gameObject.GetComponent<Button>().onClick.AddListener(GameStateManager.Instance.OnReadyState);
    }

    [PunRPC]
    public void ChangeGameStateForAllUser(GameState gameState)
    {
        PV.RPC("ChangeGameStateRPC", RpcTarget.AllBuffered, (int)gameState);
    }

    [PunRPC]
    public void ChangeGameStateRPC(int gameState)
    {
        ChangeGameState((GameState)gameState);
    }

    public void ChangeGameState(GameState gameState)
    {
        if (NowGameState != gameState)
        {
            switch (gameState)
            {
                case GameState.Lobby:
                    EnterLobbyState();
                    break;
                case GameState.Ready:
                    EnterReadyState();
                    break;
                case GameState.Fight:
                    EnterFightState();
                    break;
                case GameState.Result:
                    EnterResultState();
                    break;
                default:
                    break;
            }
        }
        NowGameState = gameState;
    }

    private void EnterLobbyState()
    {
        LobbyStateAction?.Invoke();

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.StartButton.SetActive(true);
    }

    private void EnterReadyState()
    {
        ReadyStateAction?.Invoke();

        if (GameManager.Instance.ReGameButton.IsActive())
            GameManager.Instance.ReGameButton.onClick.Invoke();

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.StartButton.SetActive(false);
        
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ReadyCoroutine());
    }

    private void EnterFightState()
    {
        FightStateAction?.Invoke();

        GameManager.Instance.AimJoystick.SetActive(true);
    }

    private void EnterResultState()
    {
        ResultStateAction?.Invoke();
        GameManager.Instance.AimJoystick.SetActive(false);
        GameManager.Instance.ResponePanel.SetActive(false);
        GameManager.Instance.ResultPanel.SetActive(true);
        GameManager.Instance.ResultText.text = "경기 결과\n" + GameManager.Instance.RangkingLogText.text;
    }

    public void OnReadyState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Ready);
        }
    }

    IEnumerator ReadyCoroutine()
    {
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "3",200);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "2", 200);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "1", 200);
        yield return new WaitForSeconds(1f);
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "게임 시작!", 200);


        PV.RPC("ChangeGameStateForAllUser", RpcTarget.AllBuffered,GameState.Fight);

        yield return new WaitForSeconds(1f);
        GameManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "",100);
    }
}