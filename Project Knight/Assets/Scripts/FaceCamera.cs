using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Sahnedeki ana kamerayý bul
        mainCamera = Camera.main;
    }

    // Kamera hareket ettikten SONRA çalýþmasý için LateUpdate kullanýyoruz.
    // Update kullanýrsak, kamera hareket ettiðinde UI titreme (jitter) yapabilir.
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Kameranýn o anki dönüþ açýlarýný (Euler) alýyoruz
            Vector3 cameraAngles = mainCamera.transform.rotation.eulerAngles;

            // X (öne/arkaya eðilme) ve Z (saða/sola yatma) açýlarýný 0'da sabitliyoruz.
            // Sadece Y (kendi etrafýnda dönme) açýsýný kameraya eþitliyoruz.
            transform.rotation = Quaternion.Euler(0f, cameraAngles.y, 0f);
        }
    }
}