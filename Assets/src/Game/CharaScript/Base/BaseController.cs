using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;


public class BaseController : MonoBehaviour
{
    public UserBodyData userData = new UserBodyData();
    public string userId = "";
    private string IPaddr = "";
    public int port = -1;
    public uint sequence = 0;
    public Tcp_Server_Socket socket;
    public KEY nowKey { get; private set; } = 0;
    public int hp = 100;

    protected List<byte[]> recvDataList = new List<byte[]>();
    protected List<KEY> inputKeyList = new List<KEY>();
    protected KEY checkKey;
    protected bool checkKeyFlg = false;
    protected Animator animator;
    protected BaseAnimation userAnimation;
    public GameHeader.UserTypeCode type=GameHeader.UserTypeCode.SOLDIER;

    //現在の回転度
    public Vector3 rotat = Vector3.zero;

    //Score
    public int deathAmount { get; protected set; } = 0;          //死んだ回数
    public int killAmount { get; set; } = 0;           //殺した回数

    public BaseComponent current;

    protected virtual void Awake() {
        current.Init();
    }

    protected virtual void Start()
    {
        animator = this.GetComponent<Animator>();
        userAnimation = current.GetComponent<BaseAnimation>();
    }
    protected virtual void Update()
    {
        //回転
        Vector3 nowRotation = rotat;
        nowRotation.x = 0;
        nowRotation.z = 0;
        this.transform.rotation = Quaternion.Euler(nowRotation);
    }
    public Task<int> UPdate()
    {
        return Task.Run(() =>
        {

            current.weapon.state.Update();

            //ダウンだけ検出するキーの初期化
            if (nowKey.HasFlag(KEY.G)) nowKey = nowKey ^ KEY.G;
            if (nowKey.HasFlag(KEY.R)) nowKey = nowKey ^ KEY.R;

            if (inputKeyList.Count > 0)
            {
                //入力値取得
                KEY inputKey = GetInputKey();
                //現在のキーを保存
                KEY oldKey = nowKey;
                //新しいキー入力を加算
                nowKey |= inputKey;
                //二度目のキー入力でフラグOFF
                nowKey = oldKey ^ inputKey;

            }

            //現在は無駄処理
            if (recvDataList.Count > 0)
            {
                byte[] recvData = GetRecvData();
            }

            //キーチェック
            if (checkKeyFlg)
            {
                nowKey = checkKey;
                checkKeyFlg = false;
            }

            return 0;
        });
    }
    public void SetCheckKey(KEY _key)
    {
        checkKey = _key;
        checkKeyFlg = true;
    }
    public void SetUserData(string _userId, Tcp_Server_Socket _socket)
    {
        userId = _userId;
        this.name = userId;
        socket = _socket;
        IPaddr = ((IPEndPoint)socket.socket.RemoteEndPoint).Address.ToString();
    }

    public void AddRecvData(byte[] _addData)
    {
        recvDataList.Add(_addData);
    }

    public void AddInputKeyList(KEY _addData)
    {
        inputKeyList.Add(_addData);
    }

    public KEY GetInputKey()
    {
        KEY returnData;
        returnData = inputKeyList[0];
        inputKeyList.RemoveAt(0);
        return returnData;
    }

    private byte[] GetRecvData()
    {
        byte[] returnData;
        returnData = recvDataList[0];
        recvDataList.RemoveAt(0);
        return returnData;
    }
    public byte[] GetScore()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, type,this.name, (byte)GameHeader.GameCode.SCOREDATA);
        returnData.AddRange(header.GetHeader());
        returnData.AddRange(BitConverter.GetBytes(deathAmount));
        returnData.AddRange(BitConverter.GetBytes(killAmount));

        return returnData.ToArray();
    }

    public void ChangeWeapon(bool _up = true)
    {
        current.ChangeWeapon(_up, () => { nowKey = nowKey ^ KEY.LEFT_BUTTON; });
    }

    public bool Damage(int _damage = 1)
    {
        //敵を倒した時trueを返す
        if (userAnimation.animationState.currentKey == ANIMATION_KEY.Dying) return false;

        hp -= _damage;
        if (hp <= 0)
        {
            hp = 0;
            userAnimation.animationState.ChangeState(ANIMATION_KEY.Dying);
            deathAmount++;
            return true;
        }

        return false;
    }

    public KeyValuePair<string, int> GetUserAddress()
    {
        KeyValuePair<string, int> address = new KeyValuePair<string, int>(IPaddr,port);
        return address;
    }

    //virtual
    public virtual byte[] GetStatus()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME,this.type,this.name, (byte)GameHeader.GameCode.BASICDATA);
        userData.SetData(this.transform.position, this.transform.localEulerAngles, (int)userAnimation.animationState.currentKey, hp);
        returnData.AddRange(header.GetHeader());
        returnData.AddRange(userData.GetData());
        if (current.weapon != null) returnData.AddRange(current.weapon.GetStatus());
        return returnData.ToArray();
    }

    public virtual byte[] GetStatusComplete()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, this.type,this.name, (byte)GameHeader.GameCode.CHECKDATA);
        userData.SetData(this.transform.position, this.transform.localEulerAngles, (int)userAnimation.animationState.currentKey, hp);
        returnData.AddRange(header.GetHeader());
        returnData.AddRange(userData.GetCompleteData());
        if (current.weapon != null) returnData.AddRange(current.weapon.GetStatus());
        return returnData.ToArray();

    }
}

