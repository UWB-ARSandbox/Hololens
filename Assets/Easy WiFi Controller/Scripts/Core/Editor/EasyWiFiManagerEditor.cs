using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EasyWiFiManager))]
public class EasyWiFiManagerEditor : Editor
{
    private static readonly string[] _dontIncludeMeServer = new string[] { "clientConnectAutomatically" , "heartbeatTimeout", "clientMaxSendRate" };
    private static readonly string[] _dontIncludeMeClient = new string[] { "serverSendBackchannel" , "serverSendHeartbeatRate", "clientTimeout" };

    public override void OnInspectorGUI()
    {

        EasyWiFiManager myScript = target as EasyWiFiManager;

        if (myScript.peerType == EasyWiFi.Core.EasyWiFiConstants.PEER_TYPE.Server)
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, _dontIncludeMeServer);
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            //client
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, _dontIncludeMeClient);
            serializedObject.ApplyModifiedProperties();

        }
    }

}