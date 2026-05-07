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
    }

    /* public override void OnActionReceived(ActionBuffers actions)
     {
         // KÝLÝT KONTROLÜ: Animasyon oynuyorsa hiçbir yeni emri dinleme!
         if (attackSystem.isActionLocked)
         {
             if (navAgent.isOnNavMesh) navAgent.isStopped = true;
             return;
         }

 // --- 1. HAREKET KARARLARI (Branch 0) ---
         int moveDecision = actions.DiscreteActions[0];
         float distance = Vector3.Distance(transform.position, playerTarget.position);

         if (moveDecision == 1 && distance > stats.attackRange) 
         {
             if(navAgent.isOnNavMesh)
             {
                 navAgent.isStopped = false;
                 navAgent.speed = stats.chaseSpeed;
                 navAgent.SetDestination(playerTarget.position);
             }
             animator.SetFloat("speed", 1f);
         }
         else 
         {
             if(navAgent.isOnNavMesh) 
             {
                 navAgent.isStopped = true;
                 navAgent.velocity = Vector3.zero; // YENÝ: W býrakýldýðýnda patinajý/kaymayý anýnda kes!
             }
             animator.SetFloat("speed", 0f);
         }

         // --- YENÝ DÖNÜÞ (ROTATION) MANTIÐI ---
         if (distance <= stats.attackRange)
         {
             // Sadece menzile girdiðinde DÝREKT oyuncuya bak (Kýlýç vurmak için)
             Vector3 direction = (playerTarget.position - transform.position).normalized;
             direction.y = 0;
             if (direction != Vector3.zero)
             {
                 transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
             }
         }
         else if (moveDecision == 1 && navAgent.isOnNavMesh && navAgent.hasPath)
         {
             // Uzaktayken duvarýn arkasýndaki oyuncuya DEÐÝL, yolun bir sonraki adýmýna (köþeye) bak!
             Vector3 direction = (navAgent.steeringTarget - transform.position).normalized;
             direction.y = 0;
             if (direction != Vector3.zero)
             {
                 transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
             }
         }

         // --- 2. SAVAÞ KARARLARI (Branch 1) ---
         int combatDecision = actions.DiscreteActions[1];

         // NOT: Artýk "distance <= stats.attackRange" kýsýtlamasý yok. Ýsterse havaya vurabilir.
         if (combatDecision == 1)
         {
             animator.SetTrigger("attack"); // Temel kombo tetikleyicisi
             attackSystem.LockAction();
         }
         else if (combatDecision == 2)
         {
             if (bossHealth.TryStartBlock())
             {
                 if (navAgent.isOnNavMesh) navAgent.isStopped = true;
                 animator.SetFloat("speed", 0f);
             }
         }
         else if (combatDecision == 3) { TriggerSpecialAttack("specialAttack1"); }
         else if (combatDecision == 4) { TriggerSpecialAttack("specialAttack2"); }
         else if (combatDecision == 5) { TriggerSpecialAttack("specialAttack3"); }
         else if (combatDecision == 6) { TriggerSpecialAttack("specialAttack4"); }
     }*/
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

        // EÐER HERHANGÝ BÝR SAVAÞ HAMLESÝ YAPILACAKSA, SALDIRMADAN ÖNCE ANINDA ZINK DÝYE DUR!
        if (combatDecision != 0)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }
            animator.SetFloat("speed", 0f);
        }

        if (combatDecision == 1)
        {
            animator.SetTrigger("attack");
            attackSystem.LockAction();
        }
        else if (combatDecision == 2)
        {
            if (bossHealth.TryStartBlock())
            {
                // Blok zaten yukarýdaki blokta durduruldu
            }
        }
        else if (combatDecision == 3) { TriggerSpecialAttack("specialAttack1"); }
        else if (combatDecision == 4) { TriggerSpecialAttack("specialAttack2"); }
        else if (combatDecision == 5) { TriggerSpecialAttack("specialAttack3"); }
        else if (combatDecision == 6) { TriggerSpecialAttack("specialAttack4"); }
    }

    // Özel saldýrýlarý tetikleyen ve kilitleyen yardýmcý fonksiyon
    private void TriggerSpecialAttack(string triggerName)
    {
        animator.SetTrigger(triggerName);
        attackSystem.LockAction();
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