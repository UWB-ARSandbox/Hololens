using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HoloToolkit.Unity;
using UnityEngine;
using System.Collections;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Foundation;
using System.Threading.Tasks;
#endif



namespace SensorEmitterServer
{

    public enum Context
    {
        WhileStartingListener,
        WhileReceivingData
    }

    public class ValuesReceivedEventArgs<T> : EventArgs
    {
        public T SensorReading { get; set; }
    }

    public class ExceptionOccuredEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public Context Context { get; set; }
    }

    /// <summary>
    /// Interface for a sensor reading (classes that hold values of sensor measurements).
    /// </summary>
    public interface ISensorReading
    {
        /// <summary>
        /// Has to return a constant number of values this ISensorReading class expects.
        /// </summary>
        int NumSensorValues { get; }

        void SetSensorValues(float[] values);
    }

    /// <summary>
    /// A TCP Server, that accepts incoming connections from the Windows Phone
    /// 'Sensor Emitter' App, and packs all received data into SensorReading
    /// objects.
    ///
    /// Usage: Create a SensorServer&lt;SensorEmitterReading&gt; object, connect to both 
    /// ValuesReceived and ExceptionOccured events and start the server with 
    /// the Start method.
    /// </summary>
    /// <typeparam name="T">A class, which has to inherit from ISensorReading 
    /// and supply a parameterless constructor.</typeparam>
    public class SensorServer<T> : IDisposable
        where T : ISensorReading, new()
    {
        private const int MagicNumber = 0x42fea723;

        public const int DefaultTcpPort = 3547;

        private volatile bool alive = true;
#if UNITY_EDITOR
        private Thread listenThread;
#endif

        public event EventHandler<ValuesReceivedEventArgs<T>> ValuesReceived;
        public event EventHandler<ExceptionOccuredEventArgs> ExceptionOccured;

        private int numSensorValues;
#if !UNITY_EDITOR && UNITY_WSA_10_0
        /// <summary>
        /// If we are running as the server, this is the listener the server will use.
        /// </summary>
        private StreamSocketListener networkListener;
        private StreamSocketListenerConnectionReceivedEventArgs globalargs;
#endif

        public SensorServer()
        {
            numSensorValues = new T().NumSensorValues;
        }

        /// <summary>
        /// Starts listening on the TCP port 3547 and awaits phones that connect to it
        /// via the Windows Phone 'Sensor Emitter' App.
        ///
        /// If any clients connects and sends data, the ValuesReceived event is raised
        /// for each packet that arrives successfully. If any error occurs in the process,
        /// or while starting the TCP server, the ExceptionOccured event is fired.
        /// Make sure to connect to these two events.
        /// </summary>
        public void Start() { Start(SensorServer<T>.DefaultTcpPort); }

        /// <summary>
        /// Starts listening on the given TCP port and awaits phones that connect to it
        /// via the Windows Phone 'Sensor Emitter' App.
        ///
        /// If any clients connects and sends data, the ValuesReceived event is raised
        /// for each packet that arrives successfully. If any error occurs in the process,
        /// or while starting the TCP server, the ExceptionOccured event is fired.
        /// Make sure to connect to these two events.
        /// </summary>
        /// <param name="tcpPort">The port to listen to.</param>
        public void Start(int tcpPort)
        {
#if !UNITY_EDITOR && UNITY_WSA_10_0        
            ReceiveDataOverNetwork(tcpPort);
#endif
#if UNITY_EDITOR
            listenThread = new Thread(new ParameterizedThreadStart(ListenForClients));
            listenThread.IsBackground = true;
            listenThread.Start(tcpPort);
#endif
        }

#if !UNITY_EDITOR && UNITY_WSA_10_0

        private async void ReceiveDataOverNetwork(int tcpPort)
        {
         
           Debug.Log("listening set");
           networkListener = new StreamSocketListener();
           networkListener.ConnectionReceived += NetworkListener_ConnectionReceived;
           try
           {
            //bind the listener to port and IP
            await networkListener.BindServiceNameAsync(tcpPort.ToString());
           }
           catch (Exception e)
           {
                Debug.Log(e.ToString());
           }  
        }    
        /// <summary>
        /// When a connection is made to us, this call back gets called and
        /// we send our data.
        /// </summary>
        /// <param name="sender">The listener that was connected to.</param>
        /// <param name="args">some args that we don't use.</param>
        private void NetworkListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if(sender!=null)
            {   
                Debug.Log("PREPARING TO WRITE size");
                IOutputStream stream1 = args.Socket.OutputStream;
                globalargs=args;
                using (DataWriter writer = new DataWriter(stream1))
                {
                    
                    byte[] buffer = BitConverter.GetBytes(MagicNumber);
                    Array.Reverse(buffer);
                    writer.WriteBytes(buffer);
                    Debug.Log("DATA TO WRITE"+ BitConverter.ToString(buffer,0));
                    DataWriterStoreOperation dswo = writer.StoreAsync();
                    dswo.Completed = new AsyncOperationCompletedHandler<uint>(DataSentHandler);
                }
                
            }  
            else
            {
                //Debug.Log("Failed to establish connection." );
                args.Socket.Dispose();
            }   
            
        }

        public async void DataSentHandler(IAsyncOperation<uint> operation, AsyncStatus status )
        {
            // If we failed, requeue the data and set the deferral time.
            if (status == AsyncStatus.Error)
            {
                // didn't send, so requeue
               Debug.Log("DATA SENT INCOMPLETED");
            }
            else
            {
                // If we succeeded, clear the sending flag so we can send another mesh
                Debug.Log("DATA SENT COMPLETED");
                Debug.Log("PREPARING TO READ DATA...");
                    DataReader networkDataReader;
                    IInputStream stream = globalargs.Socket.InputStream;
                    using (networkDataReader = new DataReader(stream))
                    {
                        bool canRead = true;
                        while (canRead&&alive)
                        {   
                               try
                               {
                                    await networkDataReader.LoadAsync(sizeof(int));
                                    int length = networkDataReader.ReadInt32();

                                if (length < 0 || length != numSensorValues)
                                {
                                    if (length < 0)
                                    {
                                        OnException(new ArgumentOutOfRangeException("The format of " +
                                            "the TCP stream is incorrect. It said 'a negative number " +
                                            "of items is going to follow this', which obviously does " +
                                            "not make sense, as exactly " + numSensorValues +
                                            " values are expected."), Context.WhileReceivingData);
                                    Debug.Log("Length" + length);
                                    }
                                    else
                                    {
                                        OnException(new ArgumentOutOfRangeException("The format of " +
                                            "the TCP stream is incorrect. It said '" + length +
                                            " items are going to be in this single measurement " +
                                            "package', which does not match the expected value. " +
                                            "Valid packages from the SensorEmitter App have " +
                                            "exactly " + numSensorValues + " values."),
                                            Context.WhileReceivingData);
                                    Debug.Log("numSensorValues " + numSensorValues);
                                    Debug.Log("length " + length);
                                    canRead = false;
                                    }
                                }
                                else
                                {
                                    byte[] dataBuffer = new byte[sizeof(float)];
                                    float[] values = new float[length];

                                    for (int i = 0; i < length; i++)
                                    {
                                        await networkDataReader.LoadAsync(sizeof(float));
                                        networkDataReader.ReadBytes(dataBuffer);

                                        if (BitConverter.IsLittleEndian) { Array.Reverse(dataBuffer); }
                                        values[i] = BitConverter.ToSingle(dataBuffer, 0);
                                    }
                                    // Fire event, that a new pack of values was read
                                    OnValuesReceived(values);
                                }
                                
                              }
                              catch (Exception e)
                              {
                                   Debug.Log(e);
                              }
                        }
                    }   
                
            }

            // Always disconnect here since we will reconnect when sending the next mesh.  

            Debug.Log("CLOSED THE CONNECTION");
            networkListener.Dispose();
        }
#endif
        public int swapEndianInt(int numberToSwap)
        {
            //convert the int to byte[]
            byte[] bytes = BitConverter.GetBytes(numberToSwap);

            byte t = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = t;
           
            t = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = t;

            // Then bitconverter can read the int32.
            return BitConverter.ToInt32(bytes, 0);
        }
#if UNITY_EDITOR
        /// <summary>
        /// Thread, that waits for incoming connections. For each connection, a
        /// seperate thread is started then.
        /// </summary>
        private void ListenForClients(object port)
        {
            var tcpListener = new TcpListener(IPAddress.Any, (int)port);
            
            // Try to start the listener
            try { tcpListener.Start(); }
            catch (SocketException sockEx)
            {
                OnException(sockEx, Context.WhileStartingListener);
            }

            while (alive)
            {
                
                // Blocks until a client has connected to the server
                TcpClient client = tcpListener.AcceptTcpClient();

                // Create a thread to handle communication with connected client
                var clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }

            try { tcpListener.Stop(); }
            catch (SocketException) { }
        }


        /// <summary>
        /// Thread, that handles the actual communication after a client connected
        /// </summary>
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            // Write magic number as a greeting and to signal we are a compatible 'sensor emitter' server
            clientStream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(MagicNumber)), 0, 4);

            // Read from the incoming TCP stream
            using (BinaryReader br = new BinaryReader(clientStream))
            {
                bool canRead = true;
                while (canRead && alive)
                {
                    try
                    {
                        int length = IPAddress.NetworkToHostOrder(br.ReadInt32());

                        if (length < 0 || length != numSensorValues)
                        {
                            if (length < 0)
                                OnException(new ArgumentOutOfRangeException("The format of " +
                                    "the TCP stream is incorrect. It said 'a negative number " +
                                    "of items is going to follow this', which obviously does " +
                                    "not make sense, as exactly " + numSensorValues +
                                    " values are expected."), Context.WhileReceivingData);
                            else
                                OnException(new ArgumentOutOfRangeException("The format of " +
                                    "the TCP stream is incorrect. It said '" + length +
                                    " items are going to be in this single measurement " +
                                    "package', which does not match the expected value. " +
                                    "Valid packages from the SensorEmitter App have " +
                                    "exactly " + numSensorValues + " values."),
                                    Context.WhileReceivingData);
                            canRead = false;
                        }
                        else
                        {
                            // Read all measurement values
                            float[] values = new float[length];
                            for (int i = 0; i < length; i++)
                                values[i] = ReadNetworkFloat(br);

                            // Fire event, that a new pack of values was read
                            OnValuesReceived(values);
                        }
                    }
                    catch (IOException) { canRead = false; }
                }
            }

            tcpClient.Close();
        }
#endif

        /// <summary>
        /// Reads a float in network byte order (big endian) with the given binary reader.
        /// This is needed because the IPAddress class doesn't provide an overload
        /// of NetworkToHostOrder for any floating point types. This is equivalent to:
        ///
        ///     int ntohl = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(reader.ReadBytes(4), 0));
        ///     return BitConverter.ToSingle(BitConverter.GetBytes(ntohl), 0);
        ///
        /// but with better performance.
        /// </summary>
        public static float ReadNetworkFloat(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian) { Array.Reverse(bytes); }
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Fires the ExceptionOccured event back to the main thread.
        /// </summary>
        private void OnException(Exception ex, Context context)
        {
            if (ExceptionOccured != null)
                ExceptionOccured.Invoke(this, new ExceptionOccuredEventArgs() { Exception = ex, Context = context });
            //Application.Current.Dispatcher.Invoke(ExceptionOccured, this,
            //        new ExceptionOccuredEventArgs() { Exception = ex, Context = context });
        }

        /// <summary>
        /// Fires the ValuesReceived event back to the main thread.
        /// </summary>
        private void OnValuesReceived(float[] values)
        {
            if (ValuesReceived != null)
            {
                T sensorReading = new T();
                sensorReading.SetSensorValues(values);
                ValuesReceived.Invoke(this, new ValuesReceivedEventArgs<T>() { SensorReading = sensorReading });
                //Application.Current.Dispatcher.Invoke(ValuesReceived, this,
                 //       new ValuesReceivedEventArgs<T>() { SensorReading = sensorReading });
            }
        }


        /// <summary>
        /// Stops all associated threads, the TCP Server and clears up ressources of this object.
        /// </summary>
        public void Dispose()
        {
            alive = false;
            #if UNITY_EDITOR
            listenThread.Abort();
            #endif
        }

    }

    /// <summary>
    /// Holds values that were received from the Windows Phone 'Sensor Emitter' App.
    /// </summary>
    public class SensorEmitterReading : ISensorReading
    {
        public int NumSensorValues { get { return 23; } }

        public float QuaternionX { get; set; }
        public float QuaternionY { get; set; }
        public float QuaternionZ { get; set; }
        public float QuaternionW { get; set; }

        public float RotationPitch { get; set; }
        public float RotationRoll { get; set; }
        public float RotationYaw { get; set; }

        public float RotationRateX { get; set; }
        public float RotationRateY { get; set; }
        public float RotationRateZ { get; set; }

        public float RawAccelerationX { get; set; }
        public float RawAccelerationY { get; set; }
        public float RawAccelerationZ { get; set; }

        public float LinearAccelerationX { get; set; }
        public float LinearAccelerationY { get; set; }
        public float LinearAccelerationZ { get; set; }

        public float GravityX { get; set; }
        public float GravityY { get; set; }
        public float GravityZ { get; set; }

        public float MagneticHeading { get; set; }
        public float TrueHeading { get; set; }
        public float HeadingAccuracy { get; set; }

        public float MagnetometerX { get; set; }
        public float MagnetometerY { get; set; }
        public float MagnetometerZ { get; set; }
        public bool MagnetometerDataValid { get; set; }

        public void SetSensorValues(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException("No array of values given.");

            if (values.Length != NumSensorValues)
                throw new ArgumentException("Unexpected length of array. Exactly " +
                                            NumSensorValues + " items were expected.");

            float x = values[0];
            float y = values[1];
            float z = values[2];
            float w = values[3];

            RotationPitch = (float)Math.Atan2(2 * (y * z + w * x), w * w - x * x - y * y + z * z);
            RotationRoll = (float)Math.Atan2(2 * (x * y + w * z), w * w + x * x - y * y - z * z);
            RotationYaw = (float)Math.Asin(-2 * (x * z - w * y));

            QuaternionX = values[0];
            QuaternionY = values[1];
            QuaternionZ = values[2];
            QuaternionW = values[3];

            RotationRateX = values[7];
            RotationRateY = values[8];
            RotationRateZ = values[9];

            RawAccelerationX = values[13];
            RawAccelerationY = values[14];
            RawAccelerationZ = values[15];

            LinearAccelerationX = values[4];
            LinearAccelerationY = values[5];
            LinearAccelerationZ = values[6];

            GravityX = values[10];
            GravityY = values[11];
            GravityZ = values[12];

            MagneticHeading = values[16];
            TrueHeading = values[17];
            HeadingAccuracy = values[18];

            MagnetometerX = values[19];
            MagnetometerY = values[20];
            MagnetometerZ = values[21];
            MagnetometerDataValid = values[22] == 1d;
        }
    }
}