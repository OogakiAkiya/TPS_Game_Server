using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class GameController : MonoBehaviour
{
    private GameObject userPrefab;
    private int count = 0;
    //public GameObject[] users { get; private set; } = new GameObject[0];
    public List<UserController> users { get; private set; } = new List<UserController>();
    private UDP_ServerController udp_Controller;
    //FPS回数
    int frameCount;
    float prevTime;


    // Start is called before the first frame update
    void Start()
    {
        udp_Controller=this.GetComponent<UDP_ServerController>();
        userPrefab = (GameObject)Resources.Load("user");

        //FPS回数
        frameCount = 0;
        prevTime = 0.0f;

        //Update回数制御
        
        QualitySettings.vSyncCount = 0; // VSyncをOFFにする
        //Application.targetFrameRate = 160; // ターゲットフレームレートを160に設定
        
    }

    // Update is called once per frame
    void Update()
    {
        //FPS表示
        ++frameCount;
        float time = Time.realtimeSinceStartup - prevTime;

        //60fpsのタイミング
        if (time >= 0.016f)
        {
            //Debug.LogFormat("{0}fps", frameCount / time);
            //データ送信
            udp_Controller.SendAllClientData();

            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
    }

    public void AddNewUser(string _userID)
    {
        //ユーザーの追加
        var add = Instantiate(userPrefab, new Vector3(count, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        add.name = _userID;
        add.GetComponent<UserController>().SetUserID(_userID);
        count++;

    }

    public void UsersUpdate()
    {
        //users = GameObject.FindGameObjectsWithTag("users");
        foreach(var user in GameObject.FindGameObjectsWithTag("users"))
        {
            users.Add(user.GetComponent<UserController>());
        }
    }
}

public static class Convert
{
    public static Vector3 GetVector3(byte[] _data, int _beginPoint = 0, bool _x = true, bool _y = true, bool _z = true)
    {
        Vector3 vect = Vector3.zero;
        if (_data.Length < sizeof(float) * 3) return vect;
        if (_x) vect.x = BitConverter.ToSingle(_data, _beginPoint + 0 * sizeof(float));
        if (_y) vect.y = BitConverter.ToSingle(_data, _beginPoint + 1 * sizeof(float));
        if (_z) vect.z = BitConverter.ToSingle(_data, _beginPoint + 2 * sizeof(float));
        return vect;
    }
    public static byte[] GetByteVector3(Vector3 _vector, bool _x = true, bool _y = true, bool _z = true)
    {
        byte[] vect = new byte[sizeof(float) * 3];
        if (_x) Buffer.BlockCopy(BitConverter.GetBytes(_vector.x), 0, vect, 0 * sizeof(float), sizeof(float));
        if (_y) Buffer.BlockCopy(BitConverter.GetBytes(_vector.y), 0, vect, 1 * sizeof(float), sizeof(float));
        if (_z) Buffer.BlockCopy(BitConverter.GetBytes(_vector.z), 0, vect, 2 * sizeof(float), sizeof(float));

        return vect;
    }

}