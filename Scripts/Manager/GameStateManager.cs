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
    
    public enum GameState {None, Lobby, Ready, Fight, Result};
    public GameState NowGameState { get; private set; }

    public event Action LobbyStateAction;
    public event Action ReadyStateAction;
    public event Action FightStateAction;
    public event Action ResultStateAction;

    WaitForSeconds waitForSecond = new WaitForSeconds(1f);

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
        NowGameState = GameState.None;
        GameObject.Find("Canvas").transform.Find("gameStartBTN").gameObject.GetComponent<Button>().onClick.AddListener(OnReadyState);
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
            RankingBoardManager.Instance.StartButton.SetActive(true);
    }

    private void EnterReadyState()
    {
        ReadyStateAction?.Invoke();

        if (RankingBoardManager.Instance.ReGameButton.IsActive())
            RankingBoardManager.Instance.ReGameButton.onClick.Invoke();

        if (PhotonNetwork.IsMasterClient)
            RankingBoardManager.Instance.StartButton.SetActive(false);
        
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ReadyCoroutine());
    }

    private void EnterFightState()
    {
        FightStateAction?.Invoke();

        RankingBoardManager.Instance.AimJoystick.SetActive(true);
    }

    private void EnterResultState()
    {
        ResultStateAction?.Invoke();
        RankingBoardManager.Instance.AimJoystick.SetActive(false);
        RankingBoardManager.Instance.ResponePanel.SetActive(false);
        RankingBoardManager.Instance.ResultPanel.SetActive(true);
        RankingBoardManager.Instance.ResultText.text = "경기 결과\n" + RankingBoardManager.Instance.RangkingLogText.text;
    }

    public void OnReadyState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeGameStateForAllUser(GameState.Ready);
        }
    }

    IEnumerator ReadyCoroutine()
    {
        RankingBoardManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "3",200);
        yield return waitForSecond;
        RankingBoardManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "2", 200);
        yield return waitForSecond;
        RankingBoardManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "1", 200);
        yield return waitForSecond;
        RankingBoardManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "게임 시작!", 200);


        PV.RPC("ChangeGameStateForAllUser", RpcTarget.AllBuffered,GameState.Fight);

        yield return waitForSecond;
        RankingBoardManager.Instance.PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered, "",100);
    }
}