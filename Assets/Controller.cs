using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using EasyWiFi.Core;

namespace HoloToolkit.Unity.SpatialMapping
{

    public class Controller : MonoBehaviour
    {

        private Vector3 v3Offset;
        private GameObject goFollow;
        private float speed = 0.5f;
        public bool calibrate = false;
        LineRenderer lineRenderer;
       
        GameObject HitObject;
        Vector3 HitPosition;
        GameObject SelectedObject;
        GameObject SelectedObjectgaze;
        RaycastHit hitInfo;
        float lastHitDistance=300;
        float distance = 10f;
        pointer Pointer;

        bool objectIsSelected = false;

        bool isPressed;
        bool isPressed2;
        bool iscalibrate;

        //for pichzoom
        float horizontal;
        float vertical;
        float zoomFactor;
        int numTouches;
        float lastFrameHorizontal;
        float lastFrameVertical;
        int lastFrameNumTouches;
        float lastFrameZoomFactor;

        // Use this for initialization
        void Start()
        {
            goFollow = Camera.main.gameObject;
            v3Offset = transform.position - goFollow.transform.position;
            Pointer = GetComponentInChildren<pointer>();


        }

        // Update is called once per frame
        void Update()
        {
            transform.position = goFollow.transform.position + v3Offset;
            select();
            takeOver();
        }

      


        void select()
        {
               
                
           if (Physics.Raycast(transform.position, transform.up, out hitInfo, 300.0f) && !objectIsSelected&&isPressed2)
           {
                SelectedObject = hitInfo.collider.gameObject;
                distance = Vector3.Distance(this.transform.position, SelectedObject.transform.position);
            }

            //select with gaze
            Vector3 headPosition = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 300.0f) && !isPressed2)
            {
                SelectedObjectgaze = hitInfo.collider.gameObject;
                
                rotate();
               
            }


        }

        public void click(ButtonControllerType button)
        {
            isPressed = button.BUTTON_STATE_IS_PRESSED;
            if (isPressed)
            {
                objectIsSelected = true;
            }
            else
            {
                SelectedObject = null;
                objectIsSelected = false;
            }

        }

        public void calibration(ButtonControllerType button)
        {
            iscalibrate = button.BUTTON_STATE_IS_PRESSED;
            if (iscalibrate && isPressed2)
            {
                this.calibrate = true;
            }
            else if(iscalibrate && !isPressed2)
            {
                this.calibrate = true;
            }
            else
            {
                calibrate = false;
                SelectedObjectgaze = null;
            }

        }

        public void takeOver()
        {
            if ((SelectedObject != null) && objectIsSelected && isPressed2)
            {
               SelectedObject.transform.position = transform.position + (transform.up * distance);
            }
        }

        public void rotate()
        {
            SelectedObjectgaze.transform.rotation = this.transform.rotation;
        }

        public void resize(SliderControllerType slider)
        {
            EasyWiFiConstants.AXIS sliderEffectX = EasyWiFiConstants.AXIS.XAxis;
            EasyWiFiConstants.AXIS sliderEffectY = EasyWiFiConstants.AXIS.YAxis;
            EasyWiFiConstants.AXIS sliderEffectZ = EasyWiFiConstants.AXIS.ZAxis;
            float sensitivity = .01f;
            float sliderValue = slider.SLIDER_VALUE * sensitivity;
            Vector3 actionVector3 = EasyWiFiUtilities.getControllerVector3(sliderValue, 0f, sliderEffectX, EasyWiFiConstants.AXIS.None);
            actionVector3+= EasyWiFiUtilities.getControllerVector3(sliderValue, 0f, sliderEffectY, EasyWiFiConstants.AXIS.None);
            actionVector3 += EasyWiFiUtilities.getControllerVector3(sliderValue, 0f, sliderEffectZ, EasyWiFiConstants.AXIS.None);
            if (SelectedObjectgaze!=null)
                SelectedObjectgaze.transform.localScale+= actionVector3;
        }

        public void switchOnOff(ButtonControllerType button)
        {
            isPressed2 = button.BUTTON_STATE_IS_PRESSED;
            if (isPressed2)
            {
                Pointer.drawline = true;
            }
            else
            {

                Pointer.drawline = false;
            }

        }

        public void moveForwardBackward(PinchZoomTouchpadControllerType touchpad)
        {
            Vector3 actionVector3;
            lastFrameNumTouches = numTouches;
             lastFrameZoomFactor = zoomFactor;
             lastFrameHorizontal = horizontal;
             lastFrameVertical = vertical;
             float zoomSensitivity = 4f;

             numTouches = touchpad.TOUCH_COUNT;
             zoomFactor = touchpad.ZOOM_FACTOR * zoomSensitivity;
             horizontal = touchpad.TOUCH1_POSITION_HORIZONTAL;
             vertical = touchpad.TOUCH1_POSITION_VERTICAL;
            if (numTouches > 0 && lastFrameNumTouches > 0)
            {
                if (numTouches == 2)
                {
                    //zooming in the camera
                    actionVector3 = new Vector3(0f, 0f, zoomFactor - lastFrameZoomFactor);
                    transform.Translate(actionVector3);
                }
            }
        }

    }
}
