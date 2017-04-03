using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using EasyWiFi.ClientControls;

public static class EasyWiFiControlsMenu
{
    [MenuItem("GameObject/Easy WiFi Client Controls/Dpad")]
    static void AddDPad()
    {
        GameObject panel = getCanvasPanel();

        Sprite dpadSprite = Resources.Load<Sprite>("dpadSprite");
        GameObject dPad = new GameObject("Dpad", typeof(DpadClientController), typeof(RectTransform), typeof(Image));
        RectTransform dPadRectTF = dPad.GetComponent<RectTransform>();

        dPadRectTF.sizeDelta = new Vector2(200f, 200f);
        dPad.transform.SetParent(panel.transform, false);
        dPad.GetComponent<Image>().sprite = dpadSprite;

        Selection.activeGameObject = dPad;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Joystick")]
    static void AddJoystick()
    {
        GameObject panel = getCanvasPanel();

        Sprite joystickSprite = Resources.Load<Sprite>("joystickSprite");
        GameObject joystick = new GameObject("Joystick", typeof(JoystickClientController), typeof(RectTransform), typeof(Image));
        RectTransform joystickRectTF = joystick.GetComponent<RectTransform>();

        joystickRectTF.sizeDelta = new Vector2(200f, 200f);
        joystick.transform.SetParent(panel.transform, false);
        joystick.GetComponent<Image>().sprite = joystickSprite;

        //joystick also has a child object for the nub
        Sprite joystickNubSprite = Resources.Load<Sprite>("joystickNubSprite");
        GameObject joystickNub = new GameObject("joystickNub", typeof(RectTransform), typeof(Image));
        RectTransform joystickNubRectTF = joystickNub.GetComponent<RectTransform>();
        joystickNubRectTF.sizeDelta = new Vector2(50f, 50f);
        joystickNub.GetComponent<Image>().sprite = joystickNubSprite;
        JoystickClientController joystickController = joystick.GetComponent<JoystickClientController>();
        joystickController.joystickImageForeground = joystickNub.GetComponent<Image>();
        joystickNub.transform.SetParent(joystick.transform, false);

        Selection.activeGameObject = joystick;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Button")]
    static void AddButton()
    {
        GameObject panel = getCanvasPanel();

        Sprite buttonRegularSprite = Resources.Load<Sprite>("buttonSprite");
        Sprite buttonPressedSprite = Resources.Load<Sprite>("buttonPressedSprite");
        GameObject button = new GameObject("Button", typeof(ButtonClientController), typeof(RectTransform), typeof(Image));
        RectTransform buttonRectTF = button.GetComponent<RectTransform>();

        buttonRectTF.sizeDelta = new Vector2(100f, 100f);
        button.transform.SetParent(panel.transform, false);
        button.GetComponent<Image>().sprite = buttonRegularSprite;
        button.GetComponent<ButtonClientController>().buttonPressedSprite = buttonPressedSprite;

        Selection.activeGameObject = button;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Switch")]
    static void AddSwitch()
    {
        GameObject panel = getCanvasPanel();

        Sprite buttonRegularSprite = Resources.Load<Sprite>("buttonSprite");
        Sprite buttonPressedSprite = Resources.Load<Sprite>("buttonPressedSprite");
        GameObject button = new GameObject("Switch", typeof(SwitchButtonClientController), typeof(RectTransform), typeof(Image));
        RectTransform buttonRectTF = button.GetComponent<RectTransform>();

        buttonRectTF.sizeDelta = new Vector2(100f, 100f);
        button.transform.SetParent(panel.transform, false);
        button.GetComponent<Image>().sprite = buttonRegularSprite;
        button.GetComponent<SwitchButtonClientController>().buttonPressedSprite = buttonPressedSprite;

        Selection.activeGameObject = button;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Touchpad")]
    static void AddTouchpad()
    {
        GameObject panel = getCanvasPanel();

        Sprite touchpadSprite = Resources.Load<Sprite>("touchpadSprite");
        GameObject touchpad = new GameObject("Touchpad", typeof(TouchpadClientController), typeof(RectTransform), typeof(Image));
        RectTransform touchpadRectTF = touchpad.GetComponent<RectTransform>();

        touchpadRectTF.sizeDelta = new Vector2(300f, 300f);
        touchpad.transform.SetParent(panel.transform, false);
        touchpad.GetComponent<Image>().sprite = touchpadSprite;

        Selection.activeGameObject = touchpad;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Horizontal Slider")]
    static void AddHorizontalSlider()
    {
        GameObject panel = getCanvasPanel();

        Sprite sliderSprite = Resources.Load<Sprite>("horizontal_slider_baseSprite");
        GameObject slider = new GameObject("Horizontal Slider", typeof(SliderClientController), typeof(RectTransform), typeof(Image));
        RectTransform sliderRectTF = slider.GetComponent<RectTransform>();

        sliderRectTF.sizeDelta = new Vector2(200f, 100f);
        slider.transform.SetParent(panel.transform, false);
        slider.GetComponent<Image>().sprite = sliderSprite;

        //slider also has a child object for the nub
        Sprite sliderNubSprite = Resources.Load<Sprite>("vertical_nubSprite");
        GameObject sliderNub = new GameObject("SliderNub", typeof(RectTransform), typeof(Image));
        RectTransform sliderNubRectTF = sliderNub.GetComponent<RectTransform>();
        sliderNubRectTF.sizeDelta = new Vector2(50f, 50f);
        sliderNub.GetComponent<Image>().sprite = sliderNubSprite;
        SliderClientController sliderController = slider.GetComponent<SliderClientController>();
        sliderController.orientation = EasyWiFi.Core.EasyWiFiConstants.SLIDER_TYPE.Horizonal;
        sliderController.sliderImageForeground = sliderNub.GetComponent<Image>();
        sliderNub.transform.SetParent(slider.transform, false);

        Selection.activeGameObject = slider;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Vertical Slider")]
    static void AddVerticalSlider()
    {
        GameObject panel = getCanvasPanel();

        Sprite sliderSprite = Resources.Load<Sprite>("vertical_slider_baseSprite");
        GameObject slider = new GameObject("Vertical Slider", typeof(SliderClientController), typeof(RectTransform), typeof(Image));
        RectTransform sliderRectTF = slider.GetComponent<RectTransform>();

        sliderRectTF.sizeDelta = new Vector2(100f, 200f);
        slider.transform.SetParent(panel.transform, false);
        slider.GetComponent<Image>().sprite = sliderSprite;

        //slider also has a child object for the nub
        Sprite sliderNubSprite = Resources.Load<Sprite>("horizontal_nubSprite");
        GameObject sliderNub = new GameObject("SliderNub", typeof(RectTransform), typeof(Image));
        RectTransform sliderNubRectTF = sliderNub.GetComponent<RectTransform>();
        sliderNubRectTF.sizeDelta = new Vector2(50f, 50f);
        sliderNub.GetComponent<Image>().sprite = sliderNubSprite;
        SliderClientController sliderController = slider.GetComponent<SliderClientController>();
        sliderController.orientation = EasyWiFi.Core.EasyWiFiConstants.SLIDER_TYPE.Vertical;
        sliderController.sliderImageForeground = sliderNub.GetComponent<Image>();
        sliderNub.transform.SetParent(slider.transform, false);

        Selection.activeGameObject = slider;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/PinchZoom Touchpad")]
    static void AddPinchZoomTouchpad()
    {
        GameObject panel = getCanvasPanel();

        Sprite touchpadSprite = Resources.Load<Sprite>("touchpadSprite");
        GameObject touchpad = new GameObject("PinchZoom Touchpad", typeof(PinchZoomTouchpadClientController), typeof(RectTransform), typeof(Image));
        RectTransform touchpadRectTF = touchpad.GetComponent<RectTransform>();

        touchpadRectTF.sizeDelta = new Vector2(300f, 300f);
        touchpad.transform.SetParent(panel.transform, false);
        touchpad.GetComponent<Image>().sprite = touchpadSprite;

        Selection.activeGameObject = touchpad;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Accelerometer")]
    static void AddAccelerometer()
    {
        GameObject panel = getCanvasPanel();

        GameObject accelerometer = new GameObject("Accelerometer", typeof(AccelerometerClientController), typeof(RectTransform));

        accelerometer.transform.SetParent(panel.transform, false);
        Selection.activeGameObject = accelerometer;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Gyro")]
    static void AddGyro()
    {
        GameObject panel = getCanvasPanel();

        GameObject gyro = new GameObject("Gyro", typeof(GyroClientController), typeof(RectTransform));

        gyro.transform.SetParent(panel.transform, false);
        Selection.activeGameObject = gyro;
    }

    [MenuItem("GameObject/Easy WiFi Client Controls/Connection Widget")]
    static void AddConnectionWidget()
    {
        GameObject panel = getCanvasPanel();

        Sprite connectionWidgetSprite = Resources.Load<Sprite>("mobileSprite");
        GameObject connectionWidget = new GameObject("Connection Widget", typeof(ConnectionClientController), typeof(RectTransform), typeof(Image));
        RectTransform connectionWidgetRectTF = connectionWidget.GetComponent<RectTransform>();

        connectionWidgetRectTF.sizeDelta = new Vector2(256f, 128f);
        connectionWidget.transform.SetParent(panel.transform, false);
        connectionWidget.GetComponent<Image>().sprite = connectionWidgetSprite;
        ConnectionClientController clientConnect = connectionWidget.GetComponent<ConnectionClientController>();

        //wifi child
        Sprite wifiSprite = Resources.Load<Sprite>("wifiSprite");
        GameObject wifi = new GameObject("WiFi", typeof(RectTransform), typeof(Image));
        RectTransform wifiRectTF = wifi.GetComponent<RectTransform>();
        wifiRectTF.sizeDelta = new Vector2(100f, 50f);
        wifiRectTF.localPosition = new Vector3(1f, 32f, 0f);
        wifi.transform.SetParent(connectionWidget.transform, false);
        wifi.GetComponent<Image>().sprite = wifiSprite;        
        clientConnect.wifiImage = wifi.GetComponent<Image>();

        //connect child
        Sprite connectSprite = Resources.Load<Sprite>("connectSprite");
        GameObject connect = new GameObject("Connect", typeof(RectTransform), typeof(Image));
        RectTransform connectRectTF = connect.GetComponent<RectTransform>();
        connectRectTF.sizeDelta = new Vector2(80f, 30f);
        connectRectTF.localPosition = new Vector3(-70f, -25f, 0f);
        connect.transform.SetParent(connectionWidget.transform, false);
        connect.GetComponent<Image>().sprite = connectSprite;
        clientConnect.connectImage = connect.GetComponent<Image>();

        //disconnect child
        Sprite disconnectSprite = Resources.Load<Sprite>("disconnectSprite");
        GameObject disconnect = new GameObject("Disconnect", typeof(RectTransform), typeof(Image));
        RectTransform disconnectRectTF = disconnect.GetComponent<RectTransform>();
        disconnectRectTF.sizeDelta = new Vector2(100f, 30f);
        disconnectRectTF.localPosition = new Vector3(79f, -25f, 0f);
        disconnect.transform.SetParent(connectionWidget.transform, false);
        disconnect.GetComponent<Image>().sprite = disconnectSprite;
        clientConnect.disconnectImage = disconnect.GetComponent<Image>();


        Selection.activeGameObject = connectionWidget;
    } 

    //our client controls need to be children of our own canvas
    static GameObject getCanvasPanel()
    {
        GameObject panel = GameObject.Find("ClientControlPanel");
        if (panel == null)
        {
            panel = createCanvasPanel();
        }

        return panel;
    }


    // if a client control canvas wasn't present create a new one
    static GameObject createCanvasPanel()
    {
        GameObject canvas = new GameObject("ClientControlCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;

        Sprite panelSprite = Resources.Load<Sprite>("clientControllerBackgroundSprite");
        GameObject panel = new GameObject("ClientControlPanel", typeof(RectTransform), typeof(Image));
        RectTransform panelRectTF = panel.GetComponent<RectTransform>();
        panelRectTF.anchorMax = new Vector2(1f, 1f);
        panelRectTF.anchorMin = new Vector2(0f, 0f);
        panelRectTF.sizeDelta = new Vector2(0f, 0f);
        panel.GetComponent<Image>().sprite = panelSprite;
        panel.GetComponent<Image>().type = Image.Type.Filled;


        panel.transform.SetParent(canvas.transform, false);

        return panel;

    }

}
