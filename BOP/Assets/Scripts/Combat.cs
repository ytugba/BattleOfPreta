using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class Combat : MonoBehaviour
{
    public ParticleSystem daggerBlast;
    public ParticleSystem abilityAvailable;
    public ParticleSystem abilityUsed;

    private const string attackTriggerName = "Attack";
    private const string specialAttackTriggerName = "Ability";

    private Animator _animator;
    private PlayerInput _playerInput;

    private bool isHit = false;
    private bool isAbilityAvailable = false;

    private int hitCounter = 0;
    public int maxHit = 5;


    public Transform hitPoint;
    public float attackRange = 0.75f;
    public LayerMask enemyLayer;

    public Camera playerCamera;

    public bool AttackInProgress {get; private set;} = false;
    public GameObject leftBound;
    public GameObject rightBound;
    public int daggerHitPoint = 5;
    public int abilityHitPoint = 15;
    public int playerMaxHealth = 100;
    public int playerMaxShield = 50;
    public Slider healthDisplay;
    public Slider shieldDisplay;
    public bool gotHit = false;

    public TMPro.TMP_Text hitScore;
    public GameObject gameManager;
    public GameObject deathPanel;
    int currentHealth = 0;
    int currentShield = 0;

    private float healthTimer = 0f;
    public float delayAmount =3f;
    public GameObject shield; 

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        PlayerPrefs.SetInt("PlayerHealth", playerMaxHealth);
        PlayerPrefs.SetInt("PlayerShield", playerMaxShield);
        currentHealth = PlayerPrefs.GetInt("PlayerHealth");
        currentShield = PlayerPrefs.GetInt("PlayerShield");
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        healthTimer += Time.deltaTime;

        if (healthTimer >= delayAmount && currentHealth < playerMaxHealth)
        {
            healthTimer = 0f;
            currentHealth += 1;
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            SetHealth();
        }

        if (isAbilityAvailable)
        {
            abilityAvailable.gameObject.SetActive(true);
            abilityAvailable.Play();
        }

        if(_playerInput.AttackInput && !AttackInProgress)
        {
            Attack();
        }
        else if (_playerInput.SpecialAttackInput && !AttackInProgress && isAbilityAvailable)
        {
            SpecialAttack();
        }
    }

    public void SetHealth()
    {
        healthDisplay.value = PlayerPrefs.GetInt("PlayerHealth");
        shieldDisplay.value = PlayerPrefs.GetInt("PlayerShield");
    }

    private void SetAttackStart()
    {
        AttackInProgress = true;
    }

    private void SetAttackEnd()
    {
        AttackInProgress = false;
    }

    internal void TakeDamagePlayer(int attackRate)
    {
        _animator.SetTrigger("Hurt");

        currentHealth = PlayerPrefs.GetInt("PlayerHealth");
        currentShield = PlayerPrefs.GetInt("PlayerShield");

        if(currentShield >= 0)
        {
            PlayerPrefs.SetInt("PlayerShield", ConsumeShield(currentShield, attackRate));
        }
        else
        {
            PlayerPrefs.SetInt("PlayerHealth", ConsumeHealth(currentHealth, attackRate));
        }

        SetHealth();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int ConsumeHealth(int currentHealth, int attackRate)
    {
        currentHealth -= attackRate;
        return currentHealth;
    }

    private int ConsumeShield(int currentShield, int attackRate)
    {
        if(attackRate >= currentShield)
        {
            int remaining = attackRate - currentShield;
            currentShield = 0;
            PlayerPrefs.SetInt("PlayerShield", currentShield);
            PlayerPrefs.SetInt("PlayerHealth",ConsumeHealth(PlayerPrefs.GetInt("PlayerHealth",remaining), attackRate));
        }
        else
        {
            //shield
            currentShield -= attackRate;
            PlayerPrefs.SetInt("PlayerShield", currentShield);
        }

        return currentShield;
    }

    private void Die()
    {
        //we die here
        _animator.SetBool("IsDead", true);
        gameManager.GetComponent<AudioSource>().Stop();
        deathPanel.SetActive(true);
        foreach(var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        this.enabled = false;
    }

    private void Attack()
    {
        _animator.SetTrigger(attackTriggerName);
        isHit = CheckHit();
        if(isHit)
        {
            //reduce life of enemy
            hitCounter++;
            if (maxHit == hitCounter)
            {
                isAbilityAvailable = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WavePoints"))
        {
            gameManager.GetComponent<EnemySpawner>().isAnyWaveActive = true;
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Health"))
        {
           currentHealth = ((currentHealth + 10) >= playerMaxHealth) ? playerMaxHealth : currentHealth + 10;
            PlayerPrefs.SetInt("PlayerHealth", currentHealth);
            Debug.Log("Health is increased.");
            SetHealth();
        }
        else if (other.CompareTag("Shield"))
        {
            currentShield = ((currentShield + 10) >= playerMaxShield) ? playerMaxShield : currentShield + 10;
            PlayerPrefs.SetInt("PlayerShield", currentShield);
            Debug.Log("Shield is increased.");
            SetHealth();
        }
    }

    private bool CheckHit()
    {
        bool hit = Hit(daggerHitPoint);

        daggerBlast.gameObject.SetActive(true);
        daggerBlast.Play();
        daggerBlast.GetComponent<AudioSource>().Play();
        if (daggerBlast.isStopped)
        {
            daggerBlast.gameObject.SetActive(false);
        }

        return hit;
    }

    private bool Hit(int damage)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(hitPoint.position, attackRange, enemyLayer);
        if (hitEnemies.Length > 0)
        {
            foreach (Collider enemy in hitEnemies)
            {
                //Debug.Log("Hit!" + enemy.name);
                enemy.GetComponent<Enemy>().TakeDamage(damage);
                //transform.rotation = Quaternion.LookRotation(enemy.transform.position);
                if (damage == abilityHitPoint)
                {
                    DropShield(enemy);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DropShield(Collider enemy)
    {
        GameObject insShield = Instantiate(shield, new Vector3(enemy.gameObject.transform.position.x + 1f, shield.transform.position.y, enemy.gameObject.transform.position.z), shield.transform.rotation);
        insShield.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        if (hitPoint == null)
            return;
        Gizmos.DrawWireSphere(hitPoint.position, attackRange);
    }

    private void SpecialAttack()
    {
        StartCoroutine(AnimateSpecial());
    }

    private IEnumerator AnimateSpecial()
    {
        _animator.SetTrigger(specialAttackTriggerName);
        yield return new WaitForSeconds(0.5f);
        //yield return new WaitUntil((() => AnimatorIsPlaying(specialAttackTriggerName)));
        abilityUsed.GetComponent<AudioSource>().Play();
        isAbilityAvailable = false;
        hitCounter = 0;
        abilityUsed.gameObject.SetActive(true);
        abilityAvailable.gameObject.SetActive(false);
        Hit(abilityHitPoint);
    }
}