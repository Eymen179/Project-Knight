using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Attack, Return }

    [Header("References & Settings")]
    public EnemyStats stats;
    public AIState currentState = AIState.Patrol;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private Animator _animator;

    //Hafiza ve Bolge Degiskenleri
    private Vector3 startPosition;
    private float memoryTimer;
    private float patrolWaitTimer;

    //"Duvar" sayilacak layer
    [SerializeField] private LayerMask obstacleMask;

    [Header("Gizmo Colors")]
    public Color redColor;
    public Color greenColor;
    public Color yellowColor;

    //Saldiri bekleme suresi (Cooldown) icin sayac
    private float nextAttackTime = 0f;

    private bool isBlocking = false;
    private float blockTimer = 0f;

    private EnemyAttackSystem attackSystem;
    private EnemyHealth enemyHealth;

    //Spawner bazli yaricap ayari
    [HideInInspector] public float spawnerWanderRadius = 0f;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (stats != null)
        {
            agent.speed = stats.patrolSpeed;
        }

        //Merkez konumunu kaydet.
        startPosition = transform.position;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTarget = player.transform;

        attackSystem = GetComponent<EnemyAttackSystem>();
    }

    void Update()
    {
        if (playerTarget == null || stats == null) return;

        SetAttackSpeed();

        //Oyuncuyu goruyor mu?
        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        //NPC oyuncuya arkasini donukken oyuncudan hasar aldiysa aninda oyuncuya kitlenmesini saglayan kontrolcu
        if (enemyHealth.currentHealth != enemyHealth.healthBeforeDamage)
        {
            //Can ayari
            enemyHealth.healthBeforeDamage = enemyHealth.currentHealth;

            //Hafiza tazele.
            memoryTimer = stats.memoryTime;

            if (distanceToPlayer <= stats.attackRange)
            {
                SwitchState(AIState.Attack);
            }
            else if (!canSeePlayer)
            {
                SwitchState(AIState.Chase);
            }
        }

        // --- (STATE MACHINE) ---
        if (canSeePlayer)
        {
            //Oyuncuyu goruyorsa hafizayi tazele ve saldirmaya/kovalamaya basla.
            memoryTimer = stats.memoryTime;

            if (distanceToPlayer <= stats.attackRange)
            {
                SwitchState(AIState.Attack);
            }
            else
            {
                SwitchState(AIState.Chase);
            }
        }
        else
        {
            // Oyuncuyu GORMUYORSA hafiza suresi bitene kadar takip et.
            if (currentState == AIState.Chase || currentState == AIState.Attack)
            {
                memoryTimer -= Time.deltaTime;
                if (memoryTimer <= 0)
                {
                    //Pes et ve merkeze don.
                    SwitchState(AIState.Return);
                }
            }
        }

        //Bulundugumuz duruma gore eylemleri yap.
        ExecuteCurrentState();
    }

    private void SwitchState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                PatrolBehavior();
                break;
            case AIState.Chase:
                ChaseBehavior();
                break;
            case AIState.Attack:
                AttackBehavior();
                break;
            case AIState.Return:
                ReturnBehavior();
                break;
        }
    }

    // --- YAPAY ZEKA DAVRANIÞLARI ---
    private void PatrolBehavior()
    {
        agent.speed = stats.patrolSpeed;

        //Hedefe ulastiysa veya hic hedefi yoksa bekle.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (_animator != null) _animator.SetFloat("speed", 0f);

            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0)
            {
                //NPC'nin ya da Spawner'inin yaricap ayarini kullan.
                float activeRadius = (spawnerWanderRadius > 0) ? spawnerWanderRadius : stats.wanderRadius;

                //Yeni noktaya dogru yurumeye basla.
                Vector3 randomPoint = GetRandomPoint(startPosition, activeRadius);
                agent.SetDestination(randomPoint);
                agent.isStopped = false;
                patrolWaitTimer = Random.Range(2f, 5f);
            }
        }
        else
        {
            if (_animator != null) _animator.SetFloat("speed", 0.5f);
        }
    }

    private void ChaseBehavior()
    {
        //Eger NPC blok yaparken oyuncu menzilden kacarsa, blogu hemen indirsin.
        if (isBlocking)
        {
            isBlocking = false;
            if (_animator != null) _animator.SetBool("isBlocking", false);
        }

        agent.speed = stats.chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);

        if (_animator != null) _animator.SetFloat("speed", 1f);
    }

    private void AttackBehavior()
    {
        agent.isStopped = true;
        FaceTarget(playerTarget.position);

        if (_animator != null) _animator.SetFloat("speed", 0f);

        //NPC blok yaparken
        if (isBlocking)
        {
            //INTERRUPT
            if (_animator != null) _animator.ResetTrigger("attack");

            blockTimer -= Time.deltaTime;

            if (blockTimer <= 0)
            {
                //Sure doldu, blok bitirilir.
                isBlocking = false;
                if (_animator != null) _animator.SetBool("isBlocking", false);

                //Blok bitince aninda saldirmasin diye nefes alma payi
                nextAttackTime = Time.time + 0.5f;
            }
            return;
        }

        //Hamle yaparken
        if (Time.time >= nextAttackTime)
        {
            int decision = Random.Range(0, 100);

            if (decision < stats.blockChance)
            {
                //Blok yapmaya karar verdi.
                isBlocking = true;
                blockTimer = Random.Range(1.5f, 2.5f);

                if (_animator != null)
                {
                    _animator.SetBool("isBlocking", true);

                    _animator.ResetTrigger("attack");
                }
            }
            else
            {
                //Saldirmaya karar verdi.
                if (_animator != null) _animator.SetTrigger("attack");
                nextAttackTime = Time.time + stats.attackCooldown;
            }
        }
    }

    private void ReturnBehavior()
    {
        agent.speed = stats.chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(startPosition);

        if (_animator != null) _animator.SetFloat("speed", 1f);

        //Merkeze ulastiysa tekrar Patrol'a gec.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SwitchState(AIState.Patrol);
        }
    }

    // --- YARDIMCI METOTLAR ---

    //Dusmanin gozu: Aci ve Engel Kontrolcusu
    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        //Mesafe Kontrolu
        if (distanceToPlayer > stats.chaseRange) return false;

        //Gorus Acisi Kontrolu
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer < stats.fovAngle / 2f)
        {
            //Duvar/Engel Kontrolu
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, obstacleMask))
            {
                return true;//Oyuncu goruluyor.
            }
        }
        return false;
    }

    //NavMesh uzerinde rastgele gecerli bir nokta bulan metot
    private Vector3 GetRandomPoint(Vector3 center, float range)
    {
        //2d bir disk icinde rastgele nokta secilir.
        Vector2 randomCircle = Random.insideUnitCircle * range;

        Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
        randomDirection += center;

        NavMeshHit hit;
        //Secilen noktanin en fazla 2 birim ustunde/altinda gecerli bir NavMesh zemini var mi diye kontrol eder.
        if (NavMesh.SamplePosition(randomDirection, out hit, range/2, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center; // Bulamazsa oldugu yerde kalsin.
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    //SPAWNER'IN ÇAGIRDIGI BEYIN SIFIRLAMA METODU
    public void ResetAI()
    {
        currentState = AIState.Patrol;
        memoryTimer = 0f;
        patrolWaitTimer = 0f;

        startPosition = transform.position;

        if (agent != null && agent.isActiveAndEnabled)
        {
            //EGer zemine tutunabildiyse eski rotasini sil.
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                ExecuteCurrentState(); //Yeni duruma gore hareket etmeye basla.
                Debug.Log("Current state: " + currentState.ToString());
            }

            agent.isStopped = false;
            if (stats != null) agent.speed = stats.patrolSpeed;
        }
    }

    private void OnDrawGizmos()
    {
        if (stats == null) return;

        float activeRadius = (spawnerWanderRadius > 0) ? spawnerWanderRadius : stats.wanderRadius;

        //NPC Gezinme Kuresi
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, activeRadius);

        //Gorus Acisi Cizgileri
        if (currentState == AIState.Patrol)
            Gizmos.color = greenColor;
        else if (currentState == AIState.Chase)
            Gizmos.color = redColor;
        else if (currentState == AIState.Attack)
            Gizmos.color = redColor;
        else if (currentState == AIState.Return)
            Gizmos.color = yellowColor;
        Vector3 forward = transform.forward * stats.chaseRange;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-stats.fovAngle / 2f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(stats.fovAngle / 2f, Vector3.up);
        Quaternion middleRayRotation = Quaternion.AngleAxis(0, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;
        Vector3 middleRayDirection = middleRayRotation * forward;

        Gizmos.DrawRay(transform.position + Vector3.up, leftRayDirection);
        Gizmos.DrawRay(transform.position + Vector3.up, rightRayDirection);
        Gizmos.DrawRay(transform.position + Vector3.up, middleRayDirection);
    }
    public void SetAttackSpeed()
    {
        GetComponent<Animator>().SetFloat("NPCAttackSpeed", stats.attackSpeedMultiplier);
    }
}