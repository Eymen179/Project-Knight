using UnityEngine;
using System.IO;

public class IconMaker : MonoBehaviour
{
    [Header("Ayarlar")]
    public string itemName = "Deneme";
    public int imageWidth = 512;
    public int imageHeight = 512;

    //Inspector'dan sag tiklayarak bu metot calistirilir.
    [ContextMenu("Ikonu Cek ve Kaydet")]
    public void TakeScreenshot()
    {
        Camera cam = GetComponent<Camera>();

        //Kameranin gorecegi bos bir tuval (RenderTexture) olustur.
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        cam.targetTexture = rt;

        //Tuvali seffafligi destekleyen (ARGB32) formata cevir.
        Texture2D screenShot = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);

        //Kameraya "Cek!" emrini ver.
        cam.Render();

        //Tuvaldeki pikselleri Texture2D'ye oku.
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);

        //Temizlik islemleri
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        //Resmi PNG'ye cevir ve projeye kaydet.
        byte[] bytes = screenShot.EncodeToPNG();
        string path = Application.dataPath + $"/Prefabs/Icons/{itemName}.png";
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Harika! Þeffaf ikon kaydedildi: {path}");
    }
}