﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class GameController : MonoBehaviour
{
    struct AddUserState
    {
       public string userID;
       public Tcp_Server_Socket socket;
    }
    public GameObject userListObj;
    private GameObject userPrefab;
    private int count = 0;
    //public List<UserController> users { get; private set; } = new List<UserController>();
    public UserController[] users;
    private UDP_ServerController udp_Controller;
    private TCP_ServerController tcp_Controller;
    TimeMeasurment timeMeasurment = new TimeMeasurment();
    private List<AddUserState> addUserList=new List<AddUserState>();



    // Start is called before the first frame update
    void Start()
    {
        users = userListObj.GetComponentsInChildren<UserController>();
        udp_Controller = this.GetComponent<UDP_ServerController>();
        tcp_Controller = this.GetComponent<TCP_ServerController>();
        userPrefab = (GameObject)Resources.Load("user");

        //Update回数制御
        QualitySettings.vSyncCount = 0; // VSyncをOFFにする
        
    }

    // Update is called once per frame
    void Update()
    {        
        var task1=tcp_Controller.UPdata();
        var task2 = udp_Controller.UPdata();
        Task.WaitAll(task1, task2);

        if (addUserList.Count > 0)
        {
            for (int i = 0; i < addUserList.Count; i++)
            {
                var add = Instantiate(userPrefab, userListObj.transform) as GameObject;
                add.name = addUserList[i].userID;
                add.transform.position = new Vector3(count, 0.0f, 0.0f);
                add.GetComponent<UserController>().SetUserData(addUserList[i].userID, addUserList[i].socket);
                count++;
            }
            addUserList.Clear();
            UsersUpdate();

        }

        //Task.WaitAll(task2);

        timeMeasurment.Fps(users.Length+"人:");
        //Task.WaitAll(task1,task2);
        
        //Task.WaitAll(task2);

    }

    private void LateUpdate()
    {
        if(!IsInvoking("Second30Invoke"))Invoke("Second30Invoke", 1f/30);
        if (!IsInvoking("SecondInvoke")) Invoke("SecondInvoke", 1f);

    }

    //addリストへの追加
    public void AddNewUser(string _userID,Tcp_Server_Socket _socket)
    {
        AddUserState add = new AddUserState();
        //add.SetUserData(_userID, _socket);
        add.userID = _userID;
        add.socket = _socket;
        addUserList.Add(add);
        //ユーザーの追加
        /*
        var add = Instantiate(userPrefab, userListObj.transform) as GameObject;
        add.name = _userID;
        add.transform.position = new Vector3(count, 0.0f, 0.0f);
        add.GetComponent<UserController>().SetUserData(_userID,_socket);
        count++;
        */
    }

    private void UsersUpdate()
    {
        users = userListObj.GetComponentsInChildren<UserController>();
    }

    public void Second30Invoke()
    {
        udp_Controller.SendAllClientData();
    }
    public void SecondInvoke()
    {
        udp_Controller.SendAllClientScoreData();
    }

}

