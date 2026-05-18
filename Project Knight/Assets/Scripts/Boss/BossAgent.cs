using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BossAgent : Agent
{
    [Header("Referanslar")]
    public Transform playerTarget;

    private NavMeshAgent navAgent;
    private Animator animator;
    private BossHealth bossHealth;
    private BossAttackSystem attackSystem;
    private EnemyStats stats;

    // --- INPUT TAMPON (BUFFER) DEÐÝÞKENLERÝ ---
    private bool attackInput;
    private bool blockInput;
    private bool special1Input;
    private bool special2Input;
    private bool special3Input;
    private bool special4Input;

    private Vector3 startPos;

    [Header("Savaþ Ayarlarý")]
    public float basicAttackCooldown = 3f; // Temel saldýrý yasaklanma süresi
    private float nextBasicAttackTime = 0f;

    // --- YENÝ EKLENEN KÝLÝT TAKÝPÇÝLERÝ ---
    private bool wasActionLocked = false;
    private bool wasBasicAttack = false;

    public override void Initialize()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        attackSystem = GetComponent<BossAttackSystem>();

        if (bossHealth != null) stats = bossHealth.stats;

        startPos = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        // 1. Boss'un Canýný Sýfýrla
        if (bossHealth != null) bossHealth.ResetHealth();

        // 2. Kilidi Aç ve Animasyonu Idle'a al
        if (attackSystem != null) attackSystem.UnlockAction();
        if (animator != null) animator.Play("Idle");

        // 3. Boss'un Pozisyonunu Sýfýrla
        // Sabit doðmamasý için baþlangýç noktasýnýn etrafýnda ufak (-2, +2) rastgele bir noktada doðuruyoruz
        transform.localPosition = startPos;

        // 4. NavMesh Agent'ý sýfýrla (Eðer aktifse)
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }

        if (playerTarget != null)
        {
            DummyHealth dummyHealth = playerTarget.GetComponent<DummyHealth>();
            if (dummyHealth != null)
            {
                dummyHealth.ResetDummy();
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (playerTarget == null || stats == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        sensor.AddObservation(distanceToPlayer / stats.chaseRange);
        sensor.AddObservation(attackSystem.isActionLocked ? 1.0f : 0.0f);
        sensor.AddObservation((float)bossHealth.currentHealth / stats.maxHealth);
    }

    // --- YENÝ EKLENEN TAMPONLAMA SÝSTEMÝ ---
    void Update()
    {
        if (Keyboard.current == null) return;

        // Tuþlarý yakala (Heuristic okunana kadar hafýzada kalýr)
        if (Keyboard.current.spaceKey.wasPressedThisFrame) attackInput = true;
        if (Keyboard.current.bKey.wasPressedThisFrame) blockInput = true;

        // 4 Özel Saldýrý için klavyeden 1, 2, 3, 4 tuþlarý
        if (Keyboard.current.digit1Key.wasPressedThisFrame) special1Input = true;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) special2Input = true;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) special3Input = true;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) special4Input = true;

        // --- YENÝ: COOLDOWN'I SALDIRI BÝTTÝÐÝNDE BAÞLAT ---
        // Eðer Boss bir önceki karede kilitliyse ve ÞU AN kilit açýldýysa (Yani animasyon bittiyse)
        if (wasActionLocked && !attackSystem.isActionLocked)
        {
            if (wasBasicAttack)
            {
                // Temel saldýrý KESÝN OLARAK bitti, sayacý ÞÝMDÝ baþlat!
                nextBasicAttackTime = Time.time + basicAttackCooldown;
                wasBasicAttack = false; // Hafýzayý sýfýrla
            }
        }
        // Bir sonraki kare için kilit durumunu hafýzada tut
        wasActionLocked = attackSystem.isActionLocked;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // --- KÝLÝT KONTROLÜ VE KESÝN FREN ---
        // Eðer animasyon oynuyorsa hiçbir yeni emri dinleme, ama tamamen DURDUÐUNDAN emin ol!
        if (attackSystem.isActionLocked)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero; // Kaymayý ve patinajý kesin olarak kes
            }
            animator.SetFloat("speed", 0f); // Koþu animasyonunda takýlý kalmasýný engelle
            return;
        }

        // --- 1. HAREKET KARARLARI (Branch 0) ---
        int moveDecision = actions.DiscreteActions[0];
        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (moveDecision == 1 && distance > stats.attackRange)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
                navAgent.speed = stats.chaseSpeed;
                navAgent.SetDestination(playerTarget.position);
            }
            animator.SetFloat("speed", 1f);
        }
        else
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }
            animator.SetFloat("speed", 0f);
        }

        // --- YÖN DÖNME MANTIÐI ---
        if (distance <= stats.attackRange)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
        }
        else if (moveDecision == 1 && navAgent.isOnNavMesh && navAgent.hasPath)
        {
            Vector3 direction = (navAgent.steeringTarget - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
            }
        }

        // --- 2. SAVAÞ KARARLARI (Branch 1) ---
        int combatDecision = actions.DiscreteActions[1];

        // EÐER HERHANGÝ BÝR SAVAÞ HAMLESÝ YAPILACAKSA, ANINDA ZINK DÝYE DUR!
        if (combatDecision != 0)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }
            animator.SetFloat("speed", 0f);
        }

        // --- YENÝ HÝBRÝT SALDIRI SÝSTEMÝ (Þans & Zorunluluk) ---
        if (combatDecision == 1)
        {
            // Eðer temel saldýrý bekleme süresi BÝTTÝYSE
            if (Time.time >= nextBasicAttackTime)
            {
                animator.SetTrigger("attack");
                attackSystem.LockAction();
                wasBasicAttack = true;
            }
            // Eðer temel saldýrý BEKLEME SÜRESÝNDEYSE (AI gafil avlandý!)
            else
            {
                // Biz araya giriyoruz ve 1, 2, 3 veya 4 numaralý özel saldýrýlardan birini ZORLA yaptýrýyoruz.
                int randomSpecial = Random.Range(1, 5); // 1 ile 4 arasýnda sayý tutar
                TriggerSpecialAttack("specialAttack" + randomSpecial);
            }
        }
        else if (combatDecision == 2)
        {
            if (bossHealth.TryStartBlock())
            {
                // Blok zaten durduruldu
            }
        }
        // AI'ýn kafasý karýþmasýn diye diðer tuþlarý (3,4,5,6) tamamen göz ardý ediyoruz.
        // O sadece "1" tuþuna basacak, biz arkada þov yapacaðýz.
        /*else if (combatDecision == 3) { TriggerSpecialAttack("specialAttack1"); }
        else if (combatDecision == 4) { TriggerSpecialAttack("specialAttack2"); }
        else if (combatDecision == 5) { TriggerSpecialAttack("specialAttack3"); }
        else if (combatDecision == 6) { TriggerSpecialAttack("specialAttack4"); }*/
    }

    // Özel saldýrýlarý tetikleyen ve kilitleyen yardýmcý fonksiyon
    private void TriggerSpecialAttack(string triggerName)
    {
        animator.SetTrigger(triggerName);
        attackSystem.LockAction();
        wasBasicAttack = false; // Özel saldýrý yapýldý, temel saldýrý deðildi!
    }

    // Savaþ anýnda animasyonlarýn hazýrlýk evresinde oyuncuya dönmek için yardýmcý metot
    public void TrackPlayer(float speed)
    {
        if (playerTarget == null) return;

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0; // Yukarý/Aþaðý eðilmeyi engelle (Sadece X-Z ekseninde dön)

        if (direction != Vector3.zero)
        {
            // Update içinde çalýþýyormuþ gibi pürüzsüz dönüþ (Slerp)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * speed);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 0;
        discreteActions[1] = 0;

        if (Keyboard.current == null) return;

        // W Tuþu: Yürüme (Basýlý tutulduðu sürece)
        if (Keyboard.current.wKey.isPressed) discreteActions[0] = 1;

        // Savaþ komutlarý (Tampondan oku ve sil)
        if (attackInput) { discreteActions[1] = 1; attackInput = false; }
        else if (blockInput) { discreteActions[1] = 2; blockInput = false; }
        else if (special1Input) { discreteActions[1] = 3; special1Input = false; }
        else if (special2Input) { discreteActions[1] = 4; special2Input = false; }
        else if (special3Input) { discreteActions[1] = 5; special3Input = false; }
        else if (special4Input) { discreteActions[1] = 6; special4Input = false; }
    }
}