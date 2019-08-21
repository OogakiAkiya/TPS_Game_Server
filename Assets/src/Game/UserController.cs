using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UserController : MonoBehaviour
{
    public string userId;
    public string IPaddr { get; set; }
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
    public Vector3 rotat=Vector3.zero;

    public BaseWeapon weapon;

    void Start()
    {
        animator = this.GetComponent<Animator>();
        animatorBehaviour = animator.GetBehaviour<AnimatorBehaviour>();
        userAnimation = this.GetComponent<UserAnimation>();

        //Ray判定用
        cam = transform.FindChild("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.FindChild("Pointer").GetComponent<RectTransform>();

        weapon = new MachineGun(Shoot);
    }

    // Update is called once per frame
    void Update()
    {
        weapon.state.Update();



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

    }

    public void SetUserID(string _userId)
    {
        userId = _userId;
        this.name = userId;

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
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] userName = enc.GetBytes(System.String.Format("{0, -" + Header.USERID_LENGTH + "}",this.name));              //12byteに設定する
        byte[] positionData = Convert.GetByteVector3(this.transform.position);
        byte[] rotationData = Convert.GetByteVector3(this.transform.localEulerAngles);
        int currentKey = 0;
        if (userAnimation) currentKey = (int)userAnimation.animationState.currentKey;

        returnData.Add((byte)Header.ID.GAME);
        returnData.AddRange(userName);
        returnData.Add((byte)Header.GameCode.BASICDATA);
        returnData.AddRange(positionData);
        returnData.AddRange(rotationData);
        returnData.AddRange(BitConverter.GetBytes(currentKey));
        returnData.AddRange(BitConverter.GetBytes(hp));
        returnData.AddRange(weapon.GetStatus());
        return returnData.ToArray();
    }


    public void Shoot()
    {
        //レイヤー変更
        this.gameObject.layer = LayerMask.NameToLayer("Default");

        //playerの移動,回転
        var nowRotation = this.transform.localEulerAngles;
        this.transform.rotation = Quaternion.Euler(rotat);

        //レイの作成
        Ray ray = cam.ScreenPointToRay(GetUIScreenPos(imageRect));
        //レイの可視化
        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow,10f);

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit,weapon.range,1<<10))
        {
            if (hit.collider.tag == "users") hit.collider.GetComponent<UserController>().ShootDamage(weapon.power);
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

    public void ShootDamage(int _damage=1)
    {
        if (userAnimation.animationState.currentKey == ANIMATION_KEY.Dying) return;

        hp-=_damage;
        if (hp < 0)
        {
            hp = 0;
            userAnimation.animationState.ChangeState(ANIMATION_KEY.Dying);
        }

    }
}

