using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

class ServerState
{
    public UdpClient socket = null;
    public IPEndPoint endPoint;
    private List<KeyValuePair<IPEndPoint, byte[]>> recvDataList = new List<KeyValuePair<IPEndPoint, byte[]>>();
    private System.Object lockObject = new System.Object();

    ~ServerState()
    {
        if (socket != null)
        {
            socket.Close();
        }
    }

    public KeyValuePair<IPEndPoint, byte[]> GetRecvData()
    {
        KeyValuePair<IPEndPoint, byte[]> returnByte;
        lock (lockObject)
        {
            returnByte = recvDataList[0];
            recvDataList.RemoveAt(0);
        }
        return returnByte;
    }

    public void AddRecvData(IPEndPoint _iPEndPoint, byte[] _data)
    {
        KeyValuePair<IPEndPoint, byte[]> addData = new KeyValuePair<IPEndPoint, byte[]>(_iPEndPoint, _data);
        lock (lockObject)
        {
            recvDataList.Add(addData);
        }
    }

    public int GetRecvDataSize()
    {
        int count = 0;
        lock (lockObject)
        {
            count = recvDataList.Count;
        }
        return count;
    }
}


class UDP_Server
{
    public ServerState server { get; private set; } = new ServerState();
    private ServerState sender = new ServerState();

    int port = 0;
    int sendPort = 12343;
    uint sequence = 0;

    public void Init(int _port)
    {
        port = _port;
        server.socket = new UdpClient(port);
        server.socket.BeginReceive(new AsyncCallback(ReceiveCallback), server);
        sender.socket = new UdpClient();

    }

    public void Init(int _port, int _sendPort)
    {
        port = _port;
        sendPort = _sendPort;
        server.socket = new UdpClient(port);
        server.socket.BeginReceive(new AsyncCallback(ReceiveCallback), server);
        sender.socket = new UdpClient();

    }


    public void Update()
    {
    }


    //本来ならsendPortはportに変わる
    public void Send(KeyValuePair<IPEndPoint, byte[]> _data)
    {
        List<byte> sendData = new List<byte>();
        sendData.AddRange(BitConverter.GetBytes(sequence));
        sendData.AddRange(_data.Value);
        sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _data.Key.Address.ToString(), sendPort);
        CountUPSequence();

    }

    //本来ならsendPortはportに変わる
    public void AllClietnSend(List<string> _ipList, List<byte[]> _data)
    {
        foreach (string IP in _ipList)
        {
            foreach (byte[] data in _data)
            {
                List<byte> sendData = new List<byte>();
                sendData.AddRange(BitConverter.GetBytes(sequence));
                sendData.AddRange(data);

                sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, IP, sendPort);
            }
        }
        CountUPSequence();

    }

    private void CountUPSequence()
    {
        sequence++;
        if (sequence > 4200000000)
        {
            sequence = 0;
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        ServerState client = (ServerState)ar.AsyncState;
        byte[] buf = client.socket.EndReceive(ar, ref client.endPoint);
        client.AddRecvData(client.endPoint, buf);
        client.socket.BeginReceive(ReceiveCallback, client);
    }

}
