using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {

        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("RedTarget") || collision.collider.gameObject.layer == LayerMask.NameToLayer("BlueTarget"))
        {
            // Kill the enemy if he is hit:
            collision.collider.gameObject.transform.parent.gameObject.GetComponent<CharacterActions>().KillPlayer();
            Debug.Log("Crashed with enemy :D");
        }

        //Kills the bullet:
        Destroy(gameObject.transform.parent.gameObject);
    }
}
