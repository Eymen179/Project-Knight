using UnityEngine;
using System.IO;

public class IconMaker : MonoBehaviour
{
    [Header("Ayarlar")]
    public string itemName = "Deneme"; // Kaydedilecek dosyanýn adý
    public int imageWidth = 512;         // Geniþlik
    public int imageHeight = 512;     // 512x512 HD ikon kalitesi

    // Inspector'dan sað týklayýp veya 3 noktaya basýp çalýþtýrabilmemizi saðlar
    [ContextMenu("Ikonu Cek ve Kaydet")]
    public void TakeScreenshot()
    {
        Camera cam = GetComponent<Camera>();

        // Kameranýn göreceði boþ bir tuval (RenderTexture) oluþtur
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;

        // Tuvali þeffaflýðý destekleyen (ARGB32) formata çevir
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);

        // Kameraya "Çek!" emrini ver
        cam.Render();

        // Tuvaldeki pikselleri Texture2D'ye oku
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);

        // Temizlik iþlemleri
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Resmi PNG'ye çevir ve projeye kaydet
        byte[] bytes = screenShot.EncodeToPNG();
        string path = Application.dataPath + $"/Prefabs/Icons/{itemName}.png";
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Harika! Þeffaf ikon kaydedildi: {path}");
    }
}