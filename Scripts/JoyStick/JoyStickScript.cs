using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class JoyStickScript : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;

    [SerializeField]
    private float multiplier = 2.7f;

    [SerializeField,Range(10,150)]
    private float leverRange;

    private Vector2 inputDirection;
    private bool isInput = false;

    public GameObject MyPlayer;

    public enum JoystickType { Move,Aim};
    public JoystickType joystickType;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        controlJoyStickLever(eventData);
        isInput = true;
    }

    //드래그해서 마우스 멈추고있는동안은 이벤트안됨 그래서 isInput써서 update 에다 해야됨
    public void OnDrag(PointerEventData eventData)
    {
        controlJoyStickLever(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;
        isInput = false;

        if (MyPlayer)
        {
            if(joystickType==JoystickType.Move)
               MyPlayer.GetComponent<PlayerMovementScript>().Move(Vector2.zero);
        }

    }

    private void controlJoyStickLever(PointerEventData eventData)
    {
        var inputPos = eventData.position - rectTransform.anchoredPosition; 

        if(joystickType == JoystickType.Aim)
            inputPos = eventData.position - new Vector2(Screen.width, 0f) - rectTransform.anchoredPosition;

        var inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;
        lever.anchoredPosition = inputVector/ multiplier; 
        inputDirection = inputVector / leverRange;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInput)
            inputControlVector();
    }

    private void inputControlVector()
    {
        if (MyPlayer)
        {
            if (joystickType == JoystickType.Move)
            {
                MyPlayer.GetComponent<PlayerMovementScript>().Move(inputDirection);
            }
            else if (joystickType == JoystickType.Aim)
            {
                MyPlayer.GetComponent<PlayerShootingScript>().Attack();

                if (inputDirection != null)
                MyPlayer.GetComponent<PlayerGunAndAimScript>().AimMove(inputDirection);
            }
        }
    }
}
