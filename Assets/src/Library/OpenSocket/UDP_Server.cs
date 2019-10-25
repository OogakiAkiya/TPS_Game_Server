﻿using System.Collections;
using System.Collections.Generic;
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
    public void CallBack(IAsyncResult ar)
    {
        byte[] buf = socket.EndReceive(ar, ref endPoint);
        AddRecvData(endPoint, buf);
    }
}


class UDP_Server
{
    public ServerState server { get; private set; } = new ServerState();
    private ClientState sender = new ClientState();
    uint sequence = 0;
    int port = 0;
    int sendPort = 12343;

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


    //本来ならsendPortはportに変わる
    public void Send(KeyValuePair<IPEndPoint, byte[]> _data)
    {
        List<byte> sendData=new List<byte>();
        sendData.AddRange(BitConverter.GetBytes(sequence));
        sendData.AddRange(_data.Value);
        sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _data.Key.Address.ToString(), sendPort);

        CountUPSequence();

    }

    public void Send(byte[] _data, string _IP, int _port)
    {

        List<byte> sendData = new List<byte>();
        sendData.AddRange(BitConverter.GetBytes(sequence));
        sendData.AddRange(_data);
        sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _IP, _port);

        CountUPSequence();

    }

    //本来ならsendPortはportに変わる
    public void AllClietnSend(List<string> _ipList, List<byte[]> _data)
    {
        List<byte> sendDataList = new List<byte>();
        byte[] sequenceByte = BitConverter.GetBytes(sequence);
        for (int i = 0; i < _data.Count; i++)
        {
            sendDataList.AddRange(BitConverter.GetBytes((_data[i].Length + sequenceByte.Length+sizeof(int))));
            sendDataList.AddRange(sequenceByte);
            sendDataList.AddRange(_data[i]);
        }
        byte[] sendData = sendDataList.ToArray();
        for (int ip = 0; ip < _ipList.Count; ip++)
        {
            sender.socket.SendAsync(sendData, sendData.Length, _ipList[ip], sendPort);
        }
        CountUPSequence();

    }

    //使ってない
    public void AllClietnSend(IDictionary<string,int> _iplist, List<byte[]> _data)
    {
        byte[][] sendDataArray = new byte[_data.Count][];
        byte[] head = BitConverter.GetBytes(sequence);
        for (int i = 0; i < _data.Count; i++)
        {
            byte[] addData = new byte[head.Length + _data[i].Length];
            Array.Copy(head, addData, head.Length);
            Array.Copy(_data[i], 0, addData, head.Length, _data[i].Length);
            sendDataArray[i] = addData;
        }
        foreach (KeyValuePair<string, int> pair in _iplist)
        {
            for (int i = 0; i < _data.Count; i++)
            {
                sender.socket.SendAsync(sendDataArray[i], sendDataArray[i].Length, pair.Key, pair.Value);
                FileController.GetInstance().Write("UDPSend", "IP=" + pair.Key + ",port=" + pair.Value);
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
        //client.CallBack(ar);
        //client.socket.BeginReceive(ReceiveCallback, client);

        
        byte[] buf = client.socket.EndReceive(ar, ref client.endPoint);
        client.AddRecvData(client.endPoint, buf);
        client.socket.BeginReceive(ReceiveCallback, client);
        
    }

}
