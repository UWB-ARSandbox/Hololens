using UnityEngine;
using System.Collections;
using EasyWiFi.ServerControls;
using EasyWiFi.Core;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("EasyWiFiController/Miscellaneous/ControllerConnected")]
public class ControllerConnected : MonoBehaviour {

    public GameObject playerBadgeObject;
    public GameObject p1Area;
    public GameObject p2Area;
    public GameObject p3Area;
    public GameObject p4Area;
    public GameObject selectionPanel;
    public alphaBlink startGameButton;

    //runtime variables
    GameObject[] instanceArray = new GameObject[4];
    bool[] playerLockArray = new bool[4];
    int[] playerMap = new int[4];
    Vector2[] midpoints = new Vector2[4];
    int controllersConnected = 0;
    int playersReady = 0;
    bool justInstantiated = false;
    


    //variables to encapsulate parameters so method can be called from main thread
    bool messageWaiting = false;
    int messagePlayerNumber = -1;
    bool messageIsConnect = false;

    void OnEnable()
    {
        EasyWiFiController.On_ConnectionsChanged += spawnNewPlayerBadge;
    }

    void OnDestroy()
    {
        EasyWiFiController.On_ConnectionsChanged -= spawnNewPlayerBadge;
    }

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            playerMap[i] = -1; //this means no player has selected this area
        }

        RectTransform temp;

        temp = p1Area.transform.GetComponent<RectTransform>();
        midpoints[0] = new Vector2(temp.position.x, temp.position.y);

        temp = p2Area.transform.GetComponent<RectTransform>();
        midpoints[1] = new Vector2(temp.position.x, temp.position.y);

        temp = p3Area.transform.GetComponent<RectTransform>();
        midpoints[2] = new Vector2(temp.position.x, temp.position.y);

        temp = p4Area.transform.GetComponent<RectTransform>();
        midpoints[3] = new Vector2(temp.position.x, temp.position.y);

    }

    void Update()
    {
        if (justInstantiated)
            justInstantiated = false;
        else if (messageWaiting)
            spawnBadge();
    }

    public bool assignPlayerIndex(int current, int playerAreaIndex)
    {
        if (playerAreaIndex >= 0 && playerAreaIndex <= 3)
        {
            if (playerLockArray[playerAreaIndex] != true)
            {
                //set the slot as occupied fix the badge there and disable the dpad
                playerLockArray[playerAreaIndex] = true;
                playerMap[current] = playerAreaIndex;
                playersReady++;

                if (playersReady > 0 && playersReady == controllersConnected)
                {
                    startGameButton.startBlink();
                }
                return true;
            }
        }
        return false;

    }

    public void unAssignPlayerIndex(int currentPlayer, int playerAreaIndex)
    {
        if (playerAreaIndex >= 0 && playerAreaIndex <= 3)
        {
            if (playerLockArray[playerAreaIndex] == true)
            {
                //set the slot as not occupied set the badge free and reenable the dpad
                playerLockArray[playerAreaIndex] = false;
                playerMap[currentPlayer] = -1;
                playersReady--;

                if (playersReady < controllersConnected)
                {
                    startGameButton.endBlink();
                }
            }
        }

    }

    public void startGameScene()
    {
        //only if everyone is ready can we start the game
        if (playersReady < controllersConnected)
        {
            return;
        }

        //go through and reshuffle all the controllers to their new player number
        //you would normall just do this by calling swap player number but iterating through the collection
        //4 times would sometimes lead to conflict (already changed one and then change two sets to a third)
        //so you have to do the whole map at once and iterate through the collection only once
        BaseControllerType control;

        if (EasyWiFiController.controllerDataDictionary != null)
        {
            foreach (string key in EasyWiFiController.controllerDataDictionary.Keys)
            {
                control = EasyWiFiController.controllerDataDictionary[key];
                //serverside the key will also have the IP of the client so we need to lookup additional one time
                if (control.logicalPlayerNumber == 0)
                {
                    control.logicalPlayerNumber = playerMap[0];
                }
                else if (control.logicalPlayerNumber == 1)
                {
                    control.logicalPlayerNumber = playerMap[1];
                }
                else if (control.logicalPlayerNumber == 2)
                {
                    control.logicalPlayerNumber = playerMap[2];
                }
                else if (control.logicalPlayerNumber == 3)
                {
                    control.logicalPlayerNumber = playerMap[3];
                }
            }
        }
        //start the game scene
        Application.LoadLevel("ControllerSelectServerScenePart2");
    }

    //apparently because the callback is being called from the listener thread we can't instantiate the prefab from within
    //the method. So instead I've just set a bool from that thread and we'll do the instantiate the next frame around
    //which is on the main thread
    void spawnBadge()
    {
        //check to make sure this wasn't a disconnection
        if (messageIsConnect)
        {
            if (!justInstantiated)
            {
                GameObject temp;
                StandardJoystickServerController joystick;
                CustomButtonServerController[] buttons;
                SelectPlayer sp;
                Text text;

                justInstantiated = true;

                temp = Instantiate(playerBadgeObject);
                temp.transform.SetParent(selectionPanel.transform, false);
                sp = temp.GetComponent<SelectPlayer>();
                sp.p1Center = midpoints[0];
                sp.p2Center = midpoints[1];
                sp.p3Center = midpoints[2];
                sp.p4Center = midpoints[3];
                sp.cc = this;
                sp.currentLogicalPlayer = messagePlayerNumber;
                text = temp.GetComponentInChildren<Text>();
                text.text = "P" + messagePlayerNumber.ToString();
                joystick = temp.GetComponent<StandardJoystickServerController>();
                joystick.player = (EasyWiFiConstants.PLAYER_NUMBER)messagePlayerNumber;
                joystick.checkForNewConnections(true, messagePlayerNumber);
                buttons = temp.GetComponents<CustomButtonServerController>();
                foreach (CustomButtonServerController button in buttons)
                {
                    button.player = (EasyWiFiConstants.PLAYER_NUMBER)messagePlayerNumber;
                    button.checkForNewConnections(true, messagePlayerNumber);
                }
                instanceArray[messagePlayerNumber] = temp;
                controllersConnected++;

                if (playersReady < controllersConnected)
                {
                    startGameButton.endBlink();
                }
            }
        }
        else
        {
            //it actually was a disconnect


            //we need to look through the array on disconnect and remove the player if he had already picked
            for (int i = 0; i < 4; i++)
            {
                if (playerMap[i] == messagePlayerNumber)
                {
                    playerLockArray[i] = false;
                    playerMap[i] = -1;
                    playersReady--;
                }
                        
            }

            Destroy(instanceArray[messagePlayerNumber].gameObject);
            instanceArray[messagePlayerNumber] = null;
            controllersConnected--;

            if (playersReady == 0 || playersReady < controllersConnected)
            {
                startGameButton.endBlink();
            }

            if (playersReady > 0 && playersReady == controllersConnected)
            {
                startGameButton.startBlink();
            }
        }

        //reset the instance variables
        messageWaiting = false;
        messageIsConnect = false;
        messagePlayerNumber = -1;
    }

    public void spawnNewPlayerBadge(bool isConnect, int playerNumber)
    {
        if (playerNumber < 0 || playerNumber > 3)
        {
            return;
        }

        messageIsConnect = isConnect;
        messagePlayerNumber = playerNumber;
        messageWaiting = true;

        
    }
}
