using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Custom Button")]
    public class CustomButtonServerController : MonoBehaviour, IServerController
    {

        public string control = "Button1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public string notifyMethod = "yourMethod";
        [Tooltip("Determines when your Notify Method gets called")]
        public EasyWiFiConstants.CALL_TYPE callType = EasyWiFiConstants.CALL_TYPE.Every_Frame;


        //runtime variables
        ButtonControllerType[] button = new ButtonControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        bool lastValue = false;


        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (button[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref button, ref currentNumberControllers);
            }
        }

        void OnDestroy()
        {
            EasyWiFiController.On_ConnectionsChanged -= checkForNewConnections;
        }

        // Update is called once per frame
        void Update()
        {
            //iterate over the current number of connected controllers
            for (int i = 0; i < currentNumberControllers; i++)
            {
                if (button[i] != null && button[i].serverKey != null && button[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }                    
            }
            

        }


        public void mapDataStructureToAction(int index)
        {
            if (callType == EasyWiFiConstants.CALL_TYPE.Every_Frame)
                SendMessage(notifyMethod, button[index],SendMessageOptions.DontRequireReceiver);
            else
            {
                if (lastValue != button[index].BUTTON_STATE_IS_PRESSED)
                {
                    SendMessage(notifyMethod, button[index], SendMessageOptions.DontRequireReceiver);
                }
                lastValue = button[index].BUTTON_STATE_IS_PRESSED;
            }
        }

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            EasyWiFiUtilities.checkForClient(control, (int)player, ref button, ref currentNumberControllers);
        }
    }

}
