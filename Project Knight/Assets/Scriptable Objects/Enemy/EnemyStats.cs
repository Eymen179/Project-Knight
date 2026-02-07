using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Scriptable Objects/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Base Stats")]
    public string enemyName;
    public int maxHealth = 100;
    public float moveSpeed = 3f;
    // Ýleride buraya saldýrý gücü vb. eklenebilir.
}
