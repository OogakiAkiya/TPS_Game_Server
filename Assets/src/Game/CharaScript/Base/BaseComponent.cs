using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseComponent:MonoBehaviour
{
    [SerializeField] public BaseController myController;
    //武器
    public BaseWeapon weapon { get; protected set; }
    public List<BaseWeapon> weaponList { get; protected set; } = new List<BaseWeapon>();
    public int weaponListIndex = 0;
    public GameHeader.UserTypeCode type = GameHeader.UserTypeCode.SOLDIER;

    [SerializeField]public int defense = 0;
    public virtual void Init() {
        if(myController)myController.type = type;
    }


    public void ChangeWeapon(bool _up, System.Action _action = null)
    {
        if (_up)
        {
            weaponListIndex++;
            if (weaponListIndex == weaponList.Count) weaponListIndex = 0;
            if (_action != null) _action();
        }
        else
        {
            weaponListIndex--;
            if (weaponListIndex < 0) weaponListIndex = weaponList.Count - 1;
            if (_action != null) _action();

        }
        //武器変更
        weapon = weaponList[weaponListIndex];
    }

    public virtual void Attack() { }
    public virtual void End() { }
}