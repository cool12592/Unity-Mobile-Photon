using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.Collections; // NativeArray

public class playerScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Text NickNameText;
    public bool isActive = true;

    // Start is called before the first frame update
    private void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName.ToString() : PV.Owner.NickName.ToString();
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        if (PV.IsMine)
        {   
            GameManager.Instance.myplayer = gameObject;
            GameObject.Find("ObjectPoolParent").transform.GetChild(0).gameObject.SetActive(true);
        }
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
            GetComponent<PlayerMovementScript>().DashInit();
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
