using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName;
    public int maxHealth = 100;
    // GÜNCELLENDÝ: Hýz ikiye ayrýldý
    public float patrolSpeed = 2f; // Devriye atarken yürüme hýzý
    public float chaseSpeed = 5f;  // Kovalarken ve merkeze dönerken koþma hýzý

    [Header("Combat Stats")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;  // Vurma menzili

    [Header("Vision & AI Senses")]
    public float chaseRange = 10f;  // Ne kadar uzaðý görebildiði
    [Range(0, 360)]
    public float fovAngle = 120f;   // Görüþ açýsý (Göz yanýlma payý, 120 derece idealdir)

    [Header("Behavior Settings")]
    public float wanderRadius = 10f; // Kendi bölgesinde ne kadar uzaða rastgele gideceði
    public float memoryTime = 4f;    // Oyuncuyu kaybedince kaç saniye daha arayacaðý/bekleyeceði
}