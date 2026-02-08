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
            // YÖNTEM 1: Tamamen kameraya kilitli (En iyisi)
            // Canvas'ýn yönünü, kameranýn baktýðý yönle ayný yap.
            // Bu sayede UI her zaman ekran düzlemine paralel durur.
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}