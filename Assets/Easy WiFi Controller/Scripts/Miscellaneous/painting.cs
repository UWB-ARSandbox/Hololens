using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using EasyWiFi.Core;

[AddComponentMenu("EasyWiFiController/Miscellaneous/Painting")]
public class painting : MonoBehaviour {

    public GameObject paintObject;

    GameObject panel;

    GameObject currentObject;
    RectTransform currentRT;
    Image currentImage;

    RectTransform canvas;
    Color currentColor;

    float canvasWidth;
    float canvasHeight;

    float drawPositionX;
    float drawPositionY;

    bool isRed;
    bool isBlue;
    bool isGreen;


	// Use this for initialization
	void Start () {
        currentColor = Color.black;
        canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();
        canvasWidth = canvas.rect.width;
        canvasHeight = canvas.rect.height;

        panel = this.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        if (isRed && isBlue && isGreen)
            currentColor = Color.white;
        else if (isRed && isBlue)
            currentColor = Color.magenta;
        else if (isRed && isGreen)
            currentColor = Color.yellow;
        else if (isRed)
            currentColor = Color.red;
        else if (isBlue && isGreen)
            currentColor = Color.cyan;
        else if (isBlue)
            currentColor = Color.blue;
        else if (isGreen)
            currentColor = Color.green;
        else
            currentColor = Color.black;	

	}


    public void paint(TouchpadControllerType touchpad) 
    {
        //are we touching
        if (touchpad.IS_TOUCHING)
        {
            //values coming in are normalized put them back
            drawPositionX = Mathf.Floor(touchpad.POSITION_HORIZONTAL * canvasWidth);
            drawPositionY = Mathf.Floor(touchpad.POSITION_VERTICAL * canvasHeight);

            //create prefab paint object
            currentObject = Instantiate(paintObject);
            currentRT = currentObject.GetComponent<RectTransform>();
            currentImage = currentObject.GetComponent<Image>();

            currentRT.transform.SetParent(panel.transform, false);
            currentRT.anchoredPosition = new Vector2(drawPositionX, drawPositionY);
            currentImage.color = currentColor;
            
        }
    }

    public void setRed(ButtonControllerType redButton)
    {
        if (redButton.BUTTON_STATE_IS_PRESSED)
            isRed = true;
        else
            isRed = false;
    }

    public void setGreen(ButtonControllerType greenButton)
    {
        if (greenButton.BUTTON_STATE_IS_PRESSED)
            isGreen = true;
        else
            isGreen = false;
    }

    public void setBlue(ButtonControllerType blueButton)
    {
        if (blueButton.BUTTON_STATE_IS_PRESSED)
            isBlue = true;
        else
            isBlue = false;
    }
}
