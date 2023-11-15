using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Threading.Tasks;

public class PhoneServer : MonoBehaviour
{
    [SerializeField] int Port = 42069;
    [SerializeField] string Response = "Amigo";

    UdpClient Server;
    IPEndPoint IPep;
    byte[] ResponseData;

    // Start is called before the first frame update
    void Start()
    {
        Server = new UdpClient(new IPEndPoint(IPAddress.Any, Port));

        ResponseData = Encoding.ASCII.GetBytes(Response);
        ReceiveBroadcast();
    }


    async void ReceiveBroadcast()
    {
        while(true)
        {
            Debug.Log("Awaiting.");

            UdpReceiveResult result = await Server.ReceiveAsync();

            string ClientRequest = Encoding.ASCII.GetString(result.Buffer);
            Debug.Log($"Client request: {ClientRequest}");
            Debug.Log($"Remote end point: {result.RemoteEndPoint}");
            Debug.Log("Sending response");
            await Server.SendAsync(ResponseData, ResponseData.Length, result.RemoteEndPoint);
        }
    }
}
