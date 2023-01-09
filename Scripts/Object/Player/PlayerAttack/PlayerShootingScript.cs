using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerShootingScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    private playerScript player;
    private GameObject gunObject;
    private Animator characterAnim;
    private Animator gunAni;
    private SpriteRenderer spriteRender;

    private readonly float attackCoolTime = 0.25f;
    private float attackCoolTimer = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        PV = GetComponent<PhotonView>();
        spriteRender = GetComponent<SpriteRenderer>();
        gunObject = transform.Find("gun").gameObject;

        if (PV.IsMine)
        {
            player = GetComponent<playerScript>();
            gunAni = gunObject.GetComponent<Animator>();
            characterAnim = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (PV.IsMine)
        {
            CheckAndRunAttackCoolTime();
        }
        if (Input.GetKeyDown(KeyCode.Space)) Attack();
    }

    public void Attack()
    {
        if (player.isActive == false) return;
        if (attackCoolTimer > 0f) return;

        SoundManager.Instance.PlayShootingSound();
        OnAttackCoolTime(attackCoolTime);
        onAttackAnimation();
        PV.RPC("ShootRPC", RpcTarget.AllBuffered, PhotonNetwork.NickName);
    }

    [PunRPC]
    private void ShootRPC(string name)
    {
        var bullet = ObjectPool.GetObject(name);

        bullet.transform.position = gunObject.transform.position + new Vector3(spriteRender.flipX ? -0.4f : 0.4f, -0.11f, 0);
        bullet.transform.rotation = gunObject.GetComponent<Transform>().rotation;
    }

    private void OnAttackCoolTime(float attackCoolTime)
    {
        attackCoolTimer = attackCoolTime;
    }

    private void onAttackAnimation()
    {
        gunAni.SetBool("isShot", true);
        characterAnim.SetTrigger("shot");
    }

    private void CheckAndRunAttackCoolTime()
    {
        if (attackCoolTimer > 0)
        {
            attackCoolTimer -= Time.deltaTime;

            if (attackCoolTimer <= 0)
            {
                attackCoolTimer = 0;
                gunAni.SetBool("isShot", false);
            }
        }
    }
}
