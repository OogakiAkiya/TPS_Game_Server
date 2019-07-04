﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

class ClientState
{
    public UdpClient socket = null;
    public IPEndPoint endPoint;
    private List<KeyValuePair<IPEndPoint, byte[]>> recvDataList = new List<KeyValuePair<IPEndPoint, byte[]>>();
    private System.Object lockObject = new System.Object();

    ~ClientState()
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


class UDP_Client
{
    public ClientState server = new ClientState();
    private ClientState sender = new ClientState();

    int port = 0;
    int sendPort = 12344;

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
        sender.socket.SendAsync(_data.Value, _data.Value.Length, _data.Key.Address.ToString(), sendPort);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        ServerState client = (ServerState)ar.AsyncState;
        byte[] buf = client.socket.EndReceive(ar, ref client.endPoint);
        client.AddRecvData(client.endPoint, buf);
        client.socket.BeginReceive(ReceiveCallback, client);
    }

}
