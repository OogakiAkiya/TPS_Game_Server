using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] int damage=10;
    bool explosionFlg = false;
    protected System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Start()
    {
        timer.Start();
        this.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localScale = this.transform.localScale + new Vector3(0.2f, 0.2f, 0.2f);
        if (this.transform.localScale.x > 4.0) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<UserController>().hp -= damage;
        explosionFlg = true;
    }
}
