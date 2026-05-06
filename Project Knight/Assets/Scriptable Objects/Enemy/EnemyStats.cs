using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName;
    public int maxHealth = 100;

    //Hiz degiskenleri
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Combat Stats")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;
    public float attackSpeedMultiplier = 1f;

    [Header("Boss Special Attacks")]
    public int[] specialAttackDamages;

    [Header("Block Stat")]
    [Range(0, 100)]
    public int blockChance = 30;

    [Header("Vision & AI Senses")]
    public float chaseRange = 10f;
    [Range(0, 360)]
    public float fovAngle = 120f;

    [Header("Behavior Settings")]
    public float wanderRadius = 10f;
    public float memoryTime = 4f;
}