using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;


public class UserController : MonoBehaviour
{
    public string userId;
    public string IPaddr;
    public uint sequence=0;
    public Tcp_Server_Socket socket;
    public KEY nowKey { get; private set; } = 0;
    public int hp = 100;

    private List<byte[]> recvDataList = new List<byte[]>();
    private List<KEY> inputKeyList = new List<KEY>();
    private Animator animator;
    private AnimatorBehaviour animatorBehaviour;
    private UserAnimation userAnimation;

    //Ray判定用
    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;
    public Vector3 rotat = Vector3.zero;

    //武器
    public BaseWeapon weapon { get; private set; }
    private List<BaseWeapon> weaponList = new List<BaseWeapon>();
    private int weaponListIndex = 0;

    //グレネード
    private Transform bomPar;
    private int remainingGrenade = 2;
    private GameObject grenadePref;
    bool throwFlg = false;
    Grenade throwBom = null;

    //Score
    public int deathAmount { get; private set; } = 0;          //死んだ回数
    public int killAmount { get; private set; } = 0;           //殺した回数

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();
        userAnimation = this.GetComponent<UserAnimation>();

        //Ray判定用
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.Find("Pointer").GetComponent<RectTransform>();

        //武器関係
        weaponList.Add(new MachineGun(Shoot));
        weaponList.Add(new HandGun(Shoot));
        weapon = weaponList[weaponListIndex];

        //グレネード
        grenadePref = Resources.Load("Bom") as GameObject;
        bomPar = GameObject.FindGameObjectWithTag("BomList").transform;
    }

    // Update is called once per frame
    private void Update()
    {
        //回転
        Vector3 nowRotation = rotat;
        nowRotation.x = 0;
        nowRotation.z = 0;
        this.transform.rotation = Quaternion.Euler(nowRotation);

        //ボム制御を手放す
        if (throwBom == null) return;
        if (throwBom.destroyFlg)
        {
            throwBom.Delete();
            throwBom = null;
        }
    }
    public Task<int> UPdate()
    {
        return Task.Run(() =>
        {

        weapon.state.Update();

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


        if (recvDataList.Count > 0)
        {
            byte[] recvData = GetRecvData();
        }


            return 0;
        });
    }


    public string GetIPAddress()
    {
        return ((IPEndPoint)socket.socket.RemoteEndPoint).Address.ToString();
    }
    public void SetUserData(string _userId,Tcp_Server_Socket _socket)
    {
        userId = _userId;
        this.name = userId;
        socket = _socket;
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


    public byte[] GetStatus()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, this.name, (byte)GameHeader.GameCode.BASICDATA);
        byte[] positionData = Convert.GetByteVector3(this.transform.position);
        byte[] rotationData = Convert.GetByteVector3(this.transform.localEulerAngles);
        int currentKey = 0;
        if (userAnimation) currentKey = (int)userAnimation.animationState.currentKey;

        returnData.AddRange(header.GetHeader());
        returnData.AddRange(positionData);
        returnData.AddRange(rotationData);
        returnData.AddRange(BitConverter.GetBytes(currentKey));
        returnData.AddRange(BitConverter.GetBytes(hp));
        if (weapon != null) returnData.AddRange(weapon.GetStatus());
        return returnData.ToArray();

    }

    public byte[] GetScore()
    {
        List<byte> returnData = new List<byte>();
        GameHeader header = new GameHeader();
        header.CreateNewData(GameHeader.ID.GAME, this.name, (byte)GameHeader.GameCode.SCOREDATA);
        returnData.AddRange(header.GetHeader());
        returnData.AddRange(BitConverter.GetBytes(deathAmount));
        returnData.AddRange(BitConverter.GetBytes(killAmount));

        return returnData.ToArray();
    }

    public void Shoot()
    {
        //レイヤー変更
        this.gameObject.layer = LayerMask.NameToLayer("Default");

        //playerの移動,回転
        Vector3 nowRotation = this.transform.localEulerAngles;
        this.transform.rotation = Quaternion.Euler(rotat);

        //レイの作成
        Ray ray = cam.ScreenPointToRay(GetUIScreenPos(imageRect));
        //レイの可視化
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow,10f);

        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, weapon.range, 1 << 10))
        {
            if (hit.collider.tag == "users")
            {
                if (hit.collider.GetComponent<UserController>().ShootDamage(weapon.power)) killAmount++;
            }
        }

        //playerの移動,回転を戻す
        this.transform.rotation = Quaternion.Euler(nowRotation);
        this.gameObject.layer = LayerMask.NameToLayer("user");

    }

    private Vector2 GetUIScreenPos(RectTransform rt)
    {

        //UIのCanvasに使用されるカメラは Hierarchy 上には表示されないので、
        //変換したいUIが所属しているcanvasを映しているカメラを取得し、 WorldToScreenPoint() で座標変換する
        return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);

    }

    public bool ShootDamage(int _damage = 1)
    {
        //敵を倒した時trueを返す
        if (userAnimation.animationState.currentKey == ANIMATION_KEY.Dying) return false;

        hp -= _damage;
        if (hp < 0)
        {
            hp = 0;
            userAnimation.animationState.ChangeState(ANIMATION_KEY.Dying);
            deathAmount++;
            return true;
        }
        return false;
    }

    public void ChangeWeapon(bool _up = true)
    {
        if (_up)
        {
            weaponListIndex++;
            if (weaponListIndex == weaponList.Count) weaponListIndex = 0;
            nowKey = nowKey ^ KEY.LEFT_BUTTON;
        }
        else
        {
            weaponListIndex--;
            if (weaponListIndex < 0) weaponListIndex = weaponList.Count - 1;
            nowKey = nowKey ^ KEY.RIGHT_BUTTON;
        }
        //武器変更
        weapon = weaponList[weaponListIndex];
    }

    public void ThrowGrenade()
    {
        if (throwBom) return;
        if (remainingGrenade <= 0) return;
        if (!grenadePref) return;
        var obj= Instantiate(grenadePref,bomPar) as GameObject;
        throwBom = obj.GetComponent<Grenade>();
        throwBom.transform.position = this.transform.position+this.transform.forward + new Vector3(0, 1, 0);
        throwBom.transform.rotation = this.transform.rotation;
        throwBom.name = System.String.Format("{0, -" + (GameHeader.USERID_LENGTH-2) + "}", this.name) + remainingGrenade;
        remainingGrenade--;
    }
    public bool GetThrowFlg()
    {
        return throwBom;
    }
}

