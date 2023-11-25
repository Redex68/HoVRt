using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System.Net;
using System.Globalization;

public class PhoneServer : MonoBehaviour
{
    [Tooltip("The port on which the script will listen for broadcasts on")]
    [SerializeField] int BroadcastPort = 42069;
    [Tooltip("The port on which the script will listen for accelerometer data")]
    [SerializeField] int AccelerometerPort = 42070;
    [SerializeField] float Timeout;

    //Default value points downwards in the phone's coordinate system
    public static Vector3 accelerometerData = Vector3.back;
    public static bool accelerometerRecent;

    byte[] ResponseData;
    float accelerometerLastRead = 0;

    // Start is called before the first frame update
    void Start()
    {
        ResponseData = Encoding.ASCII.GetBytes(AccelerometerPort.ToString());

        ReceiveBroadcast();
        AccelerometerListen();
    }

    void Update()
    {
        if(Time.time - accelerometerLastRead > Timeout)
            accelerometerRecent = false;
            //TODO: Add event that gets called here
    }

    async void ReceiveBroadcast()
    {
        UdpClient BroadcastServer = new(new IPEndPoint(IPAddress.Any, BroadcastPort));
        while(true)
        {
            Debug.Log("Awaiting.");

            UdpReceiveResult result = await BroadcastServer.ReceiveAsync();

            string ClientRequest = Encoding.ASCII.GetString(result.Buffer);
            Debug.Log($"Client request: {ClientRequest}");
            Debug.Log($"Remote end point: {result.RemoteEndPoint}");
            Debug.Log("Sending response");
            await BroadcastServer.SendAsync(ResponseData, ResponseData.Length, result.RemoteEndPoint);
        }
    }

    async void AccelerometerListen()
    {
        UdpClient Server = new(new IPEndPoint(IPAddress.Any, AccelerometerPort));
        while(true)
        {
            UdpReceiveResult response = await Server.ReceiveAsync();
            accelerometerLastRead = Time.time;
            accelerometerRecent = true;
            
            string data = Encoding.ASCII.GetString(response.Buffer);
            string[] segments = data.Split(" ");
            if(segments.Length != 3) continue;

            accelerometerData = new Vector3(float.Parse(segments[0], CultureInfo.InvariantCulture), float.Parse(segments[1], CultureInfo.InvariantCulture), float.Parse(segments[2], CultureInfo.InvariantCulture));
        }
    }
}
