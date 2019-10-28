using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewCollider : MonoBehaviour
{
    //範囲内のuserの取得
    public Dictionary<string, UserController> userMap = new Dictionary<string, UserController>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "users")
        {
            UserController userController = other.GetComponent<UserController>();
            if (userMap.ContainsKey(userController.userId)) return;
            userMap.Add(userController.userId, userController);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "users")
        {
            UserController userController = other.GetComponent<UserController>();
            userMap.Remove(userController.userId);
        }

    }
}
