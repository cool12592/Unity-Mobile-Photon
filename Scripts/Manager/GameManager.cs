﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System;
public class GameManager : MonoBehaviour
{
    public PhotonView PV;
    private static GameManager instance = null;
    private Text ScreenText;
    public Text RangkingLogText { get; private set; }
    private Queue<KeyValuePair<string, string>> killLogQueue = new Queue<KeyValuePair<string, string>>();
    private Dictionary<string, int> RankingBoard = new Dictionary<string, int>();

    private readonly float rankingBoardsynchCoolTime = 1f;
    private float rankingBoardsynchCoolTimer = 1f;
  
    public GameObject myplayer;
    public GameObject StartButton;
    public GameObject AimJoystick;
    public GameObject ResponePanel;
    public GameObject ResultPanel;
    public Button ReGameButton;
    public Text ResultText;

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
        Screen.SetResolution(960, 540, false);
    }

    public static GameManager Instance
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
        ScreenText = GameObject.Find("Canvas").transform.Find("ScreenText").gameObject.GetComponent<Text>();
        RangkingLogText = GameObject.Find("Canvas").transform.Find("RankingLog").gameObject.GetComponent<Text>();
        StartButton = GameObject.Find("Canvas").transform.Find("gameStartBTN").gameObject;
        AimJoystick = GameObject.Find("Canvas").transform.Find("Aim_Joystick").gameObject;
        ResponePanel = GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject;
        ResultPanel = GameObject.Find("Canvas").transform.Find("ResultPanel").gameObject;
        ReGameButton = ResultPanel.transform.Find("regameBTN").gameObject.GetComponent<Button>();
        ResultText = ResultPanel.transform.Find("resultText").gameObject.GetComponent<Text>();
    }

    //왜 rankingBoard 동기화해줘야되냐면 방장떠나면 다른사람이 방장되서 rankingBoard써야하니까
    //그리고 custom property는 object 형식이라 박싱언박싱일어나서 안좋은듯
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (rankingBoardsynchCoolTimer > 0f) //쿨타임마다 딕셔너리 동기화 (딕셔너리 복사작업을 프레임마다 하는건 애바인듯)
            {
                rankingBoardsynchCoolTimer -= Time.deltaTime;
                if (rankingBoardsynchCoolTimer < 0f)
                {
                    SynchRankingBoard(); //동기화해주고
                    rankingBoardsynchCoolTimer = rankingBoardsynchCoolTime; //쿨타임초기화  
                }
            }
        }
    }
    private void SynchRankingBoard()
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("SynchRankingBoardRPC", RpcTarget.AllBuffered, RankingBoard); //방장떠날때 대비 이것도 동기화해줘야됨
    }

    [PunRPC]
    private void SynchRankingBoardRPC(Dictionary<string, int> rankingBoard)
    {
        RankingBoard = rankingBoard;
    }

    public void UserJoin(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
           if(RankingBoard.ContainsKey(nickName) == false) 
                RankingBoard.Add(nickName, 0);
            UpdateRankingBoard();

            if (PhotonNetwork.LocalPlayer.NickName != nickName)
                InitGameState(nickName);
        }
    }

    private void InitGameState(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
            PV.RPC("InitGameStateRPC", RpcTarget.AllBuffered, nickName, (int)GameStateManager.Instance.NowGameState);
    }

    [PunRPC]
    private void InitGameStateRPC(string nickname, int gamestate)
    {
        if(PhotonNetwork.LocalPlayer.NickName == nickname)
        {
            GameStateManager.Instance.ChangeGameState((GameStateManager.GameState)gamestate);
        }
    }

    public void UserLeft(string nickName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (RankingBoard.ContainsKey(nickName))
                RankingBoard.Remove(nickName);
            UpdateRankingBoard();
        }
    }

    private void UpdateRankingBoard()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var sortedRankingBoard = RankingBoard.OrderByDescending(num => num.Value); //벨류값으로 내림차순
        string rankStr="";
      
        foreach (var rank in sortedRankingBoard)
        {
            rankStr += rank.Key + " : " + rank.Value + "킬\n";
        }

        PV.RPC("updateRankingTextRPC", RpcTarget.AllBuffered, rankStr);
    }

    [PunRPC]
    private void updateRankingTextRPC(string rankStr)
    {
        RangkingLogText.text = rankStr;
    }

    public void ReportTheKill(string killer, string deadPerson)
    {
        PV.RPC("killWriteRPC", RpcTarget.AllBuffered, killer, deadPerson); //마스터가 rank업데이트해야함
    }

    [PunRPC]
    private void killWriteRPC(string killer, string deadPerson)
    {
        if (killer == PhotonNetwork.LocalPlayer.NickName)
        {
            myplayer.GetComponent<PlayerHealthScript>().HealHP(0.3f);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            killLogQueue.Enqueue(new KeyValuePair<string, string>(killer, deadPerson));
            killLogOnTheScreen();

            RankingBoard[killer]++;
            UpdateRankingBoard();

            if (RankingBoard[killer] >= 5)
                OnEndGame();
        }
    }

    private void killLogOnTheScreen()
    {
        if (killLogQueue.Count() == 0 || ScreenText.text.Length != 0)
            return;
        KeyValuePair<string, string> killLogInfo = killLogQueue.Dequeue();
        PV.RPC("killLogOnTheScreenRPC", RpcTarget.AllBuffered, killLogInfo.Key, killLogInfo.Value);
        StartCoroutine(EraseScreenText(3f));
    }

    IEnumerator EraseScreenText(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        PV.RPC("SetScreenTextRPC", RpcTarget.AllBuffered,"",100);

        if (killLogQueue.Count > 0) //대기하는 애 있으면 출력
        {
            killLogOnTheScreen();
        }
    }

    [PunRPC]
    private void killLogOnTheScreenRPC(string killer, string deadPerson)
    {
        ScreenText.text = killer + "님이 " + deadPerson + "님을 처치했습니다";
    }

    [PunRPC]
    private void SetScreenTextRPC(string str, int fontSize)
    {
        ScreenText.fontSize = fontSize;
        ScreenText.text = str;
    }

    private void OnEndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameStateManager.Instance.ChangeGameStateForAllUser(GameStateManager.GameState.Result);

            //마스터는 마지막으로 rankingboard 초기화
            for (int i = 0; i < RankingBoard.Count; i++)
            {
                RankingBoard[RankingBoard.Keys.ToList()[i]] = 0;
            }
            UpdateRankingBoard();
        }
    }

}