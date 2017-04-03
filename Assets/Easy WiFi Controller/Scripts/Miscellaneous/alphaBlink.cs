using UnityEngine;
using UnityEngine.UI;
using System.Collections;
[AddComponentMenu("EasyWiFiController/Miscellaneous/alphaBlink")]
public class alphaBlink : MonoBehaviour {

    Image myImage;
    Text myText;
    Color currentImageColor;
    Color currentTextColor;
    bool ascending = true;

	// Use this for initialization
	void Start () {
        myText = this.GetComponentInChildren<Text>();
        myImage = this.GetComponent<Image>();
        currentImageColor = myImage.color;
        currentTextColor = myText.color;
    }
	

    public void startBlink()
    {
        InvokeRepeating("animateImage", .05f, .05f);
    }

    public void endBlink()
    {
        CancelInvoke("animateImage");
        currentImageColor.a = 0f;
        currentTextColor.a = 0f;
        myImage.color = currentImageColor;
        myText.color = currentTextColor;
    }

    public void animateImage()
    {
        currentImageColor = myImage.color;
        if (currentImageColor.a > .95)
            ascending = false;
        else if (currentImageColor.a < .05)
            ascending = true;

        if (ascending)
        {
            currentImageColor.a += .05f;
            currentTextColor.a += .05f;
        }
        else
        {
            currentImageColor.a -= .05f;
            currentTextColor.a -= .05f;
        }

        myImage.color = currentImageColor;
        myText.color = currentTextColor;
    }
}
