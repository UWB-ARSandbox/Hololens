using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using EasyWiFi.Core;
using UnityEngine.UI;

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
        GameObject Loading;
        CanvasRenderer image;

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

        //hovertime
         float HoverTime;
        float waitOverTime=2f;
        float waitAmount;
         Image circularSilder;

        // Use this for initialization
        void Start()
        {
            goFollow = Camera.main.gameObject;
            v3Offset = transform.position - goFollow.transform.position;
            Pointer = GetComponentInChildren<pointer>();
            HoverTime = Time.time;
            Loading = GameObject.Find("Canvas");
            circularSilder = Loading.GetComponentInChildren<Image>();
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = goFollow.transform.position + v3Offset;
            select();
            takeOver();
            rotate();
            Debug.Log(SelectedObjectgaze);
            if (SelectedObjectgaze == null)
                circularSilder.fillAmount = waitAmount;
            else
                circularSilder.fillAmount = 0f;
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
           
           
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 300.0f) && !isPressed2 )
            {
                waitAmount= (Time.time - HoverTime) / waitOverTime;
                if (Loading != null)
                    Loading.transform.position = hitInfo.point-new Vector3(0,0,0.5f);
                if (Time.time>=HoverTime+waitOverTime)
                {
                    SelectedObjectgaze = hitInfo.collider.gameObject;
                    Debug.Log("ObjectSelected");
                    HoverTime = Time.time;
                    
                }
               
                
            }
            else { HoverTime = Time.time;
                SelectedObjectgaze = null;
                if (Loading != null)
                 Loading.transform.position = Camera.main.transform.position; }
                

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
            if(SelectedObjectgaze!=null)
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
            EasyWiFiConstants.AXIS touchpadHorizontal = EasyWiFiConstants.AXIS.XAxis;
        EasyWiFiConstants.AXIS touchpadVertical = EasyWiFiConstants.AXIS.YAxis;
        lastFrameNumTouches = numTouches;
             lastFrameZoomFactor = zoomFactor;
             lastFrameHorizontal = horizontal;
             lastFrameVertical = vertical;
             float zoomSensitivity = 4f;
              float sensitivity = 2f;
            numTouches = touchpad.TOUCH_COUNT;
             zoomFactor = touchpad.ZOOM_FACTOR * zoomSensitivity;
             horizontal = touchpad.TOUCH1_POSITION_HORIZONTAL* sensitivity;
             vertical = touchpad.TOUCH1_POSITION_VERTICAL* sensitivity;
            if (numTouches > 0 && lastFrameNumTouches > 0)
            {
                if (numTouches == 1 && SelectedObjectgaze != null)
                {
                    actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal - lastFrameHorizontal, vertical - lastFrameVertical, touchpadHorizontal, touchpadVertical);
                    SelectedObjectgaze.transform.position += actionVector3;
                }
                if (numTouches == 2&& SelectedObjectgaze!=null)
                {
                    
                     SelectedObjectgaze.transform.position = SelectedObjectgaze.transform.position + Camera.main.transform.forward * (zoomFactor - lastFrameZoomFactor) ;
                }
            }
        }

    }
}
