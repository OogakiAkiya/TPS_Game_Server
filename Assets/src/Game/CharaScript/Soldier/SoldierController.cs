using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierController : BaseController
{
    //Ray判定用
    private Camera cam;
    private RectTransform imageRect;
    private Canvas canvas;

    //グレネード
    private Transform bomPar;
    private int remainingGrenade = 2;
    private GameObject grenadePref;
    bool throwFlg = false;
    Grenade throwBom = null;

    // Start is called before the first frame update
    public void Start()
    {
        base.Init();

        //Ray判定用
        cam = transform.Find("Camera").gameObject.GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        imageRect = GameObject.Find("Canvas").transform.Find("Pointer").GetComponent<RectTransform>();

        //武器関係
        weaponList.Add(new MachineGun(Atack));
        weaponList.Add(new HandGun(Atack));
        weapon = weaponList[weaponListIndex];

        //グレネード
        grenadePref = Resources.Load("Bom") as GameObject;
        bomPar = GameObject.FindGameObjectWithTag("BomList").transform;

    }

    // Update is called once per frame
    public void Update()
    {
        base.BaseUpdate();

        //ボム制御を手放す
        if (throwBom == null) return;
        if (throwBom.destroyFlg)
        {
            throwBom.Delete(this);
            throwBom = null;
        }

    }


    public void ThrowGrenade()
    {
        if (throwBom) return;
        if (remainingGrenade <= 0) return;
        if (!grenadePref) return;
        var obj = Instantiate(grenadePref, bomPar) as GameObject;
        throwBom = obj.GetComponent<Grenade>();
        throwBom.transform.position = this.transform.position + this.transform.forward + new Vector3(0, 1, 0);
        throwBom.transform.rotation = this.transform.rotation;
        throwBom.name = System.String.Format("{0, -" + (GameHeader.USERID_LENGTH - 2) + "}", this.name) + remainingGrenade;
        remainingGrenade--;
    }
    public bool GetThrowFlg()
    {
        return throwBom;
    }



    //override
    public override void Atack()
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
                if (hit.collider.GetComponent<BaseController>().Damage(weapon.power)) killAmount++;
            }
        }

        //playerの移動,回転を戻す
        this.transform.rotation = Quaternion.Euler(nowRotation);
        this.gameObject.layer = LayerMask.NameToLayer("user");

    }


    public override bool Damage(int _damage = 1)
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

    private Vector2 GetUIScreenPos(RectTransform rt)
    {
        return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);
    }


}
