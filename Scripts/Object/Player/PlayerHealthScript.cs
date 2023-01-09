using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHealthScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV;
    [SerializeField]
    private Image healthImage;
    private Rigidbody2D rigidBody;
    private Animator characterAnim;
    private playerScript player;

    private readonly float damagedCoolTime = 0.35f;
    private float damagedCoolTimer = 0f;
    private bool invincibility = false;
    private string recentAttacker;
    
    private enum ColorList{Original,DamagedColor };

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        if (PV.IsMine)
        {
            rigidBody = gameObject.GetComponent<Rigidbody2D>();
            characterAnim = GetComponent<Animator>();
            player = GetComponent<playerScript>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            RunDamagedCoolTime();
        }
        if (Input.GetKeyDown(KeyCode.T)) TakeDamage(PhotonNetwork.NickName);

    }

    public void OnInvincibility()
    {
        invincibility = true;
    }
    public void OffInvincibility()
    {
        invincibility = false;
    }

    public void HealHP(float recoverHP)
    {
        healthImage.fillAmount += recoverHP;
        if (healthImage.fillAmount > 1f)
            healthImage.fillAmount = 1f;
    }

    public void TakeDamage(string enemyName)
    {
        if (invincibility) return;

        recentAttacker = enemyName;
        PV.RPC("ChangeColorRPC", RpcTarget.AllBuffered, (int)ColorList.DamagedColor);
        OnDamagedCoolTime(damagedCoolTime);
        ReducedHP(0.1f);
    }

    [PunRPC]
    private void ChangeColorRPC(int color)
    {
        switch ((ColorList)color)
        {
            case ColorList.Original:
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                break;
            case ColorList.DamagedColor:
                GetComponent<SpriteRenderer>().color = new Color(1f, 0.1f, 0.1f, 0.5f);
                break;
            default:
                break;
        }

    }


    private void OnDamagedCoolTime(float damagedCoolTime)
    {
        damagedCoolTimer = damagedCoolTime;
    }

    private void RunDamagedCoolTime()
    {
        if (damagedCoolTimer > 0f)
        {
            damagedCoolTimer -= Time.deltaTime;

            if (damagedCoolTimer <= 0)
            {
                damagedCoolTimer = 0;
                PV.RPC("ChangeColorRPC", RpcTarget.AllBuffered, (int)ColorList.Original);
            }
        }
    }

    private void ReducedHP(float num)
    {
        healthImage.fillAmount -= num;

        if (healthImage.fillAmount <= 0 && player.isActive)
        {
            Death();
        }
    }

    private void Death()
    {
        player.isActive = false;
        rigidBody.velocity = Vector2.zero;
        GameManager.Instance.ReportTheKill(recentAttacker, PhotonNetwork.NickName);
        characterAnim.SetTrigger("death");
    }

    //death애니메이션끝에이벤트달아놈
    public void LateDeath()
    {
        if (PV.IsMine)
        {
            GameManager.Instance.ResponePanel.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
    }
    [PunRPC]
    private void DestroyRPC() => Destroy(gameObject);

    //변수 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(healthImage.fillAmount);
        }
        else
        {
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
