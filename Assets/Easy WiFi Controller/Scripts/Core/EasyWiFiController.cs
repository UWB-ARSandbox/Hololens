using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
#endif

//General architecture of EasyWiFIController (discovery part) is as follows
//Client will broadcast it's address UDP broadcast in the discovery phase
//Server upon recieving broadcast will reply to client letting it to know to stop broadcasting and giving client a client number
//Client will then send back an inventory of its controls/backchannels
//Server will send back the server key for each of the controls/backchannels
//client will then start sending controller data at specified rate
//(optional) server will then start sending backchannel data at specified rate

//NOTE: Obviously it would be a more straightforward architecture if the server broadcast (not the client) but apparently
//      some mobile phones block receiving broadcasts (not all and there are ways around but doesn't seem to work reliably)
//      and mobile phones are likely to be the client

//This class contains all the sockets/communication details below is a message cheatsheet

//Client Broadcast ->   "Client_Broadcast:(ClientIP)"
//Server Broadcast Response -> "Broadcast_ServerResponse:(ServerIP)"
//Client Inventory -> "Client_Inventory:(ClientIP):controltype#controlname,controltype#controlname,controltype#controlname,...etc" (can be one or many)
//Server Inventory Response -> "Inventory_ServerResponse:serverkey#controlname,serverkey#controlname,serverkey#controlname,...etc" (one for each sent in inventory)
//Client Controller data (endlines in data stream) -> "Client_ControllerData:(packet#):
//(serverkey)#datapoint,datapoint,datapoint,...etc
//(serverkey)#datapoint,datapoint,datapoint,...etc
//(serverkey)#datapoint,datapoint,datapoint,...etc" (one for each client control and each control may have one or many datapoints)
//Server Backchannel data (endlines in data stream) -> "Server_BackchannelData:(packet#):
//(clientkey)#datapoint,datapoint, ...etc
//(clientkey)#datapoint,datapoint, ...etc (one for each server backchannel)
//Server Heartbeat -> "Server_Heartbeat:(packet#)"
//Client Log -> "Client_Log:(logmessage)"
//Client Disconnect -> "Client_Disconnect:(ClientIP)"

namespace EasyWiFi.Core
{

    public static class EasyWiFiController
    {
        public static string peerType;
        public static string serverIPAddress;
        public static string myIPAddress;
        public static string appName;
        public static bool isVerbose;
        public static int serverSocketListenPort;
        public static int clientScoketListenPort;
      

        public delegate void controllerConnectionsChangedHandler(bool isConnect, int playerNumber);

        public static event controllerConnectionsChangedHandler On_ConnectionsChanged;

        public static Dictionary<string,BaseControllerType> controllerDataDictionary;

        //client only
        public static EasyWiFiConstants.CURRENT_CLIENT_STATE clientState;
        public static bool readyToTransmitInventory; //this is different that the state of network traffic, this is if locally all client controls have registered
        public static bool readyToTransmitBackchannel;
        public static DateTime lastHeartbeatTime;


#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1|| UNITY_WSA_8_1)
        public static DatagramSocket clientBroadcast;
        public static DatagramSocket clientSend;
        public static DatagramSocket clientListen;
        public static HostName clientBroadcastEndPoint;
        public static HostName clientSendEndPoint;
        public static HostName clientListenEndPoint;
#else
        public static UdpClient clientBroadcast;
        public static UdpClient clientSend;
        public static UdpClient clientListen;
        public static Thread listenThread;
        public static Thread broadcastThread;
        public static IPEndPoint clientBroadcastEndPoint;
        public static IPEndPoint clientSendEndPoint;
        public static IPEndPoint clientListenEndPoint;
#endif


        //server only
        public static EasyWiFiConstants.CURRENT_SERVER_STATE serverState;

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        public static DatagramSocket serverListen;
        public static Dictionary<string, DatagramSocket> serverSendDictionary;
         public static Dictionary<string, int> serverSendEndPointDictionary;
        public static HostName serverListenEndPoint;
#else
        public static UdpClient serverListen;
        public static Dictionary<string, UdpClient> serverSendDictionary;
        public static Dictionary<string, IPEndPoint> serverSendEndPointDictionary;
        public static IPEndPoint serverListenEndPoint;
#endif


        public static List<string> serverSendKeys;
        public static int lastConnectedPlayerNumber = EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED;
        public static bool isConnect = false;
        public static bool isNew = false;
        public static DateTime lastCallbackTime;


        static EasyWiFiController()
        {
            peerType = string.Empty;
            serverIPAddress = string.Empty;
            appName = string.Empty;
            
        }



#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        async public static void initialize(string name, string type, int serverPort, int clientPort, bool verbose, bool clientConnectAutomatically)
        {
            appName = name;
            peerType = type;
            serverSocketListenPort = serverPort;
            clientScoketListenPort = clientPort;
            isVerbose = verbose;
          

             myIPAddress = getNetworkIPAddress();

            controllerDataDictionary = new Dictionary<string, BaseControllerType>();
            serverSendKeys = new List<string>();

            if (isVerbose)
            {
                Debug.Log("Initializing Easy WiFi Controller...");
            }


            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER))
            {

             serverIPAddress = getNetworkIPAddress();

                serverSendDictionary = new Dictionary<string, DatagramSocket>();

                serverSendEndPointDictionary = new Dictionary<string, int>();

                //setup for server listening (setup for server send happens later)

                serverListen = new DatagramSocket();

                //Task taskA = Task.Run( () => {listen();});
                serverListen.MessageReceived +=listen;
               // serverListenEndPoint = new IPEndPoint(IPAddress.Any, serverSocketListenPort);
                serverListenEndPoint = new HostName("0.0.0.0");
                try
                {
                     await serverListen.BindEndpointAsync(null, serverSocketListenPort.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }
                

                serverState = EasyWiFiConstants.CURRENT_SERVER_STATE.Listening;

            }
            else if (peerType.Equals(EasyWiFiConstants.PEERTYPE_CLIENT))
            {
                //if we're initing with connect automatically this will be changed below when checking for server
                clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected;

                //setup for client broadcast (setup for client send to server happens later)

                clientListen = new DatagramSocket();
                 clientListen.MessageReceived +=listen;
        
                
                clientListenEndPoint = new HostName("0.0.0.0");

        

                clientBroadcast = new DatagramSocket();

                clientBroadcastEndPoint = new HostName("255.255.255.255");
                try
                {
        
                     await clientListen.BindEndpointAsync(null, clientScoketListenPort.ToString());
                       
                }
                catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }
       
        //Task taskA = Task.Run( () => {listen();});
       
                if (clientConnectAutomatically)
                {
                    checkForServer();
                }

            }
        }
#else
        public static void initialize(string name, string type, int serverPort, int clientPort, bool verbose, bool clientConnectAutomatically)
        {
            appName = name;
            peerType = type;
            serverSocketListenPort = serverPort;
            clientScoketListenPort = clientPort;
            isVerbose = verbose;
           
            myIPAddress = Network.player.ipAddress;

            controllerDataDictionary = new Dictionary<string, BaseControllerType>();
            serverSendKeys = new List<string>();

            if (isVerbose)
            {
                Debug.Log("Initializing Easy WiFi Controller...");
            }

           
            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER))
            {

                serverIPAddress = Network.player.ipAddress;
                serverSendDictionary = new Dictionary<string, UdpClient>();
                serverSendEndPointDictionary = new Dictionary<string, IPEndPoint>();

                //setup for server listening (setup for server send happens later)

                serverListen = new UdpClient()
                {
                    EnableBroadcast = true
                };

                serverListen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                serverListenEndPoint = new IPEndPoint(IPAddress.Any, serverSocketListenPort);
                serverListen.Client.Bind(serverListenEndPoint);
                listenThread = new Thread(new ThreadStart(listen))
                {
                    IsBackground = true
                };
                listenThread.Start();

                serverState = EasyWiFiConstants.CURRENT_SERVER_STATE.Listening;

            }
            else if (peerType.Equals(EasyWiFiConstants.PEERTYPE_CLIENT))
            {
                //if we're initing with connect automatically this will be changed below when checking for server
                clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected;

                //setup for client broadcast (setup for client send to server happens later)
                clientBroadcast = new UdpClient()
                {
                    EnableBroadcast = true
                };

                clientBroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, serverSocketListenPort);

                //setup for client listening
                clientListen = new UdpClient()
                {
                    EnableBroadcast = true
                };


                clientListen.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                clientListenEndPoint = new IPEndPoint(IPAddress.Any, clientScoketListenPort);
                clientListen.Client.Bind(clientListenEndPoint);
                listenThread = new Thread(new ThreadStart(listen))
                {
                    IsBackground = true
                };
                listenThread.Start();

                if (clientConnectAutomatically)
                {
                    checkForServer();
                }

            }
        }
#endif

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //this method is called upon init of the client automatically (if set to) to look for the server
        //this can also be invoked in gamecode to essentially relook for server (if we want to reconnect or find a new one)
         public static void checkForServer()
        {

            //reset the connection variables
            clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting;
            serverIPAddress = String.Empty;


            Task taskA = Task.Run( () => {broadcastLoop();});

        }
#else
        public static void checkForServer()
        {

           //reset the connection variables
            clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting;
            serverIPAddress = String.Empty;


            broadcastThread = new Thread(new ThreadStart(broadcastLoop))
            {
                IsBackground = true
            };
            broadcastThread.Start();

        }
#endif

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        public async static void broadcastLoop()
        {
            byte[] bytes;
            while (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
            {
                //broadcast until we know who the server is
                if (isVerbose)
                {
                    sendClientLog(createClientLogMessage("Sending out client broadcast..."));
                }
                bytes = Encoding.UTF8.GetBytes(EasyWiFiConstants.MESSAGETYPE_BROADCAST + EasyWiFiConstants.SPLITMESSAGE_COLON + myIPAddress);
                try{
                    using (var stream = await clientBroadcast.GetOutputStreamAsync(clientBroadcastEndPoint, serverSocketListenPort.ToString()))
                    {
                        
                        using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                        {
                             
                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                         
                        }
                     }
                }
                catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }
                
                Task.Delay(3000);
            }
        }
#else
        public static void broadcastLoop()
        {
            byte[] bytes;
            while (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
            {
                //broadcast until we know who the server is
                if (isVerbose)
                {
                    sendClientLog(createClientLogMessage("Sending out client broadcast..."));
                }
                bytes = Encoding.UTF8.GetBytes(EasyWiFiConstants.MESSAGETYPE_BROADCAST + EasyWiFiConstants.SPLITMESSAGE_COLON + myIPAddress);

                try
                {
                    clientBroadcast.Send(bytes, (int)bytes.Length, clientBroadcastEndPoint);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }
                Thread.Sleep(3000);
            }
        }
#endif



#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        public async static void listen(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs eventArguments)
        {
             byte[] numArray;
            string message;

            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER) && isVerbose)
            {
                Debug.Log("listening for communication...");
            }
            
           
                try
                {
                    if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER))
                    {

                        uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                        message = eventArguments.GetDataReader().ReadString(stringLength);

                        if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CLIENTLOG))
                        {
                            //client log message
                            listenClientLogMessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_BROADCAST))
                        {
                            //client broadcast message
                            listenClientBroadcast(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CLIENT_INVENTORY))
                        {
                            //client inventory message
                            listenClientInventoryMessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CONTROLLER_DATA))
                        {
                            //client controller data
                            listenClientControllerData(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_DISCONNECT))
                        {
                            listenClientDisconnect(message);
                        }



                    }
                    else if (peerType.Equals(EasyWiFiConstants.PEERTYPE_CLIENT))
                    {
                       
                        
                      
                        uint stringLength = eventArguments.GetDataReader().UnconsumedBufferLength;
                        message = eventArguments.GetDataReader().ReadString(stringLength);

                        if (message.Contains(EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_BROADCAST))
                        {
                            //server broadcast response message
                            listenServerBroadcastResponse(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_INVENTORY))
                        {
                            //server inventory response message
                            listenServerInventoryResponse(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_BACKCHANNEL_DATA))
                        {
                            //server backchannel message
                            listenServerBackchannelmessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_HEARTBEAT))
                        {
                            //server heartbeat message
                            listenServerHeartbeatmessage(message);
                        }

                    }

                }
                catch (Exception exception)
                {
                    //closing a connection during a blocking receive will generate an exception
                    //we'll break out of the loop when we counter this exception
                    if (isVerbose)
                    {
                        Debug.Log("Normal socket listening interrupted to exit. Source:" + exception.Source);
                    }
                                   
                }
            
        }
#else
        public static void listen()
        {
            byte[] numArray;
            string message;

            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER) && isVerbose)
            {
                Debug.Log("listening for communication...");
            }

            while (true)
            {
                try
                {
                    if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER))
                    {

                        numArray = EasyWiFiController.serverListen.Receive(ref serverListenEndPoint);

                        message = Encoding.UTF8.GetString(numArray, 0, numArray.Length);

                        if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CLIENTLOG))
                        {
                            //client log message
                            listenClientLogMessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_BROADCAST))
                        {
                            //client broadcast message
                            listenClientBroadcast(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CLIENT_INVENTORY))
                        {
                            //client inventory message
                            listenClientInventoryMessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_CONTROLLER_DATA))
                        {
                            //client controller data
                            listenClientControllerData(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_DISCONNECT))
                        {
                            listenClientDisconnect(message);
                        }



                    }
                    else if (peerType.Equals(EasyWiFiConstants.PEERTYPE_CLIENT))
                    {

                        numArray = EasyWiFiController.clientListen.Receive(ref clientListenEndPoint);

                        message = Encoding.UTF8.GetString(numArray, 0, numArray.Length);


                        if (message.Contains(EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_BROADCAST))
                        {
                            //server broadcast response message
                            listenServerBroadcastResponse(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_INVENTORY))
                        {
                            //server inventory response message
                            listenServerInventoryResponse(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_BACKCHANNEL_DATA))
                        {
                            //server backchannel message
                            listenServerBackchannelmessage(message);
                        }
                        else if (message.Contains(EasyWiFiConstants.MESSAGETYPE_HEARTBEAT))
                        {
                            //server heartbeat message
                            listenServerHeartbeatmessage(message);
                        }

                    }

                }
                catch (Exception exception)
                {
                    //closing a connection during a blocking receive will generate an exception
                    //we'll break out of the loop when we counter this exception
                    if (isVerbose)
                    {
                        Debug.Log("Normal socket listening interrupted to exit. Source:" + exception.Source);
                    }
                    break;
                }
            }
        }
#endif    

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the client to tell the server we are disconnecting
        async public  static void sendDisconnect(string message)
        {
            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending Disconnect message... " + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting && clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {
            try{
                using (var stream = await clientSend.GetOutputStreamAsync(clientSendEndPoint, serverSocketListenPort.ToString()))
                    {
                    using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                    {
                       
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                         
                    }
                 }
              }catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }

                clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected;
            }
        }
#else
        //used by the client to tell the server we are disconnecting
        public static void sendDisconnect(string message)
        {
            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending Disconnect message... " + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting && clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {

                try
                { 
                    //if we're broadcasting we don't yet know the server
                    clientSend.Send(bytes, (int)bytes.Length, clientSendEndPoint);
                    
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

                clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected;
            }
        }
#endif

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the client to send controller data to the server
        async public static void sendClientLog(string message)
        {
            Debug.Log(message);
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting && clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {
                
                try{
                     
        using (var stream = await clientSend.GetOutputStreamAsync(clientSendEndPoint, serverSocketListenPort.ToString()))
                    {
                        using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                        {
                            
                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                            
                        }
                     }
                 }catch (Exception e)
                    {
                        Debug.Log("here"+e.ToString());
                        Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                         return;
                    }


            }
        }
#else
        //used by the client to send controller data to the server
        public static void sendClientLog(string message)
        {
            Debug.Log(message);
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting && clientState != EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {

                try
                {
                    //if we're broadcasting we don't yet know the server
                    clientSend.Send(bytes, (int)bytes.Length, clientSendEndPoint);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

            }
        }
#endif


#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the client to send controller data to the server
        async public static void sendWiFIControllerData(string message)
        {

            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending controller data... " + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
                try{
                   using (var stream = await clientSend.GetOutputStreamAsync(clientSendEndPoint, serverSocketListenPort.ToString()))
                    {
                            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                            {
                       
                                writer.WriteBytes(bytes);
                                await writer.StoreAsync();
                                 
                            }
                         }
                }catch (Exception e)
                {
                    Debug.Log("here sending  backchannel data"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }

        }
#else
        //used by the client to send controller data to the server
        public static void sendWiFIControllerData(string message)
        {

            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending controller data... " + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                clientSend.Send(bytes, (int)bytes.Length, clientSendEndPoint);
            }
            catch (Exception ex)
            {
                //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                //log the error and move on (try again later)
                Debug.Log(ex.Message);
            }

        }
#endif


#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the server to send our backchannel data
        async public static void sendWiFIBackchannelData(string message, string clientIP)
        {

            if (isVerbose)
            {
                Debug.Log("Sending out server backchannel data... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {
                try{
                  using (var stream = await serverSendDictionary[clientIP].GetOutputStreamAsync(new Windows.Networking.HostName(clientIP), serverSendEndPointDictionary[clientIP].ToString()))
                    {
                        using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                        {
                       
                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                             
                        }
                     }
                }catch (Exception e)
                {
                    Debug.Log("here sending  backchannel data"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }

            }
        }
#else
        //used by the server to send our backchannel data
        public static void sendWiFIBackchannelData(string message, string clientIP)
        {

            if (isVerbose)
            {
                Debug.Log("Sending out server backchannel data... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {

                try
                {
                    serverSendDictionary[clientIP].Send(bytes, (int)bytes.Length, serverSendEndPointDictionary[clientIP]);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

            }
        }
#endif


#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the server to send our heartbeat
        async public static void sendHeartbeat(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server heartbeat... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {
            try
             {
                using (var stream = await serverSendDictionary[clientIP].GetOutputStreamAsync(new Windows.Networking.HostName(clientIP), serverSendEndPointDictionary[clientIP].ToString()))
                    {
                    using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                    {
                       
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                         
                    }
                 }
                }catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }
            }
        }
#else
        //used by the server to send our heartbeat
        public static void sendHeartbeat(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server heartbeat... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {

                try
                {
                    serverSendDictionary[clientIP].Send(bytes, (int)bytes.Length, serverSendEndPointDictionary[clientIP]);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

            }
        }
#endif



#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the client to send controller data to the server
        async public static void sendControllerInventory(string message)
        {
            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending out client Inventory..." + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            try
            {
               using (var stream = await clientSend.GetOutputStreamAsync(clientSendEndPoint, serverSocketListenPort.ToString()))
                    {
                    using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                    {
                       
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                         
                    }
                 }
                }catch (Exception e)
                        {
                            Debug.Log("here"+e.ToString());
                            Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                             return;
                        }

        }
#else
        //used by the client to send controller data to the server
        public static void sendControllerInventory(string message)
        {
            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Sending out client Inventory..." + message));
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                clientSend.Send(bytes, (int)bytes.Length, clientSendEndPoint);
            }
            catch (Exception ex)
            {
                //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                //log the error and move on (try again later)
                Debug.Log(ex.Message);
            }

        }
#endif



#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the server in response to a client's broadcast
        async public static void sendServerBroadcastResponse(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server broadcast response... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {
            try{
               using (var stream = await serverSendDictionary[clientIP].GetOutputStreamAsync(new Windows.Networking.HostName(clientIP), serverSendEndPointDictionary[clientIP].ToString()))
                    {
                    using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                    {
                       
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                         
                    }
                 }
                }   catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }

            }
        }
#else
        //used by the server in response to a client's broadcast
        public static void sendServerBroadcastResponse(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server broadcast response... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {

                try
                {
                    serverSendDictionary[clientIP].Send(bytes, (int)bytes.Length, serverSendEndPointDictionary[clientIP]);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

            }
        }
#endif


#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
        //used by the server in response to a client's inventory
       async public static void sendServerInventoryResponse(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server inventory Reply... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {
            try{
                using (var stream = await serverSendDictionary[clientIP].GetOutputStreamAsync(new Windows.Networking.HostName(clientIP), serverSendEndPointDictionary[clientIP].ToString()))
                {
                    using (var writer = new Windows.Storage.Streams.DataWriter(stream))
                    {
                       
                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();
                         
                    }
                 }
            }catch (Exception e)
                {
                    Debug.Log("here"+e.ToString());
                    Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
                     return;
                }

            }
        }
#else
        //used by the server in response to a client's inventory
        public static void sendServerInventoryResponse(string message, string clientIP)
        {
            if (isVerbose)
            {
                Debug.Log("Sending out server inventory Reply... " + message);
            }
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {

                try
                {
                    serverSendDictionary[clientIP].Send(bytes, (int)bytes.Length, serverSendEndPointDictionary[clientIP]);
                }
                catch (Exception ex)
                {
                    //the most common exception here is if you forgot to turn on your wifi (network unreachable)
                    //log the error and move on (try again later)
                    Debug.Log(ex.Message);
                }

            }
        }
#endif

        //used by the server listening to a client broadcast message
        public static void listenClientBroadcast(string message)
        {
            string[] splitMessage;

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
            DatagramSocket newClient;
            HostName newIPEndPoint;
#else
            UdpClient newClient;
            IPEndPoint newIPEndPoint;
#endif


            if (isVerbose)
            {
                Debug.Log("Recieved Client Broadcast... " + message);
            }
            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
            string newClientIPAddress = splitMessage[1];

            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {

                if (!serverSendDictionary.ContainsKey(newClientIPAddress))
                {
                    //we've received a client broadcast so add it and send a response

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
                    newClient = new DatagramSocket();
                    newIPEndPoint = new HostName(newClientIPAddress);
                    serverSendEndPointDictionary.Add(newClientIPAddress, clientScoketListenPort);
#else
                    newClient = new UdpClient()
                    {
                        EnableBroadcast = true
                    };

                    newIPEndPoint = new IPEndPoint(IPAddress.Parse(newClientIPAddress), clientScoketListenPort);
                    serverSendEndPointDictionary.Add(newClientIPAddress, newIPEndPoint);
#endif

                    serverSendDictionary.Add(newClientIPAddress, newClient);
                    serverSendKeys.Add(newClientIPAddress);
                    
                    

                }
                sendServerBroadcastResponse(EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_BROADCAST + EasyWiFiConstants.SPLITMESSAGE_COLON + myIPAddress, newClientIPAddress);
            }
        }

        //used by the client listening to a server responding to a broadcast message
        public static void listenServerBroadcastResponse(string message)
        {
            if (clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.Broadcasting)
            {
                string[] splitMessage;

                if (isVerbose)
                {
                    sendClientLog(createClientLogMessage("Recieved Server Broadcast Response... " + message));
                }
                splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
                serverIPAddress = splitMessage[1];

                //we've received the server response so add endpoint to send directly to server

#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
                clientSend = new DatagramSocket();
                 clientSendEndPoint = new HostName(serverIPAddress);
#else
                clientSend = new UdpClient()
                {
                    EnableBroadcast = true
                };
                clientSendEndPoint = new IPEndPoint(IPAddress.Parse(serverIPAddress), serverSocketListenPort);
#endif


                clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.ServerFound;
                if (readyToTransmitInventory)
                {
                    //if we aren't ready the wifi manager will send it when ready
                    sendControllerInventory(createInventoryMessage());
                }
            }
        }

        //used by the server listening to a client inventory message
        public static void listenClientInventoryMessage(string message)
        {
            string[] splitMessage, splitMessage2;
            string clientIP;
            List<string> responseKeys = new List<string>();;
            string currentServerKey = null;

            if (isVerbose)
            {
                Debug.Log("Recieved Client Inventory... " + message);
            }

            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);

            clientIP = splitMessage[1];

            splitMessage = splitMessage[2].Split(EasyWiFiConstants.SPLITARRAY_COMMA, StringSplitOptions.RemoveEmptyEntries);
          
            //we've received a client inventory so loop and register each control
            for (int i = 0; i < splitMessage.Length; i++)
            {
                splitMessage2 = splitMessage[i].Split(EasyWiFiConstants.SPLITARRAY_POUND, StringSplitOptions.RemoveEmptyEntries);
                currentServerKey = registerControl(splitMessage2[0], splitMessage2[1],clientIP);
                responseKeys.Add(currentServerKey+ EasyWiFiConstants.SPLITMESSAGE_POUND +splitMessage2[1] + EasyWiFiConstants.SPLITMESSAGE_COMMA);
            }

            //notify all the controls that a connection has changed
            if (isNew)
            {
                //reset the flag to false now
                isNew = false;

                //since this is a new connection (which can mean either brand new or previously disconnected)
                //send a callback
                lastCallbackTime = DateTime.UtcNow;
                isConnect = true;
                lastConnectedPlayerNumber = controllerDataDictionary[currentServerKey].logicalPlayerNumber;
                On_ConnectionsChanged(isConnect, lastConnectedPlayerNumber);
            }

            //now we need to tell the client the server key for each inventory item
            sendServerInventoryResponse(createInventoryResponseMessage(responseKeys), clientIP);
        }

        //used by the client listening to a server responding to a inventory message
        public static void listenServerInventoryResponse(string message)
        {
            string[] splitMessage, splitMessage2;

            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Received Server's Inventory Response... " + message));
            }
            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
            splitMessage = splitMessage[1].Split(EasyWiFiConstants.SPLITARRAY_COMMA, StringSplitOptions.RemoveEmptyEntries);

            //map the server keys to the data structure
            for (int i = 0; i < splitMessage.Length; i++)
            {
                splitMessage2 = splitMessage[i].Split(EasyWiFiConstants.SPLITARRAY_POUND, StringSplitOptions.RemoveEmptyEntries);
                controllerDataDictionary[splitMessage2[1]].serverKey = splitMessage2[0];
            }

            //we now have the server keys so set the flag to start the controller data stream
            clientState = EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData;
        }

        //used by the server listening to a client controller data message
        public static void listenClientControllerData(string message)
        {
            string[] splitMessage, splitMessage2;
            string packetNumber;
            string currentServerKey;

            if (isVerbose)
            {
                Debug.Log("Recieved Controller Data... " + message);
            }
            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
            packetNumber = splitMessage[1];
            splitMessage = splitMessage[2].Split(EasyWiFiConstants.SPLITARRAY_NEWLINE, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < splitMessage.Length; i++)
            {
                //here we have each line (including the server key)
                splitMessage2 = splitMessage[i].Split(EasyWiFiConstants.SPLITARRAY_POUND, StringSplitOptions.RemoveEmptyEntries);
                currentServerKey = splitMessage2[0];

                //pass the rest of the line (without server key) into the mapping function
                if (splitMessage2.Length > 1 && serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting && controllerDataDictionary.ContainsKey(currentServerKey))
                {
                    controllerDataDictionary[currentServerKey].mapNetworkDataToStructure(Convert.ToInt32(packetNumber), splitMessage2[1]);
                }
            }
        }

        //used by the client listening to a server backchannel message
        public static void listenServerBackchannelmessage(string message)
        {
            string[] splitMessage, splitMessage2;
            string packetNumber;
            string currentClientKey;

            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Recieved Server's Backchannel message... " + message));
            }
            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
            packetNumber = splitMessage[1];
            splitMessage = splitMessage[2].Split(EasyWiFiConstants.SPLITARRAY_NEWLINE, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < splitMessage.Length; i++)
            {
                //here we have each line (including the server key)
                splitMessage2 = splitMessage[i].Split(EasyWiFiConstants.SPLITARRAY_POUND, StringSplitOptions.RemoveEmptyEntries);
                currentClientKey = splitMessage2[0];

                //pass the rest of the line (without server key) into the mapping function
                if (splitMessage2.Length > 1 && controllerDataDictionary.ContainsKey(currentClientKey))
                {
                    controllerDataDictionary[currentClientKey].mapNetworkDataToStructure(Convert.ToInt32(packetNumber), splitMessage2[1]);
                }
            }
        }

        //used by the client listening to a server heartbeat message
        public static void listenServerHeartbeatmessage(string message)
        {
            if (isVerbose)
            {
                sendClientLog(createClientLogMessage("Recieved Server's Heartbeat message... " + message));
            }

            //register the current time of recieving heartbeat
            lastHeartbeatTime = DateTime.UtcNow;


        }

        //used by the server listening to a client log message
        public static void listenClientLogMessage(string message)
        {
            if (isVerbose)
            {
                Debug.Log("Client log: " + message);
            }
        }

        //used by the server listening to a client disconnect message
        public static void listenClientDisconnect(string message)
        {
            string[] splitMessage;
            BaseControllerType temp = null;

            if (isVerbose)
            {
                Debug.Log("Recieved Client Disconnect... " + message);
            }
            splitMessage = message.Split(EasyWiFiConstants.SPLITARRAY_COLON, StringSplitOptions.RemoveEmptyEntries);
            string clientIPAddress = splitMessage[1];

            if (serverState != EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting)
            {
                //loop on the main collection but all have the same keys
                foreach (string key in controllerDataDictionary.Keys)
                {
                    if (key.Contains(clientIPAddress))
                    {
                        //this will in effect notify the scripts watching this data
                        temp = controllerDataDictionary[key];
                        temp.previousConnectionPlayerNumber = temp.logicalPlayerNumber;
                        temp.logicalPlayerNumber = EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED;
                    }
                }

                //notify all the controls that a connection has changed
                isConnect = false;
                if (temp != null)
                    On_ConnectionsChanged(false,temp.previousConnectionPlayerNumber);
                
            }
        }

        public static string createInventoryMessage()
        {
            List<string> keys = new List<string>(controllerDataDictionary.Keys);
            BaseControllerType temp;

            string message = EasyWiFiConstants.MESSAGETYPE_CLIENT_INVENTORY + EasyWiFiConstants.SPLITMESSAGE_COLON + myIPAddress + EasyWiFiConstants.SPLITMESSAGE_COLON;

            foreach (string key in keys)
            {
                temp = controllerDataDictionary[key];

                message += temp.controllerType + EasyWiFiConstants.SPLITMESSAGE_POUND + temp.clientKey + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }

            return message;
        }

        public static string createInventoryResponseMessage(List<string> serverkeys)
        {
            string message = EasyWiFiConstants.MESSAGETYPE_SERVER_RESPONSE_INVENTORY + EasyWiFiConstants.SPLITMESSAGE_COLON;

            foreach (string serverkey in serverkeys)
            {
                //remember pound is already in message
                message += serverkey;
            }

            return message;
        }

        public static string createControllerDataMessage(int packetNumber)
        {
            List<string> keys = new List<string>(controllerDataDictionary.Keys);
            BaseControllerType temp;
            
            string message = EasyWiFiConstants.MESSAGETYPE_CONTROLLER_DATA + EasyWiFiConstants.SPLITMESSAGE_COLON + packetNumber.ToString() + EasyWiFiConstants.SPLITMESSAGE_COLON;

            foreach (string key in keys)
            {
                temp = controllerDataDictionary[key];

                if (!temp.controllerType.Contains(EasyWiFiConstants.BACKCHANNEL_FILTER))
                {
                    message += temp.serverKey + EasyWiFiConstants.SPLITMESSAGE_POUND + temp.mapStructureToNetworkData();
                }
            }

            return message;
        }

        public static void createAndSendBackchannelMessages(int packetNumber)
        {
            List<string> keys = new List<string>(controllerDataDictionary.Keys);
            BaseControllerType temp;

            

            //double loop because we are creating one message for each client
            foreach (string client in serverSendKeys)
            {
                bool send = false;
                string message = EasyWiFiConstants.MESSAGETYPE_BACKCHANNEL_DATA + EasyWiFiConstants.SPLITMESSAGE_COLON + packetNumber.ToString() + EasyWiFiConstants.SPLITMESSAGE_COLON;
                string append;

                foreach (string key in keys)
                {
                    temp = controllerDataDictionary[key];

                    if (temp.clientIP.Equals(client) && temp.controllerType.Contains(EasyWiFiConstants.BACKCHANNEL_FILTER) && temp.logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                    {
                        append = temp.mapStructureToNetworkData();
                        if (append != null && !append.Equals(String.Empty))
                        {
                            send = true;
                            message += temp.clientKey + EasyWiFiConstants.SPLITMESSAGE_POUND + append;
                        }
                    }
                }

                if (send)
                {
                    EasyWiFiController.sendWiFIBackchannelData(message, client);
                }

            }
        }

        public static void createAndSendHeartbeatMessages(int packetNumber)
        {
            BaseControllerType temp;

            //loop through each client
            foreach (string client in serverSendKeys)
            {
                bool send = true;
                string message = EasyWiFiConstants.MESSAGETYPE_HEARTBEAT + EasyWiFiConstants.SPLITMESSAGE_COLON + packetNumber.ToString();

                foreach (string key in controllerDataDictionary.Keys)
                {
                    temp = controllerDataDictionary[key];

                    if (temp.clientIP.Equals(client) && temp.logicalPlayerNumber == EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                    {
                        //we've found that this is actually disconnected
                        send = false;
                        break;
                    }
                }

                //we don't want to send heartbeat to those marked as disconnected
                if (send)
                {
                    EasyWiFiController.sendHeartbeat(message, client);
                }
            }
        }

        public static string createClientLogMessage(string logMessage)
        {
            Debug.Log(logMessage);
            string message = EasyWiFiConstants.MESSAGETYPE_CLIENTLOG + EasyWiFiConstants.SPLITMESSAGE_COLON + logMessage;
            return message;
        }

        public static string createDisconnectMessage()
        {
            string message = EasyWiFiConstants.MESSAGETYPE_DISCONNECT + EasyWiFiConstants.SPLITMESSAGE_COLON + myIPAddress;
            return message;
        }


        //this method is responsible for registering a control which will instance a specific controller type
        //add it to the data structure and set its key and return it
        //There are two ways this method is called, one on the client the UI script in its startup registers it client side
        //The second way is the server when a client sends over its inventory
        public static string registerControl(string type,string name, string IP="0")
        {
            BaseControllerType controller;
            string key = "";

            switch (type)
            {
                case EasyWiFiConstants.CONTROLLERTYPE_DPAD:
                    controller = new DpadControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_DPAD;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_JOYSTICK:
                    controller = new JoystickControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_JOYSTICK;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_BUTTON:
                    controller = new ButtonControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_BUTTON;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_TOUCHPAD:
                    controller = new TouchpadControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_TOUCHPAD;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_SLIDER:
                    controller = new SliderControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_SLIDER;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_PINCHZOOMTOUCHPAD:
                    controller = new PinchZoomTouchpadControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_PINCHZOOMTOUCHPAD;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_ACCELEROMETER:
                    controller = new AccelerometerControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_ACCELEROMETER;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_FLOAT:
                    controller = new FloatBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_FLOAT;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_INT:
                    controller = new IntBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_INT;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_DOUBLE:
                    controller = new DoubleBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_DOUBLE;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_BOOL:
                    controller = new BoolBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_BOOL;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_DECIMAL:
                    controller = new DecimalBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_DECIMAL;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_STRING:
                    controller = new StringBackchannelType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_STRING;
                    break;
                case EasyWiFiConstants.CONTROLLERTYPE_GYRO:
                    controller = new GyroControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_GYRO;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_FLOAT:
                    controller = new FloatBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_FLOAT;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_INT:
                    controller = new IntBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_INT;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_DOUBLE:
                    controller = new DoubleBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_DOUBLE;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_BOOL:
                    controller = new BoolBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_BOOL;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_DECIMAL:
                    controller = new DecimalBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_DECIMAL;
                    break;
                case EasyWiFiConstants.BACKCHANNELTYPE_STRING:
                    controller = new StringBackchannelType();
                    controller.controllerType = EasyWiFiConstants.BACKCHANNELTYPE_STRING;
                    break;
                default:
                    //should not occur
                    Debug.Log("Error: a controller type that isn't defined was registered");
                    controller = new DpadControllerType();
                    controller.controllerType = EasyWiFiConstants.CONTROLLERTYPE_DPAD;
                    break;
            }


            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_CLIENT))
            {
                //on the client we care what both the client and server key is but only client is known at registration time
                //the server key is discovered when the server responds to the inventory and will be populated then
                controller.clientKey = name;
                key = controller.clientKey;
                
            }
            if (peerType.Equals(EasyWiFiConstants.PEERTYPE_SERVER))
            {
                controller.serverKey = IP + name;
                controller.clientKey = name;
                controller.clientIP = IP;
                controller.logicalPlayerNumber = getNewPlayerNumber(name);
                controller.lastReceivedPacketTime = DateTime.UtcNow;
                lastConnectedPlayerNumber = controller.logicalPlayerNumber;
                isConnect = true; 
                key = controller.serverKey;
            }

            if (!controllerDataDictionary.ContainsKey(key))
            {
                //brand new first time here
                controllerDataDictionary.Add(key, controller);
                isNew = true;
            }
            else if (controllerDataDictionary[key].logicalPlayerNumber == EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
            {
                //already here just reassign (original disconnected and new one is here)
                //we will only change the player number if it's been occupied in between
                controllerDataDictionary[key].lastReceivedPacketTime = DateTime.UtcNow;
                controllerDataDictionary[key].justReconnected = true;
                if (!EasyWiFiUtilities.isPlayerNumberOccupied(controllerDataDictionary[key].previousConnectionPlayerNumber,controllerDataDictionary[key].clientKey))
                {
                    //not occupied assign it's previous number 
                    controllerDataDictionary[key].logicalPlayerNumber = controllerDataDictionary[key].previousConnectionPlayerNumber;
                }
                else
                {
                    //occupied give it the lowest available number
                    controllerDataDictionary[key].logicalPlayerNumber = controller.logicalPlayerNumber;
                }
                isNew = true;
            }
            else
            {
                //was already here and isn't currently marked as disconnected (most likely a delayed packet)
                isNew = false;
            }

            return key;
            
        }

        //this function is responsible for looping through all the keys and giving out the NEW player number
        //remember this instance has not been added yet
        public static int getNewPlayerNumber(string controlName)
        {
            int player = EasyWiFiUtilities.getHighestPlayerNumber() + 1;
            int returnIndex = 0;
            bool foundControl = false;

            for (int i = 0; i <= player; i++)
            {
                foundControl = EasyWiFiUtilities.isPlayerNumberOccupied(i, controlName);

                if (foundControl == false)
                {
                    //we found no control with this player number (lowest yet so return it)
                    returnIndex = i;
                    break;
                }
            }

            return returnIndex;
        }

        //not all platforms have unity network in them (WP8, windows store, etc)
        //don't use this method on mono platforms though it doesn't seem to work
        public static string getNetworkIPAddress()
        {
            
            string localIP = "?";
#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
            try{

             var icp = NetworkInformation.GetInternetConnectionProfile();
            var host = NetworkInformation.GetHostNames();
            foreach (var ip in host)
            {
                if (ip.Type == HostNameType.Ipv4)
                {
                      localIP = ip.CanonicalName;
                      Debug.Log("my Ip"+localIP);
                }
            }
            }catch (Exception ex)
                {
                    
                    Debug.Log(ex.Message);
                }
#endif
            return localIP;
        }

        //forces the connection callback to be called
        public static void forceConnectionRefresh()
        {
            lastCallbackTime = DateTime.UtcNow;
            On_ConnectionsChanged(isConnect, lastConnectedPlayerNumber);
        }

        //closes out all the UDPClients and Threads
        public static void endUDPClientAndThread()
        {
            serverState = EasyWiFiConstants.CURRENT_SERVER_STATE.Disconnecting;
#if !UNITY_EDITOR && (UNITY_WSA_10_0 || UNITY_WP_8_1 || UNITY_WSA_8_1)
            //close connections
            if (clientListen != null)
            {
                clientListen.Dispose();
                clientListen = null;
            }
            if (clientBroadcast != null)
            {
                clientBroadcast.Dispose();
                clientBroadcast = null;
            }
            if (serverListen != null)
            {
                serverListen.Dispose();
                serverListen = null;
            }

            if (clientSend != null)
            {
                clientSend.Dispose();
                clientSend = null;
            }
            if (serverSendKeys != null)
            {
                foreach (string key in serverSendKeys)
                {
                    if (serverSendDictionary[key] != null)
                    {
                        serverSendDictionary[key].Dispose();
                        serverSendDictionary[key] = null;
                    }
                }
            }
#else
            //close connections
            if (clientListen != null)
            {
                clientListen.Close();
                clientListen = null;
            }
            if (clientBroadcast != null)
            {
                clientBroadcast.Close();
                clientBroadcast = null;
            }
            if (serverListen != null)
            {
                serverListen.Close();
                serverListen = null;
            }

            if (clientSend != null)
            {
                clientSend.Close();
                clientSend = null;
            }
            if (serverSendKeys != null)
            {
                foreach (string key in serverSendKeys)
                {
                    if (serverSendDictionary[key] != null)
                    {
                        serverSendDictionary[key].Close();
                        serverSendDictionary[key] = null;
                    }
                }
            }
                        //abort threads
            if (listenThread != null)
            {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
                listenThread.Interrupt();
#endif
#if UNITY_EDITOR
                listenThread.Abort();//not support in il2cpp
#endif

                listenThread = null;
            }
            if (broadcastThread != null)
            {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
                broadcastThread.Interrupt();
#endif
#if UNITY_EDITOR
                broadcastThread.Abort();//not support in il2cpp
#endif
                broadcastThread = null;
            }
#endif



        }
    }
}
