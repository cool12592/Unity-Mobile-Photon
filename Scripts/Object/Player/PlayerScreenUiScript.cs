using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerScreenUiScript : MonoBehaviourPunCallbacks
{
    public GameObject moveJoystick { get; private set; }
    public GameObject aimJoystick { get; private set; }
    private Button attackButton;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            moveJoystick = GameObject.Find("Canvas").transform.Find("Move_Joystick").gameObject;
            moveJoystick.SetActive(true);
            moveJoystick.GetComponent<JoyStickScript>().MyPlayer = gameObject;


            aimJoystick = GameObject.Find("Canvas").transform.Find("Aim_Joystick").gameObject;
            aimJoystick.GetComponent<JoyStickScript>().MyPlayer = gameObject;


            attackButton = GameObject.Find("Canvas").transform.Find("Aim_Joystick").transform.Find("attack_BTN").gameObject.GetComponent<Button>();
            attackButton.onClick.AddListener(GetComponent<PlayerShootingScript>().Attack);
        }
    }
}
