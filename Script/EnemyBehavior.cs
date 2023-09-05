using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public AudioClip[] audios;
    AudioSource source;

    bool canAttack = true;
    bool isDamaged = false;
    NavMeshAgent agent;
    Rigidbody rb;
    public GameObject enemyAnim;
    public Transform playerPos;
    public GameObject deathGround;
    public Transform respawn;

    Vector3 destination;
    public LayerMask isNotGround;

    float directionXPlayer;
    float directionZPlayer;

    bool isStunned;

    PlayerBehavior player;

    int lifes = 1;

    public enum states
    {
        Idle,
        Patrol,
        Follow,
        Search
    }

    public states actualState;
    private Transform mainTarget;

    [Header("State: Idle")]
    public float timeIdle = 0f;
    private float timeWaiting;

    [Header("State: Patrol")]
    public Transform waypoint1;
    public Transform waypoint2;
    private Transform waypointActual;
    public float distMinWaypoint = 2;
    private float distWaypointActual;

    [Header("State: Follow")]
    public GameObject firstEnemyTarget;
    public GameObject secondEnemyTarget;
    public GameObject thirdEnemyTarget;
    public GameObject fourthEnemyTarget;
    public Transform mainEnemyTarget;
    private float distEnemyTarget;
    public float visionRange = 5;

    [Header("State: Search")]
    public float timePersistence = 0f;
    private float timeNoVision;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackArea = .5f;
    public LayerMask layerMask;

    [Header("Attack with impulse")]
    private float directionX = 0;
    private float directionZ = 0;
    public float horizontalForce;
    public float verticalForce;

    [Header("Respawn Enemy & Score Count")]
    public Transform respawnPointEnemy;
    int score = 0;

    void Start()
    {
        source = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerBehavior>();
        isStunned = false;
        waypointActual = waypoint1;
        Wait();
    }

    private void Wait()
    {
        actualState = states.Idle;
        timeWaiting = Time.time;
    }

    void Update()
    {
        if (player.horizontal != 0.0f)
        {
            directionXPlayer = player.horizontal;
        }

        if (player.vertical != 0.0f)
        {
            directionZPlayer = player.vertical;
        }

        if (!isDamaged)
        {
            destination = playerPos.position;
        }

        if (player.hitEnemy)
        {
            isDamaged = true;
        }

        if (mainEnemyTarget.position.x <= transform.position.x)
        {
            directionX = 1;
        }
        if (mainEnemyTarget.position.x > transform.position.x)
        {
            directionX = -1;
        }

        if (mainEnemyTarget.position.z <= transform.position.z)
        {
            directionZ = 1;
        }
        if (mainEnemyTarget.position.z > transform.position.z)
        {
            directionZ = -1;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 1.5f, isNotGround))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
            agent.enabled = false;
            canAttack = false;
        }

        CheckStates();
        //Attack();

        Vector3 lookMainTarget = new Vector3(mainTarget.transform.position.x, transform.position.y, mainTarget.transform.position.z);

        transform.LookAt(lookMainTarget);

        if (player.hasRoundEnded)
        {
            StartCoroutine(RespawnEnemy());
        }

        if(player.numOfPlayerAlive == 1)
        {
            if(lifes == 1)
            {
                score++;
            }
        }
    }

    IEnumerator RespawnEnemy()
    {
        agent.enabled = false;
        transform.position = respawnPointEnemy.position;
        lifes = 1;

        yield return new WaitForSeconds(1);

        agent.enabled = true;
    }

    private void Attack()
    {
        Collider[] inimigosBatidos = Physics.OverlapSphere(attackPoint.position, attackArea, layerMask);
        if (canAttack && !isStunned)
        {
            foreach (var inimigo in inimigosBatidos)
            {
                if (mainTarget.position == secondEnemyTarget.transform.position)
                {
                    secondEnemyTarget.GetComponent<EnemyBehavior>().HitenByEnemy();
                }
                if (mainTarget.position == firstEnemyTarget.transform.position)
                {
                    if(UnityEngine.Random.Range(0,25) == 0)
                    {
                        print($"Bati no {inimigo.name}!");
                        player.HittenByEnemy(transform);
                    }
                }
            }
        }
    }

    private void HitenByEnemy()
    {
        //print("Sofri ataque");
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().useGravity = true;
        agent.enabled = false;
        GetComponent<Rigidbody>().velocity = new Vector3(directionX * horizontalForce, verticalForce, directionZ * verticalForce);
        StartCoroutine(Stun());
    }

    private void CheckStates()
    {
        if (actualState != states.Follow && HasEnemyInRange())
        {
            FollowEnemy();
        }

        switch (actualState)
        {
            case states.Idle:

                if (WaitEnough())
                {
                    PatrolArea();
                }
                else
                {
                    mainTarget = transform;
                }

                break;

            case states.Patrol:

                if (CloseToWaypoint())
                {
                    Wait();
                    ChangeWaypoint();
                }
                else
                {
                    mainTarget = waypointActual;
                }

                break;

            case states.Follow:

                if (!HasEnemyInRange())
                {
                    SearchEnemy();
                }
                else
                {
                    mainTarget = mainEnemyTarget;
                }

                break;

            case states.Search:

                if (NoEnemyInRange())
                {
                    Wait();
                }

                break;
        }

        if (firstEnemyTarget == null)
        {
            firstEnemyTarget = GameObject.FindWithTag("Player");
        }

        if (secondEnemyTarget == null)
        {
            secondEnemyTarget = GameObject.FindWithTag("Player");
        }

        //Define o alvo escolhido
        agent.SetDestination(mainTarget.position);
    }

    private bool NoEnemyInRange()
    {
        return timeNoVision + timePersistence <= Time.time;
    }

    private void SearchEnemy()
    {
        actualState = states.Search;
        timeNoVision = Time.time;
        mainTarget = null;
    }

    private void ChangeWaypoint()
    {
        waypointActual = (waypointActual == waypoint1) ? waypoint2 : waypoint1;
    }

    private bool CloseToWaypoint()
    {
        distWaypointActual = Vector3.Distance(transform.position, waypointActual.position);
        return distWaypointActual <= distMinWaypoint;
    }

    private void PatrolArea()
    {
        actualState = states.Patrol;
    }

    private bool WaitEnough()
    {
        return timeWaiting + timeIdle <= Time.time;
    }

    private void FollowEnemy()
    {
        actualState = states.Follow;
    }

    private bool HasEnemyInRange()
    {
        if (Vector3.Distance(transform.position, firstEnemyTarget.transform.position) <= visionRange)
        {
            mainEnemyTarget = firstEnemyTarget.transform;
        }

        if (Vector3.Distance(transform.position, secondEnemyTarget.transform.position) <= visionRange)
        {
            mainEnemyTarget = secondEnemyTarget.transform;
        }

        if (Vector3.Distance(transform.position, thirdEnemyTarget.transform.position) <= visionRange)
        {
            mainEnemyTarget = thirdEnemyTarget.transform;
        }

        if (Vector3.Distance(transform.position, fourthEnemyTarget.transform.position) <= visionRange)
        {
            mainEnemyTarget = fourthEnemyTarget.transform;
        }

        distEnemyTarget = Vector3.Distance(transform.position, mainEnemyTarget.position);

        return distEnemyTarget <= visionRange;
    }

    public void Hitten()
    {
        source.PlayOneShot(audios[0]);
        agent.enabled = false;
        rb.velocity = new Vector3(directionXPlayer * horizontalForce, verticalForce, directionZPlayer * verticalForce);
        StartCoroutine(Stun());
    }

    IEnumerator Stun()
    {
        agent.enabled = false;
        yield return new WaitForSeconds(1.5f);
        agent.enabled = true;
        isStunned = false;
    }

    IEnumerator Respawn()
    {
        agent.enabled = false;
        transform.position = respawn.position;
        agent.enabled = true;
        yield return new WaitForSeconds(.05f);
        deathGround.SetActive(true);
        yield return new WaitForSeconds(.4f);
        lifes--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Pillow")
        {
            isStunned = true;
            StartCoroutine(Stun());
        }

        if (other.gameObject.tag == "Death")
        {
            player.numOfPlayerAlive--;
            lifes = 0;
            deathGround.SetActive(false);
            StartCoroutine(Respawn());
        }

        if (other.gameObject.tag == "Button")
        {
            if (GameObject.Find("Bridge Manager").GetComponent<BridgeBehavior>().canPress)
            {
                player.btnPressed = true;
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            source.PlayOneShot(audios[0]);
            player.HittenByEnemy(transform);
            print("A");
        }
        if (collision.gameObject.layer == 7)
        {
            source.PlayOneShot(audios[0]);
            collision.gameObject.GetComponent<EnemyBehavior>().HitenByEnemy();
            print(collision.gameObject.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackArea);
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
