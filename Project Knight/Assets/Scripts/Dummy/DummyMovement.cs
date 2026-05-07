using UnityEngine;

public class DummyMovement : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public Transform[] waypoints;   // A, B, C, D (veya daha fazla) noktalarýný buraya sürükle
    public float moveSpeed = 4f;    // Kuklanýn koþma hýzý

    private Transform currentTarget;

    void Start()
    {
        // Oyun baþlar baþlamaz kendine rastgele bir hedef seç
        PickRandomWaypoint();
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || currentTarget == null) return;

        // Vector3.MoveTowards ile hedefe doðru sabit bir hýzla yürü
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        // Kuklanýn yüzünü gittiði yöne doðru çevir (Moonwalk yapmasýn)
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0; // Y eksenini sýfýrla ki karakter yere baksýn/eðilmesin
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }

        // Hedefe ulaþtýk mý? (0.1f gibi küçük bir hata payý býrakmak her zaman iyidir)
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            PickRandomWaypoint();
        }
    }

    private void PickRandomWaypoint()
    {
        if (waypoints.Length == 0) return;

        Transform newTarget = currentTarget;

        // Ayný noktayý (örneðin A'dayken tekrar A'yý) seçmemesi için ufak bir döngü
        // Eðer sahnede sadece 1 nokta varsa sonsuz döngüye girmemesi için güvenlik önlemi
        if (waypoints.Length > 1)
        {
            while (newTarget == currentTarget)
            {
                newTarget = waypoints[Random.Range(0, waypoints.Length)];
            }
        }
        else
        {
            newTarget = waypoints[0];
        }

        currentTarget = newTarget;
    }

    // ML-Agent her turu sýfýrladýðýnda kuklayý da rastgele bir noktaya ýþýnlamak için
    public void ResetMovement()
    {
        if (waypoints.Length > 0)
        {
            // Kuklayý rastgele bir noktanýn üzerine ýþýnla
            Transform randomStartPoint = waypoints[Random.Range(0, waypoints.Length)];
            transform.position = randomStartPoint.position;

            // Sonra kendine gitmek için YENÝ bir hedef seç
            PickRandomWaypoint();
        }
    }
}