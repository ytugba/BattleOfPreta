using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Serializable]
    public class AgentBehaviour
    {
        public float speed;
        public int angularSpeed;
        public float acceleration;
        public float stoppingDistance;
        public bool breakOnPlayer; //true if not wandering
    };

    public AgentBehaviour wandering;
    public AgentBehaviour chasing;

    //enemy general information
    private int currentHealth;
    private GameObject player;
    private float currentHitDelay;
    private bool isFollowing = false;

    public Collider[] interactableZones;

    public float playerDetectionRange = 4f;
    public float attackDetectionRange = 0.75f;
    public int maxHealth = 50;
    public bool hasPlayerDetected = false;
    public int hitPoint = 5;
    public float hitDelay = 1f;



    #region publicObjects
    public Animator animator;
    public ParticleSystem splash;
    public Rigidbody enemyBody;
    public Slider healthBar;
    public LayerMask playerLayer;
    public Transform enemyHitPoint;
    public LayerMask enemyHandLayer;
    public NavMeshAgent enemyAgent;
    private float hitTimer;
    public GameObject heart;
    #endregion

    private void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void LateUpdate()
    {
        Collider[] searchPlayer = Physics.OverlapSphere(enemyBody.transform.position, playerDetectionRange, playerLayer);

        if (searchPlayer.Length > 0)
        {
            hasPlayerDetected = true;
            animator.SetTrigger("Run");
            FollowPlayer();
        }
        else
        {
            hasPlayerDetected = false;
            animator.SetTrigger("Walk");
            WanderAround();
        }
    }

    private void WanderAround()
    {
        float dist = enemyAgent.remainingDistance; 
        if (dist != Mathf.Infinity && dist <= wandering.stoppingDistance)
        {
            SetAgent(wandering);
            //no player detected Wander
            Vector3 dest = RandomPoint(transform.position, playerDetectionRange);
            enemyAgent.SetDestination(dest);
            LookAt(dest);
        }
    }

    Vector3 RandomPoint(Vector3 center, float range)
    {
        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }

    private void FollowPlayer()
    {
        SetAgent(chasing);
        //detect player, wwalk on her, hit if within attack range.
        if(isFollowing)
        {
            enemyAgent.SetDestination(player.transform.position);
        }
    }

    private void SetAgent(AgentBehaviour agent)
    {
        enemyAgent.speed = UnityEngine.Random.Range(agent.speed - (agent.speed / 3),agent.speed + (agent.speed / 3));
        enemyAgent.angularSpeed = UnityEngine.Random.Range(agent.speed - (agent.speed / 3), agent.speed + (agent.speed / 3));
        enemyAgent.acceleration = UnityEngine.Random.Range(agent.speed - (agent.speed / 3), agent.speed + (agent.speed / 3));
        enemyAgent.stoppingDistance = agent.stoppingDistance;
        enemyAgent.autoBraking = agent.breakOnPlayer;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        splash.Play();
        healthBar.value = currentHealth;
        if (currentHealth <= 0)
        {
            GameObject insHeart = Instantiate(heart, new Vector3(this.gameObject.transform.position.x + 1f, heart.transform.position.y, this.gameObject.transform.position.z), heart.transform.rotation);
            insHeart.SetActive(true);
            Death();

            //gameObject.SetActive(false);
        }
    }

    void Death()
    {
        UnityEngine.Debug.Log("Dead");
        foreach (var i in interactableZones)
        {
            i.enabled = false;
        }
        this.gameObject.GetComponent<Collider>().enabled = false;
        splash.Play();
        animator.SetTrigger("Die");
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyHitPoint == null)
            return;
        if (transform == null)
            return;
        Gizmos.DrawWireSphere(enemyHitPoint.position, attackDetectionRange);
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
    }

    private void FixedUpdate()
    {
        if(hasPlayerDetected)
        {
            LookAt(player.transform.position);
            Collider[] hitPlayer = Physics.OverlapSphere(enemyHitPoint.position, attackDetectionRange, playerLayer);
            if (hitPlayer.Length > 0)
            {
                StopMoving();
                AttackPlayer();
            }
            isFollowing = true;
        }
    }

    private void AttackPlayer()
    {
        Collider[] hitPlayer = Physics.OverlapSphere(enemyHitPoint.position, attackDetectionRange, enemyHandLayer);

        if (hitPlayer != null)
        {
            currentHitDelay = UnityEngine.Random.Range(hitDelay - (hitDelay / 3), hitDelay + (hitDelay /3));
            LookAt(player.transform.position);
            if (hitTimer <= currentHitDelay)
            {
                hitTimer += Time.deltaTime * 1;
            }
            else
            {
                animator.SetTrigger("Attack");
                player.GetComponent<Combat>().TakeDamagePlayer(hitPoint);
                this.gameObject.GetComponent<AudioSource>().Play();
                hitTimer = 0f;
            }
        }
    }

    private void LookAt(Vector3 Pos)
    {
        Vector3 lookAt = Pos;
        transform.LookAt(Vector3.Lerp(lookAt, Pos, enemyAgent.angularSpeed * Time.deltaTime));
    }

    private void StopMoving()
    {
        enemyBody.MovePosition((Vector3)this.transform.position);
        isFollowing = false;
    }
}
