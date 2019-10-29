using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewCollider : MonoBehaviour
{
    //範囲内のuserの取得
    public Dictionary<string, UserController> userMap = new Dictionary<string, UserController>();
    public GameController gameController;

    private void Update()
    {
        gameController=GameObject.Find("Server").GetComponent<GameController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "users")
        {
            if (userMap.ContainsKey(other.name)) return;
            if (gameController == null) return;
            for (int i = 0; i < gameController.users.Length; i++)
            {
                if(other.name==gameController.users[i].name)userMap.Add(gameController.users[i].name, gameController.users[i]);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "users")
        {
            userMap.Remove(other.name);
        }

    }
}
