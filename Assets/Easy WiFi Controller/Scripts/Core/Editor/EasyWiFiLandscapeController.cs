using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
public static class EasyWiFiLandscapeController
{

    static EasyWiFiLandscapeController()
    {
        EditorApplication.hierarchyWindowChanged += OnHierarchyChange;
    }
    static void OnHierarchyChange()
    {

        if (EditorApplication.currentScene.Contains("MultiplayerDynamicClientScene") ||
            EditorApplication.currentScene.Contains("ControlsKitchenSinkClientScene") ||
            EditorApplication.currentScene.Contains("DrawingClientScene") ||
            EditorApplication.currentScene.Contains("UnityUINavigationClientScene") ||
            EditorApplication.currentScene.Contains("PanTiltZoomClientScene") ||
            EditorApplication.currentScene.Contains("DualStickZoomClientScene") ||
            EditorApplication.currentScene.Contains("PrecomputedSteeringClientScene") ||
            EditorApplication.currentScene.Contains("MultiplayerControllerSelectClientScene") ||            
            EditorApplication.currentScene.Contains("SteeringWheelClientScene"))
        {
            //we only need to execute once on our scenes
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            EditorApplication.hierarchyWindowChanged -= OnHierarchyChange;
        }

    }
}