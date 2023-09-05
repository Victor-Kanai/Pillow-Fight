using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class PlayerBehavior : MonoBehaviour
{
    public AudioClip[] audios;
    AudioSource source;

    private float speed = 1.5f;
    public bool isThrow = false;
    public bool isDashing = false;
    public bool canAttack = true;
    private float dashForce = 3f;
    private int lifes = 1;
    public float attackRange = 1;
    public bool hitEnemy;
    public float horizontal;
    public float vertical;
    public bool isFalling;
    public bool btnPressed = false;


    [Header("End of the Round and Starting a New")]
    public Transform respawnPoint;
    public bool hasRoundEnded = false;
    public int numOfPlayerAlive = 5;
    int score = 0;

    [Header("Showing the hud when the player wins or not")]
    public GameObject winHud;
    public GameObject losehud;

    private bool canTeleport = false;

    public LayerMask layerMask;
    public LayerMask floor;

    public GameObject pillow;
    public GameObject deathFloor;
    public Transform teleportPoint;
    public Transform throwPillow;
    public Transform attackPoint;

    public Vector3 direction;
    public CharacterController controller;
    private int directionX = 0;
    private int directionZ = 0;

    [Header("Mobile Version of the Game")]
    public FixedJoystick mobileJoystick;
    public bool isPlayingOnMobile;
    public GameObject mobilePanel;
    public GameObject verificationPanel;
    public bool hasAttackedOnMobile;
    public bool hasThrownOnMobile;
    public bool hasDashedOnMobile;

    void Start()
    {
        Time.timeScale = 0;
        winHud.SetActive(false);
        losehud.SetActive(false);
        controller = GetComponent<CharacterController>();
        source = GetComponent<AudioSource>();
        hitEnemy = false;
    }

    void Update()
    {
        if (!isPlayingOnMobile)
        {
            mobilePanel.SetActive(false);
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

        }
        else if (isPlayingOnMobile)
        {
            mobilePanel.SetActive(true);
            horizontal = mobileJoystick.Horizontal;
            vertical = mobileJoystick.Vertical;
        }

        direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude > 0.1)
        {
            float rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, .25f, floor))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
            isFalling = false;
            canAttack = true;
        }
        else
        {
            isFalling = true;
            direction.y = -1;
            canAttack = false;
        }

        if (!canTeleport)
        controller.Move(direction * speed * Time.deltaTime);

        if (Input.GetButtonDown("Fire1") && isThrow == false && canAttack && !isPlayingOnMobile)
        {
            Attack();
        }

        if (Input.GetButtonDown("Fire2") && isThrow == false && canAttack && !isPlayingOnMobile)
        {
            PillowThrown();
        }

        if (Input.GetKey(KeyCode.E) && isDashing == false)
        {
            StartCoroutine(makeDash());
        }
        if (Input.GetKeyDown(KeyCode.E) && isDashing == false)
        {
            source.PlayOneShot(audios[1]);
        }

        if(numOfPlayerAlive == 1)
        {
            hasRoundEnded = true;

        }

        if (hasRoundEnded)
        {
            isPlayingOnMobile = false;

            //Mostra o hud de vitória :)
            winHud.SetActive(true);
            source.PlayOneShot(audios[6]);

            //Reseta a posicao do player e dos bots :)
            //StartCoroutine(StartNewRound());
        }
    }

    public void PillowThrown()
    {
        hasThrownOnMobile = true;
        source.PlayOneShot(audios[3]);
        Instantiate(pillow, throwPillow.position, throwPillow.rotation);
        isThrow = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Death")
        {
            lifes = 0;
            numOfPlayerAlive--;
            deathFloor.SetActive(false);

            isPlayingOnMobile = false;

            //Mostra o hud de derrota
            losehud.SetActive(true);
            source.PlayOneShot(audios[5]);
            //StartCoroutine(Respawn());
        }

        if(other.gameObject.tag == "Button")
        {
            if(GameObject.Find("Bridge Manager").GetComponent<BridgeBehavior>().canPress)
            {
                btnPressed = true;
            }
            
        }

        if(other.gameObject.tag == "Pillow")
        {
            source.PlayOneShot(audios[4]);
        }
    }

    public void Attack()
    {
        hasAttackedOnMobile = true;
        source.PlayOneShot(audios[0]);

        Collider[] enemyHit = Physics.OverlapSphere(attackPoint.position, attackRange, layerMask);

        foreach (Collider enemy in enemyHit)
        {
            StartCoroutine(Damage());
            enemy.GetComponent<EnemyBehavior>().Hitten();
        }
    }

    public void MakeDashOnMobileVersion()
    {
        StartCoroutine(makeDash());
        source.PlayOneShot(audios[1]);
    }

    public void HittenByEnemy(Transform enemyPos)
    {
        if(enemyPos.position.x <= transform.position.x)
        {
            directionX = 1;
        }
        if (enemyPos.position.x > transform.position.x)
        {
            directionX = -1;
        }

        if (enemyPos.position.z <= transform.position.z)
        {
            directionZ = 1;
        }
        if (enemyPos.position.z > transform.position.z)
        {
            directionZ = -1;
        }
        speed = 0;
        controller.SimpleMove(new Vector3(directionX * 15, 15f, directionZ * 15));

        source.PlayOneShot(audios[2]);

        StartCoroutine(HittenDelay());
    }

    IEnumerator StartNewRound()
    {
        yield return new WaitForSeconds(1f);
        numOfPlayerAlive = 5;
        hasRoundEnded = false;
        lifes = 1;
        transform.position = respawnPoint.position;
    }

    IEnumerator Respawn()
    {
        canTeleport = true;
        transform.position = teleportPoint.position;
        yield return new WaitForSeconds(.05f);
        deathFloor.SetActive(true);
        yield return new WaitForSeconds(.4f);
        canTeleport = false;
        lifes--;
    }

    IEnumerator HittenDelay()
    {
        yield return new WaitForSeconds(.25f);
        speed = 1.5f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    IEnumerator Damage()
    {
        hitEnemy = true;
        yield return new WaitForSeconds(.001f);
        hitEnemy = false;
    }

    private IEnumerator makeDash()
    {
        canAttack = false;
        controller.Move(transform.forward * dashForce * Time.deltaTime);
        yield return new WaitForSeconds(.3f);
        canAttack = true;
        isDashing = true;
        hasDashedOnMobile = true;
        yield return new WaitForSeconds(1.5f);
        isDashing = false;
    }

    //Botões de verificação de qual plataforma está jogando
    public void DesktopVersion()
    {
        isPlayingOnMobile = false;
        verificationPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MobileVersion()
    {
        isPlayingOnMobile = true;
        verificationPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
