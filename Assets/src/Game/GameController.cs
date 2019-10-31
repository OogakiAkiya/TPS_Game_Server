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
    public GameObject userListObj;                                                          //userを追加する親の参照
    public UserController[] users;                                                          //ログインしているユーザー
    private UserController[] notActiveUsers=new UserController[userAmount];                 //ログイン待ちインスタンス
    private int notActiveIndex= userAmount;
    private UDP_ServerController udp_Controller;
    private TCP_ServerController tcp_Controller;
    private List<AddUserState> addUserList=new List<AddUserState>();
    private const int userAmount = 100;                                                 //ログイン最大数

    //デバッグ用
    TimeMeasurment timeMeasurment = new TimeMeasurment();

    // Start is called before the first frame update
    void Start()
    {
        //Update回数制御
        QualitySettings.vSyncCount = 0; // VSyncをOFFにする

        //参照の作成
        udp_Controller = this.GetComponent<UDP_ServerController>();
        tcp_Controller = this.GetComponent<TCP_ServerController>();

        //userのインスタンスを作成
        GameObject userPrefab = (GameObject)Resources.Load("user");
        for (int i = 0; i < userAmount; i++)
        {
            var add = Instantiate(userPrefab, userListObj.transform) as GameObject;
            add.name = "___" + i;
            add.transform.position = new Vector3(i, 0.0f, 0.0f);
            add.SetActive(false);
            notActiveUsers[i] = add.GetComponent<UserController>();

        }
        users = userListObj.GetComponentsInChildren<UserController>();

    }

    // Update is called once per frame
    void Update()
    {
        Task[] tasks = new Task[2];
        tasks[0]=tcp_Controller.UPdata();
        tasks[1] = udp_Controller.UPdata();

        //デバッグ処理
        timeMeasurment.Fps(users.Length + "人:");

        //TCPとUDPのUpdate処理を終わるのをまつ
        Task.WaitAll(tasks);

        //ユーザーの非同期のアップデート処理
        tasks = new Task[users.Length];
        for(int i = 0; i < users.Length; i++)
        {
            tasks[i] = users[i].UPdate();
        }


        //ユーザーの追加処理
        AddUser();

        //TCPとUDPのUpdate処理を終わるのをまつ
        Task.WaitAll(tasks);

    }

    private void LateUpdate()
    {
        //if(!IsInvoking("Second30Invoke"))Invoke("Second30Invoke", 1f/30*((int)(users.Length/20)+1));
        if(!IsInvoking("Second30Invoke"))Invoke("Second30Invoke", 1f/30);
        if (!IsInvoking("SecondInvoke")) Invoke("SecondInvoke", 1f);
        if (!IsInvoking("SecondTempInvoke")) Invoke("SecondTempInvoke", 4f);

    }

    //addリストへの追加
    public void AddUserList(string _userID,Tcp_Server_Socket _socket)
    {
        for(int i = 0; i < addUserList.Count; i++)
        {
            if (addUserList[i].userID == _userID) return;
        }
        AddUserState add = new AddUserState();
        add.userID = _userID;
        add.socket = _socket;
        addUserList.Add(add);
    }
    private void AddUser()
    {
        if (addUserList.Count > 0)
        {
            for (int i = 0; i < addUserList.Count; i++)
            {
                if (notActiveIndex < 0) return;
                int index = userAmount - notActiveIndex--;
                notActiveUsers[index].gameObject.SetActive(true);
                notActiveUsers[index].name = addUserList[i].userID;
                notActiveUsers[index].SetUserData(addUserList[i].userID, addUserList[i].socket);
            }
            addUserList.Clear();
            UsersUpdate();
        }
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
    public void SecondTempInvoke()
    {
       udp_Controller.SendClientCompData();
    }
}

