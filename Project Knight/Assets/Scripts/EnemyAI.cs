using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Attack, Return }

    [Header("Referanslar & Ayarlar")]
    public EnemyStats stats;
    public AIState currentState = AIState.Patrol;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private Animator _animator;

    // Hafýza ve Bölge Deðiþkenleri
    private Vector3 startPosition;       // Doðduðu/Beklediði merkez nokta
    private float memoryTimer;           // Oyuncuyu görmediðinde geri sayan sayaç
    private float patrolWaitTimer;       // Rastgele gezinirken bekleme süresi

    // Görüþ (Line of Sight) için katman ayarý (Duvarlarýn arkasýný görmemesi için)
    // Eðer bir harita eklersen duvarlarý "Obstacle" gibi bir katmana alabilirsin.
    [SerializeField] private LayerMask obstacleMask;

    public Color redColor;
    public Color greenColor;
    public Color yellowColor;

    private EnemyAttackSystem attackSystem;
    private float nextAttackTime = 0f; // Saldýrý bekleme süresi (Cooldown) için sayaç

    private bool isBlocking = false;
    private float blockTimer = 0f;

    private EnemyHealth enemyHealth;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (stats != null)
        {
            agent.speed = stats.patrolSpeed;
        }

        // Merkez konumunu kaydet
        startPosition = transform.position;

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) playerTarget = player.transform;

        attackSystem = GetComponent<EnemyAttackSystem>();
    }

    void Update()
    {
        if (playerTarget == null || stats == null) return;

        // 1. Oyuncuyu Görüyor mu? (Açý, Mesafe ve Duvar Kontrolü)
        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- DURUM MAKÝNESÝ (STATE MACHINE) ---

        if (canSeePlayer)
        {
            // Oyuncuyu görüyorsa hafýzayý tazele ve saldýrmaya/kovalamaya baþla
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
            // Oyuncuyu GÖRMÜYORSA
            if (currentState == AIState.Chase || currentState == AIState.Attack)
            {
                // Hafýza süresi bitene kadar takip etmeye çalýþ
                memoryTimer -= Time.deltaTime;
                if (memoryTimer <= 0)
                {
                    // Hafýza bitti, pes et ve merkeze dön
                    SwitchState(AIState.Return);
                }
            }
            if(currentState == AIState.Patrol && enemyHealth.currentHealth != enemyHealth.healthBeforeDamage)
            {
                // Devriye halindeyken oyuncuyu görmezse rastgele gezinmeye devam et
                SwitchState(AIState.Chase);
            }
        }

        // Bulunduðumuz duruma göre eylemleri yap
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
        // Devriye atarken yürüme hýzýna geç
        agent.speed = stats.patrolSpeed;

        // Hedefe ulaþtýysa veya hiç hedefi yoksa bekle
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Beklerken (Hareketsizken) Idle animasyonu
            if (_animator != null) _animator.SetFloat("speed", 0f);

            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0)
            {
                // Yeni noktaya doðru yürümeye baþla
                Vector3 randomPoint = GetRandomPoint(startPosition, stats.wanderRadius);
                agent.SetDestination(randomPoint);
                agent.isStopped = false;
                patrolWaitTimer = Random.Range(2f, 5f);
            }
        }
        else
        {
            // Yürüyüþ halindeyse Walk animasyonu (0.5 deðeri Walk'u tetikler)
            if (_animator != null) _animator.SetFloat("speed", 0.5f);
        }
    }

    private void ChaseBehavior()
    {
        // Eðer NPC blok yaparken sen menzilden kaçarsan, bloku hemen indirsin
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

        // 1. DURUM: Eðer NPC þu an gardýný almýþ (Blok) durumdaysa
        if (isBlocking)
        {
            blockTimer -= Time.deltaTime; // Blok süresinden düþ

            if (blockTimer <= 0)
            {
                // Süre doldu, gardýný indir
                isBlocking = false;
                if (_animator != null) _animator.SetBool("isBlocking", false);

                // Blok bitince anýnda saldýrmasýn diye yarým saniye nefes alma payý
                nextAttackTime = Time.time + 0.5f;
            }
            return; // Blok halindeyken aþaðýdaki saldýrý kodlarýný OKUMA
        }

        // 2. DURUM: Bekleme süresi bitti, yeni bir hamle yapma vakti
        if (Time.time >= nextAttackTime)
        {
            // KARAR ANI: Rastgele bir sayý tut (0-100 arasý)
            int decision = Random.Range(0, 100);

            if (decision < stats.blockChance)
            {
                // --- BLOK YAPMAYA KARAR VERDÝ ---
                isBlocking = true;
                blockTimer = Random.Range(1.5f, 3.5f); // 1.5 ile 3.5 saniye arasý blokta kalacak
                if (_animator != null) _animator.SetBool("isBlocking", true);
            }
            else
            {
                // --- SALDIRMAYA KARAR VERDÝ ---
                if (_animator != null) _animator.SetTrigger("attack");
                nextAttackTime = Time.time + stats.attackCooldown;
            }
        }
    }

    private void ReturnBehavior()
    {
        // Merkeze dönerken koþma hýzýna geç
        agent.speed = stats.chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(startPosition);

        // Run (Koþma) animasyonunu oynat
        if (_animator != null) _animator.SetFloat("speed", 1f);

        // Merkeze ulaþtýysa tekrar devriyeye (Patrol) baþla
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SwitchState(AIState.Patrol);
        }
    }

    // --- YARDIMCI METOTLAR ---

    // Düþmanýn gözü: Açý ve Engel kontrolü
    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // 1. Mesafe Kontrolü
        if (distanceToPlayer > stats.chaseRange) return false;

        // 2. Görüþ Açýsý Kontrolü (Önündeki x derecelik koni içinde mi?)
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer < stats.fovAngle / 2f)
        {
            // 3. Duvar/Engel Kontrolü (Raycast ile)
            // Lazer ýþýnýný düþmanýn göz hizasýndan (Vector3.up) atýyoruz
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distanceToPlayer, obstacleMask))
            {
                // Çarpýþma yoksa oyuncuyu net görüyor demektir
                return true;
            }
        }
        return false;
    }

    // NavMesh üzerinde rastgele geçerli bir nokta bulur
    private Vector3 GetRandomPoint(Vector3 center, float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += center;

        NavMeshHit hit;
        // Seçilen rastgele nokta NavMesh'e uygun mu diye kontrol et (SamplePosition)
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center; // Bulamazsa olduðu yerde kalsýn
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    // Test ortamýnda görüþ açýsýný (FOV Konisi) ve Devriye alanýný çizdirelim
    private void OnDrawGizmos()
    {
        if (stats == null) return;

        // Devriye (Wander) Alaný
        Gizmos.color = new Color(0, 1, 0, 0.3f); // Yarý saydam yeþil
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, stats.wanderRadius);

        // Görüþ Açýsý Çizgileri
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
}