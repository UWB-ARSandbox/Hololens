using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using EasyWiFi.Core;
using UnityEngine.UI;



    public class Controller : MonoBehaviour
    {

        private Vector3 v3Offset;
        private GameObject goFollow;
        private float speed = 0.5f;
        public bool calibrate = false;
        LineRenderer lineRenderer;
        float tapTime ;
        GameObject HitObject;
        Vector3 HitPosition;
        GameObject SelectedObject;
        GameObject SelectedObjectgaze;
        RaycastHit hitInfo;
        float lastHitDistance=300;
        public float distance = 10f;
        pointer Pointer;
        GameObject Loading;
        GameObject frame;

        CanvasRenderer image;

        bool objectIsSelected = false;
        bool touchTakeOver = false;
        public bool isPressed = false;
        bool isPressed2;
        bool iscalibrate=false;
        int mode = 0;
        bool hit=false;
        //for pichzoom
        float horizontal; float horizontal2;
        float vertical; float vertical2;
        float zoomFactor; float zoomFactor2;
        int numTouches; int numTouches2;
        float lastFrameHorizontal; float lastFrameHorizontal2;
        float lastFrameVertical; float lastFrameVertical2;
        int lastFrameNumTouches; int lastFrameNumTouches2;
        float lastFrameZoomFactor; float lastFrameZoomFactor2;
     

        string Source;
        public int modeHand=0;
    

    //hovertime
    float HoverTime;
        float waitOverTime=2f;
        float waitAmount=1f;
         Image circularSilder;
        AnchorTextAbove Echo;

        Vector3 headPosition;
        Vector3 gazeDirection ;

        public Shader outlineShader;
        public Shader WireFrame;
        Shader standardShader;
        private GameObject highlightedObject = null;

        public GameObject Arrow;

        // Use this for initialization
        void Start()
        {
            goFollow = Camera.main.gameObject;
            v3Offset = transform.position - goFollow.transform.position;
            Pointer = GetComponentInChildren<pointer>();
            HoverTime = Time.time;
            Loading = GameObject.Find("Canvas");
            frame = GameObject.Find("Frame");
        Arrow = GameObject.Find("Arrow");
            tapTime = Time.time;
            circularSilder = Loading.GetComponentInChildren<Image>();
           
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = goFollow.transform.position + v3Offset;
            select();
            takeOver();
           
            //##GAZE Selection counter###
            // Debug.Log(SelectedObjectgaze);
            /*
            if (SelectedObjectgaze == null)
                circularSilder.fillAmount = waitAmount;
            else
                circularSilder.fillAmount = 0f;
                */
            if (Echo != null)
            {
                
                if (mode == 0)
                    Echo.Text = "translate";
                else if (mode == 1)
                    Echo.Text = "rotate";
                else if (mode == 2)
                    Echo.Text = "rescale";
            }
        
       
            if(highlightedObject!=null && SelectedObject!=null && Arrow!=null)
             {
                Arrow.transform.position = SelectedObject.transform.position;
            Arrow.transform.rotation = SelectedObject.transform.rotation;
            Vector3 newVect = SelectedObject.transform.localScale * 3;
            newVect.x = Mathf.Clamp(newVect.x, 0.1f,2 );
            newVect.y = Mathf.Clamp(newVect.y, 0.1f,2 );
            newVect.z = Mathf.Clamp(newVect.z, 0.1f, 2);
            Arrow.transform.localScale = newVect;

        }
        else
            {
                Arrow.transform.position = Camera.main.transform.position;
                Arrow.transform.localScale = new Vector3(0, 0, 0);
            }
        }

      


        void select()
        {
            //select with gaze

            headPosition = Camera.main.transform.position;
            gazeDirection = Camera.main.transform.forward;
           
         
            if (Physics.Raycast(transform.position, transform.up, out hitInfo, 300.0f,1) && !objectIsSelected)
           {
                SelectedObject = hitInfo.collider.gameObject;
               
                Source = "controller";
                distance = Vector3.Distance(this.transform.position, SelectedObject.transform.position);
                if (Loading != null)
                    Loading.transform.position = hitInfo.point - new Vector3(0, 0, 0.005f);
            }
            
            else if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 300.0f,1) && !objectIsSelected)
            {
                //##GAZE Selection counter###
                /* waitAmount= (Time.time - HoverTime) / waitOverTime;
                 if (Loading != null)
                     Loading.transform.position = hitInfo.point-new Vector3(0,0,0.5f);

                 if (Time.time>=HoverTime+waitOverTime)
                 {
                     SelectedObjectgaze = hitInfo.collider.gameObject;
                     Debug.Log("ObjectSelected");
                     HoverTime = Time.time;

                 }*/

                SelectedObject = hitInfo.collider.gameObject;
               
               
                Source = "camera";
                distance = Vector3.Distance(Camera.main.transform.position, SelectedObject.transform.position);
                if (Loading != null)
                    Loading.transform.position = hitInfo.point - new Vector3(0, 0, 0.005f);
            }
            else
            {
                Loading.transform.position = Camera.main.transform.position;
                frame.transform.position= Camera.main.transform.position;
            frame.transform.localScale = new Vector3(0, 0, 0);

        }
            //##GAZE Selection counter###
            /*
            else { HoverTime = Time.time;
                SelectedObjectgaze = null;
                if (Loading != null)
                 Loading.transform.position = Camera.main.transform.position; }*/


        }

        public void selection(ButtonControllerType button)
        {
            isPressed = button.BUTTON_STATE_IS_PRESSED;
            if (isPressed)
            {
                objectIsSelected = true;
                if (SelectedObject != null)
                {
                    highlightObject(SelectedObject);
                    Echo = SelectedObject.GetComponentInChildren<AnchorTextAbove>();
                    SelectedObject.GetComponent<Rigidbody>().isKinematic = true;
                }

            }
            else
            {
                if (SelectedObject != null)
                {
                    SelectedObject.GetComponent<Rigidbody>().isKinematic = false;
                }
                if (Echo != null)
                    Echo.Text = "";
                Echo = null;
              
                SelectedObject = null;
                objectIsSelected = false;
                mode = 0;
                Pointer.drawline = true;
                removeHighlight();

            }

        }

        public void calibration(ButtonControllerType button)
        {
            iscalibrate = button.BUTTON_STATE_IS_PRESSED;
            if (iscalibrate && mode != 1)
            {
                calibrate = true;
                Debug.Log("calibrate");
            }else if (iscalibrate&&mode == 1)
            {
                rotate();
            }
    
            else
            {
                calibrate = false;
                       
            }

        }

        public void changeMode(ButtonControllerType button)
        {
            isPressed2 = button.BUTTON_STATE_IS_PRESSED;
            if (isPressed2)
            {
                mode++;
                if (mode > 2)
                    mode = 0;
                Debug.Log(mode);
            }
            if(mode==0)
                Pointer.drawline = true;
            else
                Pointer.drawline = false;



        }

    public void changeTranslate()
    {
        modeHand = 0;
    }
    public void changeRotate()
    {
        modeHand = 1;
    }
    public void changeScale()
    {
        modeHand = 2;
    }

    public void takeOver()
        {
            Vector3 multiply = new Vector3(1.1f, 1.1f, 1.1f);  
            if ((SelectedObject != null) && objectIsSelected && mode==0 &&!touchTakeOver)
            {

                if (Source.Equals("controller") && Physics.Raycast(transform.position, transform.up, 300f,1))
                {
                    SelectedObject.transform.position = transform.position + (transform.up * distance);
                    //SelectedObject.transform.LookAt(transform.position);
                }
                else if (Source.Equals("camera") && Physics.Raycast(transform.position, transform.up, 300f,1))
                {
                    SelectedObject.transform.position = transform.position + ((Camera.main.transform.forward + transform.up) * (distance / 2f));
                    Pointer.drawline = false;
                }
                else
                {
                    Pointer.drawline = true;
                }
                    
            }else if((SelectedObject != null) && objectIsSelected && mode == 1 && !touchTakeOver)
            {

                //do projection
                frame.GetComponent<MeshFilter>().mesh = SelectedObject.GetComponent<MeshFilter>().mesh;
                frame.transform.position = SelectedObject.transform.position;
                frame.transform.localScale = SelectedObject.transform.localScale * 1.2f;
                frame.transform.rotation = this.transform.rotation;
                
            }

        }
       
        public void rotate()
        {
            if(SelectedObject!=null)
                 SelectedObject.transform.rotation = this.transform.rotation;
            
        }

      

        public void Touchpad(PinchZoomTouchpadControllerType touchpad)
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
                
                if (numTouches == 1 )
                {
                    actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal - lastFrameHorizontal, vertical - lastFrameVertical, touchpadHorizontal, touchpadVertical);

                    touchTakeOver = true;
                    /*translate=0*/
                    if (mode==0 && SelectedObject != null)
                    {
                        //distance += actionVector3.y;
                        SelectedObject.transform.position += actionVector3;
                    }
                    /*rotate=1*/
                    else if (mode==1 && SelectedObject != null)
                    {
                        Vector3 newVect = new Vector3(actionVector3.y, -actionVector3.x, actionVector3.z);
                        SelectedObject.transform.Rotate(newVect * 50f, Space.World);
                    }
                    /*scale=2*/
                    else if (mode==2 && SelectedObject != null)
                    {
                        float average = (actionVector3.x + actionVector3.y) / 2f;
                        Vector3 newVect = new Vector3(SelectedObject.transform.localScale.x, SelectedObject.transform.localScale.y, SelectedObject.transform.localScale.z);
                        newVect += new Vector3 (average, average, average);
                        newVect.x = Mathf.Clamp(newVect.x, 0.1f, 10);
                        newVect.y = Mathf.Clamp(newVect.y, 0.1f, 10);
                        newVect.z = Mathf.Clamp(newVect.z, 0.1f, 10);
                        SelectedObject.transform.localScale = newVect;
                    }
                    
                    
                }
                       
            }
            else
            {
                touchTakeOver = false;
            }
           
        }

        public void Touchpad2(PinchZoomTouchpadControllerType touchpad)
        {
            Vector3 actionVector3;
            EasyWiFiConstants.AXIS touchpadHorizontal = EasyWiFiConstants.AXIS.XAxis;
            EasyWiFiConstants.AXIS touchpadVertical = EasyWiFiConstants.AXIS.YAxis;
            lastFrameNumTouches2 = numTouches2;
            lastFrameZoomFactor2 = zoomFactor2;
            lastFrameHorizontal2 = horizontal2;
            lastFrameVertical2 = vertical2;
            float zoomSensitivity = 4f;
            float sensitivity = 1f;
           


            numTouches2 = touchpad.TOUCH_COUNT;
            zoomFactor2 = touchpad.ZOOM_FACTOR * zoomSensitivity;
            horizontal2 = touchpad.TOUCH1_POSITION_HORIZONTAL * sensitivity;
            vertical2 = touchpad.TOUCH1_POSITION_VERTICAL * sensitivity;

            if (numTouches2 > 0 && lastFrameNumTouches2 > 0)
            {

                if (numTouches2 == 1)
                {
                    actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal2 - lastFrameHorizontal2, vertical2 - lastFrameVertical2, touchpadHorizontal, touchpadVertical);

                    
                    /*translate=0*/
                    if (mode == 0 && SelectedObject != null)
                    {
                        distance += actionVector3.x*2f;
                        
                    }
                    /*rotate=1*/
                    else if (mode == 1 && SelectedObject != null)
                    {
                        Vector3 newVect = new Vector3(0, 0, -actionVector3.x);
                        SelectedObject.transform.Rotate(newVect * 50f, Space.World);
                    }
                   


                }
               

            }
           

        }

        public void highlightObject(GameObject obj)
        {
            // Outline this object using the shader
            if (obj.name == "Frame")
                return;
            highlightedObject = obj;
            standardShader = obj.GetComponent<Renderer>().material.shader;
        highlightedObject.GetComponent<Renderer>().material.shader = outlineShader;
            float outlineWidth = .01f * obj.GetComponent<Renderer>().bounds.size.magnitude;
            highlightedObject.GetComponent<Renderer>().material.SetFloat("_Outline", outlineWidth);
            
        }

        public void removeHighlight()
        {
            if (highlightedObject != null)
            {
                Destroy(highlightedObject.GetComponent<LineRenderer>());
                // Remove the object's shader outline
                highlightedObject.GetComponent<Renderer>().material.shader = standardShader;
                highlightedObject = null;
            }
        }
    }

   

