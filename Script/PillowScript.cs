using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillowScript : MonoBehaviour
{
    PlayerBehavior player;
    Rigidbody rigidBody;

    public LayerMask layerMask;
    float speed = 4f;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        player = FindObjectOfType<PlayerBehavior>();

        StartCoroutine(decreaseVelocity());
        StartCoroutine(DestroyPillow());
    }

    void Update()
    {
        shoot();

        RaycastHit hit;

        if (!Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            stop();
        }
    }

    public void shoot()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void stop()
    {
        speed = 0;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
    }

    IEnumerator decreaseVelocity()
    {
        yield return new WaitForSeconds(.06f);

        speed = speed - .3f;

        if (speed <= 1)
        {
            stop();
        }

        StartCoroutine(decreaseVelocity());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            stop();
        }

        else if (other.gameObject.tag == "Player")
        {
            player.isThrow = false;
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyPillow()
    {
        yield return new WaitForSeconds(2f);

        player.isThrow = false;
        Destroy(gameObject);
    }
}
