using UnityEngine;
using System.Collections;
using System;
using EasyWiFi.Core;

[AddComponentMenu("EasyWiFiController/Manager/EasyWiFiManager")]
public class EasyWiFiManager : MonoBehaviour {

    public int serverSocketPort = 2015;
    public int clientSocketPort = 2016;
    public EasyWiFiConstants.PEER_TYPE peerType = EasyWiFiConstants.PEER_TYPE.Server;
    [Tooltip("Amount of time in seconds that if the server hasn't recieved any data from a particular client will consider it a timeout and mark it disconnected. 0 is disabled")]
    [Range(0f, 100f)]
    public float clientTimeout = 0f;
    [Tooltip("Only set true if you have some data that you're communicating back from your game to your phone controller")]
    public bool serverSendBackchannel = false;
    [Tooltip("Rate at which you want your server to send heartbeats back to your controller in seconds. 0 is disabled")]
    [Range(0f,100f)]
    public float serverSendHeartbeatRate = 0f;
    public bool clientConnectAutomatically = true;
    public EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE clientMaxSendRate = EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.CapAt90Hz;
    [Tooltip("Amount of time in seconds that the controller checks for timeout. If the check fails twice in a row then the controller will disconnect. 0 is disabled")]
    [Range(0f, 100f)]
    public float heartbeatTimeout = 0f;
    public bool logVerbose = false;
    public string applicationName = "EasyWiFiController";

    bool transmittedInventory = false;

    int packetNumber = 0;
    int heartbeatPacketNumber = 0;
    int consecutiveAttempts = 0;
    float lastSendTime = 0f;
    float maxRate = 0f;

    // Use this for initialization
    void Awake()
    {
        //check to see if we already have an existing EasyWiFiManager from another scene
        //if we do destroy this one
        foreach (var gameObj in FindObjectsOfType(typeof(EasyWiFiManager)))
        {
            if (gameObj.GetInstanceID() == this.GetInstanceID())
            {
                //this is the current gameobject ignore
            }
            else
            {
                //another one must have been present from another scene so destroy the one we're currently in since the other one has active connections
                DestroyImmediate(this.gameObject);
                return;
            }
        }


        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (peerType == EasyWiFiConstants.PEER_TYPE.Server)
        {
            EasyWiFiController.initialize(applicationName, EasyWiFiConstants.PEERTYPE_SERVER, serverSocketPort,clientSocketPort,logVerbose, clientConnectAutomatically);
        }
        else 
        {
            EasyWiFiController.initialize(applicationName, EasyWiFiConstants.PEERTYPE_CLIENT, serverSocketPort, clientSocketPort,logVerbose, clientConnectAutomatically);
        }

        DontDestroyOnLoad(transform.gameObject);

    }

    void Start()
    {
        

        if (peerType == EasyWiFiConstants.PEER_TYPE.Client)
        {
            lastSendTime = Time.realtimeSinceStartup;
            
            //set the rate
            switch (clientMaxSendRate)
            {
                case EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.CapAt30Hz:
                    maxRate = .033333333f;
                    break;
                case EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.CapAt60Hz:
                    maxRate = .016666666f;
                    break;
                case EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.CapAt90Hz:
                    maxRate = .011111111f;
                    break;
                case EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.SendAsFastAsPossible:
                    maxRate = 0f;
                    break;
                case EasyWiFiConstants.CONTROLLERDATA_MAX_SEND_RATE.SendInfrequentlyOncePerSecond:
                    maxRate = 1f;
                    break;
                default:
                    maxRate = 0f;
                    break;

            }


            //There can be a timing issue between when the server has been discovered but we might not be ready yet
            //to transmit our inventory.  Becuase this script executes in start and registrations in awake all the local registrations have now take place
            EasyWiFiController.readyToTransmitInventory = true;

            if (!transmittedInventory && (EasyWiFiController.clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting && EasyWiFiController.clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected))
            {
                //depending on the order in which client and server are started check our state and send inventory immediately
                EasyWiFiController.sendControllerInventory(EasyWiFiController.createInventoryMessage());
                EasyWiFiController.clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingInventory;
            }

            if (heartbeatTimeout > 0f)
            {
                InvokeRepeating("checkHeartbeatForTimeout", heartbeatTimeout, heartbeatTimeout);
            }
        }
        else
        {
            if (serverSendBackchannel)
            {
                EasyWiFiController.readyToTransmitBackchannel = true;
            }
            if (serverSendHeartbeatRate > 0f)
            {
                InvokeRepeating("sendServerHeartbeat", serverSendHeartbeatRate, serverSendHeartbeatRate);
            }
            if (clientTimeout > 0f)
            {
                InvokeRepeating("checkForClientTimeout", clientTimeout, clientTimeout);
            }
        }
    }


    void LateUpdate() 
    {
        if (peerType == EasyWiFiConstants.PEER_TYPE.Client)
        {
            //we are the client we are responsible for triggering the send of Easy WiFi Controllers data structure over
            //the network and because of this we want to order this script to run after the others

            if (EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
            {
                //send the data
                if ((Time.realtimeSinceStartup - lastSendTime) > maxRate)
                {
                    lastSendTime = Time.realtimeSinceStartup;
                    EasyWiFiController.sendWiFIControllerData(EasyWiFiController.createControllerDataMessage(packetNumber));

                    //increment the packet number
                    if (packetNumber < EasyWiFiConstants.ROLLOVER_PACKET_NUMBER)
                        packetNumber++;
                    else
                        packetNumber = 0;
                }
            }


        }
        else 
        {
            //send the backchannel (if present)
            if (serverSendBackchannel)
            {

                //server backchannels are for feeding back UI elements to the controller and are less sensitive
                //like in the racing example it animates the spedometer
                //so we cap this at 30 times per second
                if ((Time.realtimeSinceStartup - lastSendTime) > .033333333f)
                {
                    lastSendTime = Time.realtimeSinceStartup;
                    EasyWiFiController.createAndSendBackchannelMessages(packetNumber);

                    //increment the packet number
                    if (packetNumber < EasyWiFiConstants.ROLLOVER_PACKET_NUMBER)
                        packetNumber++;
                    else
                        packetNumber = 0;
                }
            }
        }
    }


    void OnApplicationQuit()
    {
        EasyWiFiController.endUDPClientAndThread();
    }

    void sendServerHeartbeat()
    {
        EasyWiFiController.createAndSendHeartbeatMessages(heartbeatPacketNumber);

        //increment the packet number
        if (heartbeatPacketNumber < EasyWiFiConstants.ROLLOVER_PACKET_NUMBER)
            heartbeatPacketNumber++;
        else
            heartbeatPacketNumber = 0;
    }

    void checkHeartbeatForTimeout()
    {
        if (EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
        {

            if ((DateTime.UtcNow - EasyWiFiController.lastHeartbeatTime).TotalSeconds > Convert.ToDouble(heartbeatTimeout))
            {
                consecutiveAttempts++;
                //we have passed the heartbeat timeout disconnect and go back to discovery mode
                if (consecutiveAttempts > 1)
                {
                    //disconnect

                    if (logVerbose)
                    {
                        Debug.Log("Heartbeat timeout. Going Back to Discovery Mode");
                    }
                    EasyWiFiController.clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected;
                    consecutiveAttempts = 0;

                    //take this time to set all the client side controls ready to reconnect
                    BaseControllerType temp;
                    if (EasyWiFiController.controllerDataDictionary != null)
                    {
                        foreach (string key in EasyWiFiController.controllerDataDictionary.Keys)
                        {
                            temp = EasyWiFiController.controllerDataDictionary[key];
                            temp.justReconnected = true;
                        }
                    }

                    //go back to discovery mode and check for a new server
                    EasyWiFiController.checkForServer();
                }
            }
            
        }
    }

    void checkForClientTimeout()
    {
        //check for any clients that may have timed out (if we just had a reconnection skip this time around
        if (!(EasyWiFiController.isConnect == true
        && (DateTime.UtcNow - EasyWiFiController.lastCallbackTime).TotalSeconds < clientTimeout
        && (DateTime.UtcNow - EasyWiFiController.lastCallbackTime).TotalSeconds > 0d))
        {
            EasyWiFiUtilities.checkForClientTimeout(clientTimeout);
        }
    }
}
