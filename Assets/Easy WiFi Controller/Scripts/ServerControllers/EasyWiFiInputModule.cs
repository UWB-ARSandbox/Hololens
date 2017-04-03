using System;
using EasyWiFi.Core;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("EasyWiFiController/Server/Unity GUI Input Module/Easy WiFi Input Module")]
    public class EasyWiFiInputModule : BaseInputModule
    {

        protected EasyWiFiInputModule()
        { }
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.UNITY_UI_INPUT_TYPE navigationControlType = EasyWiFiConstants.UNITY_UI_INPUT_TYPE.Joystick;
        public string navigationControlName = "Joystick1";

        public EasyWiFiConstants.UNITY_UI_SELECTION_TYPE submitControlType = EasyWiFiConstants.UNITY_UI_SELECTION_TYPE.Button;
        public string submitControlName = "Button1";

        public EasyWiFiConstants.UNITY_UI_SELECTION_TYPE cancelControlType = EasyWiFiConstants.UNITY_UI_SELECTION_TYPE.Button;
        public string cancelControlName = "Button2";

        public float repeatEventRate = .25f;


        //runtime variables
        JoystickControllerType[] joystick = new JoystickControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int joystickCurrentNumberControllers = 0;
        DpadControllerType[] dpad = new DpadControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int dpadCurrentNumberControllers = 0;
        ButtonControllerType[] submitButton = new ButtonControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int submitCurrentNumberControllers = 0;
        ButtonControllerType[] cancelButton = new ButtonControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int cancelCurrentNumberControllers = 0;
        bool eventLock;
        int currentEventIndex = 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (submitButton[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                //check for client on all 3 UI navigation items
                EasyWiFiUtilities.checkForClient(submitControlName, (int)player, ref submitButton, ref submitCurrentNumberControllers);
                EasyWiFiUtilities.checkForClient(cancelControlName, (int)player, ref cancelButton, ref cancelCurrentNumberControllers);

                if (navigationControlType == EasyWiFiConstants.UNITY_UI_INPUT_TYPE.Dpad)
                {
                    EasyWiFiUtilities.checkForClient(navigationControlName, (int)player, ref dpad, ref dpadCurrentNumberControllers);
                }
                else
                {
                    EasyWiFiUtilities.checkForClient(navigationControlName, (int)player, ref joystick, ref joystickCurrentNumberControllers);
                }
            }
        }

        protected override void OnDestroy()
        {
            EasyWiFiController.On_ConnectionsChanged -= checkForNewConnections;
            base.OnDestroy();            
        }


        public override void UpdateModule()
        {
        }

        public override bool IsModuleSupported()
        {
            //if we place an easy wifi input module obviously we want to support it
            return true;
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            //if they are all still null we aren't initialized yet return false
            if (submitButton == null && cancelButton == null && joystick == null && dpad == null)
            {
                //if they are all still null after checking not ready
                return false;
            }

            //the controls aren't all null so check the actual inputs
            var shouldActivate = false;


            //iterate over the current number of connected controllers
            for (int i = 0; i < submitCurrentNumberControllers; i++)
            {
                if (submitButton[i] != null && submitButton[i].serverKey != null && submitButton[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    shouldActivate |= submitButton[i].BUTTON_STATE_IS_PRESSED;
                    currentEventIndex = i;
                }
            }

            for (int i = 0; i < cancelCurrentNumberControllers; i++)
            {
                if (cancelButton[i] != null && cancelButton[i].serverKey != null && cancelButton[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    shouldActivate |= cancelButton[i].BUTTON_STATE_IS_PRESSED;
                    currentEventIndex = i;
                }
            }

            if (navigationControlType == EasyWiFiConstants.UNITY_UI_INPUT_TYPE.Dpad)
            {
                for (int i = 0; i < dpadCurrentNumberControllers; i++)
                {
                    if (dpad[i] != null && dpad[i].serverKey != null && dpad[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                    {
                        shouldActivate |= dpad[i].DPAD_DOWN_PRESSED;
                        shouldActivate |= dpad[i].DPAD_UP_PRESSED;
                        shouldActivate |= dpad[i].DPAD_LEFT_PRESSED;
                        shouldActivate |= dpad[i].DPAD_RIGHT_PRESSED;
                        currentEventIndex = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < joystickCurrentNumberControllers; i++)
                {
                    if (joystick[i] != null && joystick[i].serverKey != null && joystick[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                    {
                        shouldActivate |= joystick[i].JOYSTICK_HORIZONTAL != 0f;
                        shouldActivate |= joystick[i].JOYSTICK_VERTICAL != 0f;
                        currentEventIndex = i;
                    }
                }
            }

            return shouldActivate;
        }

        public override void ActivateModule()
        {
            base.ActivateModule();

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        }

        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

        }

        /// <summary>
        /// Process submit keys.
        /// </summary>
        private bool SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;
            if (eventLock)
                return false;

            var data = GetBaseEventData();
            if (submitButton[currentEventIndex].BUTTON_STATE_IS_PRESSED)
            {
                eventLock = true;
                Invoke("releaseEventLock", repeatEventRate);
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
            }

            if (cancelButton[currentEventIndex].BUTTON_STATE_IS_PRESSED)
            {
                eventLock = true;
                Invoke("releaseEventLock", repeatEventRate);
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            }
            return data.used;
        }


        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            if (navigationControlType == EasyWiFiConstants.UNITY_UI_INPUT_TYPE.Dpad)
            {
                if (dpad[currentEventIndex].DPAD_LEFT_PRESSED)
                    move.x = -1f;
                if (dpad[currentEventIndex].DPAD_RIGHT_PRESSED)
                    move.x = 1f;
                if (dpad[currentEventIndex].DPAD_DOWN_PRESSED)
                    move.y = -1f;
                if (dpad[currentEventIndex].DPAD_UP_PRESSED)
                    move.y = 1f;
            }
            else
            {
                move.x = (float)Convert.ToDecimal(joystick[currentEventIndex].JOYSTICK_HORIZONTAL);
                move.y = (float)Convert.ToDecimal(joystick[currentEventIndex].JOYSTICK_VERTICAL);

                //joystick menu navigation is not exact if we are more up and down than left and right we only want the vertical
                if (move.x > move.y)
                    move.y = 0f;
                else if (move.y > move.x)
                    move.x = 0f;
            }


            if (move.x < 0)
                move.x = -1f;
            if (move.x > 0)
                move.x = 1f;

            if (move.y < 0)
                move.y = -1f;
            if (move.y > 0)
                move.y = 1f;
            return move;
        }

        /// <summary>
        /// Process move navigation (dpad or joystick)
        /// </summary>
        private bool SendMoveEventToSelectedObject()
        {
            if (eventLock)
                return false;

            Vector2 movement = GetRawMoveVector();
            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            if (!Mathf.Approximately(axisEventData.moveVector.x, 0f)
                || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
            {
                eventLock = true;
                Invoke("releaseEventLock", repeatEventRate);
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            }
            return axisEventData.used;
        }

        void releaseEventLock()
        {
            eventLock = false;
        }

        private bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            //check for client on all 3 UI navigation items
            EasyWiFiUtilities.checkForClient(submitControlName, (int)player, ref submitButton, ref submitCurrentNumberControllers);
            EasyWiFiUtilities.checkForClient(cancelControlName, (int)player, ref cancelButton, ref cancelCurrentNumberControllers);

            if (navigationControlType == EasyWiFiConstants.UNITY_UI_INPUT_TYPE.Dpad)
            {
                EasyWiFiUtilities.checkForClient(navigationControlName, (int)player, ref dpad, ref dpadCurrentNumberControllers);
            }
            else
            {
                EasyWiFiUtilities.checkForClient(navigationControlName, (int)player, ref joystick, ref joystickCurrentNumberControllers);
            }


        }


    }
}