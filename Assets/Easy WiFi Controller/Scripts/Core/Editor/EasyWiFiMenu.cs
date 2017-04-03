using UnityEngine;
using UnityEditor;
using System.Collections;

public static class EasyWiFiMenu
{

    [MenuItem("GameObject/Easy WiFi Manager/Add EasyWiFiManager")]
    static void AddEasyWiFiManager()
    {

        if (GameObject.FindObjectOfType<EasyWiFiManager>() == null)
        {
            GameObject wifimanager;
            MonoScript script;
            wifimanager = new GameObject("EasyWiFiManager", typeof(EasyWiFiManager));
            script = MonoScript.FromMonoBehaviour(wifimanager.GetComponent<EasyWiFiManager>());
            MonoImporter.SetExecutionOrder(script, -100);
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "EasyWiFiManager already exists in your scene", "OK");
        }

        Selection.activeObject = GameObject.FindObjectOfType<EasyWiFiManager>().gameObject;
    }

}