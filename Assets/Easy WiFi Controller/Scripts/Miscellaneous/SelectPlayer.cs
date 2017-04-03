using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using UnityEngine.UI;

[AddComponentMenu("EasyWiFiController/Miscellaneous/SelectPlayer")]
public class SelectPlayer : MonoBehaviour
{

    [HideInInspector]
    public Vector2 p1Center;
    [HideInInspector]
    public Vector2 p2Center;
    [HideInInspector]
    public Vector2 p3Center;
    [HideInInspector]
    public Vector2 p4Center;
    [HideInInspector]
    public ControllerConnected cc;
    [HideInInspector]
    public int currentLogicalPlayer;

    Vector3 lockedPosition;
    bool isLocked = false;
    int lockedArea = -1;
    RectTransform myRT;
    Image sprite;

    void Start()
    {
        myRT = this.GetComponent<RectTransform>();
        sprite = this.GetComponent<Image>();
    }

    void Update()
    {
        if (isLocked)
        {
            transform.position = lockedPosition;
        }


    }


    public void assignPlayer(ButtonControllerType button)
    {
        if (button.BUTTON_STATE_IS_PRESSED)
        {
            if (!isLocked)
            {
                bool result = false;

                //check to see if the position is in one of the player areas
                if (myRT.position.x >= (p1Center.x - 100) &&
                    myRT.position.x <= (p1Center.x + 100) &&
                    myRT.position.y >= (p1Center.y - 100) &&
                    myRT.position.y <= (p1Center.y + 100))
                {
                    //in player 1 area
                    lockedArea = 0;
                    result = cc.assignPlayerIndex(currentLogicalPlayer, lockedArea);
                }
                else if (myRT.position.x >= (p2Center.x - 100) &&
                    myRT.position.x <= (p2Center.x + 100) &&
                    myRT.position.y >= (p2Center.y - 100) &&
                    myRT.position.y <= (p2Center.y + 100))
                {
                    //in player 2 area
                    lockedArea = 1;
                    result = cc.assignPlayerIndex(currentLogicalPlayer, lockedArea);
                }
                else if (myRT.position.x >= (p3Center.x - 100) &&
                    myRT.position.x <= (p3Center.x + 100) &&
                    myRT.position.y >= (p3Center.y - 100) &&
                    myRT.position.y <= (p3Center.y + 100))
                {
                    //in player 3 area
                    lockedArea = 2;
                    result = cc.assignPlayerIndex(currentLogicalPlayer, lockedArea);
                }
                else if (myRT.position.x >= (p4Center.x - 100) &&
                    myRT.position.x <= (p4Center.x + 100) &&
                    myRT.position.y >= (p4Center.y - 100) &&
                    myRT.position.y <= (p4Center.y + 100))
                {
                    //in player 4 area
                    lockedArea = 3;
                    result = cc.assignPlayerIndex(currentLogicalPlayer, lockedArea);
                }

                //check to see if we've successfully locked
                if (result == true)
                {
                    isLocked = true;
                    sprite.color = Color.gray;
                    lockedPosition = this.transform.position;
                }
                else
                {
                    //unsuccessful lock (means occupied by another already so reset locked area
                    lockedArea = -1;
                }
            }
        }

    }

    public void unAssignPlayer(ButtonControllerType button)
    {
        if (button.BUTTON_STATE_IS_PRESSED)
        {
            if (isLocked)
            {
                cc.unAssignPlayerIndex(currentLogicalPlayer, lockedArea);
                isLocked = false;
                sprite.color = Color.white;
                lockedPosition = Vector3.zero;
                lockedArea = -1;
            }

        }
    }

    public void startGame(ButtonControllerType button)
    {
        if (button.BUTTON_STATE_IS_PRESSED)
        {
            if (isLocked)
            {
                //we can only vote to start the game if we've already chosen
                cc.startGameScene();
            }

        }
    }
}
