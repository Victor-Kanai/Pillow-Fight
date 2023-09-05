using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeBehavior : MonoBehaviour
{
    public GameObject bridgeLeft;
    public GameObject bridgeRight;
    public GameObject obstacleBridgeLeft;
    public GameObject obstacleBridgeRight;
    public GameObject aiBridgeLeft;
    public GameObject aiBridgeRight;
    public GameObject player;

    public bool canPress = true;

    Animator leftBridgeAnim;
    Animator rightBridgeAnim;

    private void Awake()
    {
        obstacleBridgeLeft.SetActive(false);
        obstacleBridgeRight.SetActive(false);

        aiBridgeLeft.GetComponent<MeshRenderer>().enabled = false;
        aiBridgeRight.GetComponent<MeshRenderer>().enabled = false;
    }

    void Start()
    {
       leftBridgeAnim = bridgeLeft.GetComponent<Animator>();
        rightBridgeAnim = bridgeRight.GetComponent<Animator>();
    }

    void Update()
    {
        if (player.GetComponent<PlayerBehavior>().btnPressed)
        {
            player.GetComponent<PlayerBehavior>().btnPressed = false;
            canPress = false;
            leftBridgeAnim.SetTrigger("Pressed");
            rightBridgeAnim.SetTrigger("Pressed"); 

            StartCoroutine(ReturnPos());
        }
    }

    IEnumerator ReturnPos()
    {
        yield return new WaitForSeconds(.01f);

        obstacleBridgeLeft.SetActive(true);
        obstacleBridgeRight.SetActive(true);

        yield return new WaitForSeconds(7f);

        obstacleBridgeLeft.SetActive(false);
        obstacleBridgeRight.SetActive(false);
        canPress = true;
    }
}
