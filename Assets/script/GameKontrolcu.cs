using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using Input = UnityEngine.Input;
using UnityStandardAssets.Characters.FirstPerson;


// Oyunun genel akýsýný, düþman yönetimýni, silah deðiþtirmeyi,
// saðlýk bomba sistemini ve menü geçiþlerini yöneten ana kontrol sýnýfý
public class GameKontrolcu : MonoBehaviour
{
    [Header("DÝÐER AYARLAR")]
    public GameObject GameOverCanvas;          // Oyun bitiþinde gösterilen "Game Over" ekraný
    public GameObject VictoryCanvas;           // Tüm düþmanlar öldurüldüðünde gösterilen zafer ekraný
    public GameObject PauseCanvas;             // Oyun duraklatýldýgýnda gösterilen pause menüsü
    public AudioSource OyunÝciSes;             // Arka plan müzigi,oyun içi ses
    public TextMeshProUGUI Saglik_Sayisi_Text; // Envanterdeki can sayýsýný gösteren UI metni
    public TextMeshProUGUI Bomba_Sayisi_Text;  // Envanterdeki bomba sayýsýný gösteren UI metni
    public AudioSource Itemyok;                // Item kullanýlmak istendiðinde ama yoksa çalan ses

    [Header("SAGLÝK AYARLAR")]
    float health = 100;      // Oyuncunun anlýk can puaný (0-100 arasý)
    public Image HealthBar;  // Can barý UI görseli (fillAmount ile güncellenir)


    [Header("SÝLAH AYARLARÝ")]
    public GameObject[] silahlar;    // Tüm silah nesnelerini tutan dizi (0:Taramalý, 1:Pompali, 2:Sniper, 3:Magnum)
    public AudioSource degisimsesi;  // Silah deðiþtirme sesi
    int aktifsira;                   // Þu an aktif olan silahýn dizi indeksi
    public GameObject Bomba;         // Fýrlatýlacak bomba prefabý
    public GameObject BombaPoint;    // Bombanýn fýrlatýlacaðý konum ve rotasyon noktasý
    public Camera Benimcam;          // Bomba fýrlatma yönünü hesaplamak için kullanýlan kamera

    [Header("DUSMAN AYARLARÝ")]
    public GameObject[] dusmanlar;        // Spawn edilebilecek farklý düþman prefablarý
    public GameObject[] cikisnoktalari;   // Düþmanlarýn çýkýþ (spawn) noktalarý
    public GameObject[] hedefnoktalari;   // Düþmanlarýn yürüyeceði hedef noktalarý
    public TextMeshProUGUI KalanDusman_Text; // Kalan düþman sayýsýný gösteren UI metni
    public int BaslangicDusmanSayisi;     // Sahneye toplam kaç düþman çýkarýlacaðý (Inspector'dan ayarlanýr)
    public static int KalanDusmanSayisi; // Sahnede hâlâ hayatta olan düþman sayýsý (statik — diðer scriptler eriþir)

    public static bool OyunDurdumu; // Oyunun duraklatýlýp duraklatýlmadýðýný tüm scriptlere bildiren global bayrak

    void Start()
    {
        Time.timeScale = 1f;       // Oyun baþlarken zamanýn normal hýzda akmasýný garantile
        BaslangicÝslemleri();      // Baþlangýç deðerlerini ve PlayerPrefs'i ayarla
        OyunÝciSes = GetComponent<AudioSource>();
        OyunÝciSes.Play();         // Arka plan müziðini baþlat
        HealthBar.fillAmount = 1;  // Can barýný tam dolu göster
        aktifsira = 0;             // Baþlangýç silahý olarak ilk silahý seç
        StartCoroutine(DusmanCikar()); // Düþman spawn döngüsünü baþlat
    }

    // Oyun baþlarken tüm baþlangýç deðerlerini sýfýrlar ve PlayerPrefs'e yazar
    void BaslangicÝslemleri()
    {
        OyunDurdumu = false;

        // Tüm silah mermi miktarlarýný baþlangýç deðerlerine sýfýrla
        PlayerPrefs.SetInt("Taramali_Mermi", 190);
        PlayerPrefs.SetInt("Pompali_Mermi", 50);
        PlayerPrefs.SetInt("Sniper_Mermi", 30);
        PlayerPrefs.SetInt("Magnum_Mermi", 30);

        // Saðlýk ve bomba envanterini sýfýrla
        PlayerPrefs.SetInt("Saglik_sayisi", 0);
        PlayerPrefs.SetInt("Bomba_sayisi", 0);

        // Oyunun baþladýðýný iþaretle (diðer scriptler bu deðeri kontrol edebilir)
        PlayerPrefs.SetInt("OyunBasladimi", 1);

        // Kalan düþman sayýsýný UI'a ve statik deðiþkene yaz
        KalanDusman_Text.text = BaslangicDusmanSayisi.ToString();
        KalanDusmanSayisi = BaslangicDusmanSayisi;

        // Saðlýk ve bomba sayýsýný UI'a yaz
        Saglik_Sayisi_Text.text = PlayerPrefs.GetInt("Saglik_sayisi").ToString();
        Bomba_Sayisi_Text.text = PlayerPrefs.GetInt("Bomba_sayisi").ToString();
    }

    // Her 2 saniyede bir rastgele düþman spawn eden sonsuz coroutine
    IEnumerator DusmanCikar()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // 2 saniye bekle

            if (BaslangicDusmanSayisi != 0) // Spawn edilecek düþman kaldýysa
            {
                // Rastgele düþman tipi, çýkýþ noktasý ve hedef noktasý seç
                int dusman = Random.Range(0, 5);
                int cikisnoktasi = Random.Range(0, 2);
                int hedefnoktasi = Random.Range(0, 2);

                // Düþmaný seçilen çýkýþ noktasýnda oluþtur ve hedefini belirle
                GameObject Obje = Instantiate(dusmanlar[dusman], cikisnoktalari[cikisnoktasi].transform.position, Quaternion.identity);
                Obje.GetComponent<Dusman>().HedefBelirle(hedefnoktalari[hedefnoktasi]);
                BaslangicDusmanSayisi--; // Spawn sayacýný azalt
            }
        }
    }

    // Bir düþman öldürüldüðünde Dusman scripti tarafýndan çaðrýlýr
    public void DusmanSayisiGuncelle()
    {
        KalanDusmanSayisi--;
        if (KalanDusmanSayisi <= 0) // Tüm düþmanlar öldürüldüyse
        {
            VictoryCanvas.SetActive(true); // Zafer ekranýný göster
            KalanDusman_Text.text = "0";
            Time.timeScale = 0;            // Zamaný durdur
        }
        else
        {
            KalanDusman_Text.text = KalanDusmanSayisi.ToString(); // UI'ý güncelle
        }
    }

    // Düþman saldýrýsý veya baþka bir hasar kaynaðý tarafýndan çaðrýlýr
    public void CanAzalt(float darbegucu)
    {
        health -= darbegucu;
        HealthBar.fillAmount = health / 100; // Can barýný güncelle
        if (health <= 0)
        {
            GameOver(); // Can sýfýrlandýysa oyunu bitir
        }
    }

    // H tuþuna basýldýðýnda envanterden bir can kutusu kullanýr
    public void Saglik_doldur()
    {
        // Can kutusu varsa VE can zaten 100 deðilse kullan
        if (PlayerPrefs.GetInt("Saglik_sayisi") != 0 && health != 100)
        {
            health = 100;
            HealthBar.fillAmount = health / 100;  // Can barýný tam doldur
            PlayerPrefs.SetInt("Saglik_sayisi", PlayerPrefs.GetInt("Saglik_sayisi") - 1); // Envanterden bir tane düþ
            Saglik_Sayisi_Text.text = PlayerPrefs.GetInt("Saglik_sayisi").ToString();     // UI'ý güncelle
        }
        else
        {
            ItemYok(); // Can kutusu yoksa veya can zaten doluysa ses çal
        }
    }

    // Can kutusu toplanýnca çaðrýlýr — envantere bir can kutusu ekler
    public void Saglik_Al()
    {
        PlayerPrefs.SetInt("Saglik_sayisi", PlayerPrefs.GetInt("Saglik_sayisi") + 1);
        Saglik_Sayisi_Text.text = PlayerPrefs.GetInt("Saglik_sayisi").ToString(); // UI'ý güncelle
    }

    void Update()
    {
        // 1-4 sayý tuþlarýyla doðrudan silah seçimi
        if (Input.GetKeyDown(KeyCode.Alpha1) && !OyunDurdumu)
        {
            SilahDegistir(0); // Taramalý
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !OyunDurdumu)
        {
            SilahDegistir(1); // Pompalý
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && !OyunDurdumu)
        {
            SilahDegistir(2); // Sniper
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && !OyunDurdumu)
        {
            SilahDegistir(3); // Magnum
        }

        // Q tuþuyla sýradaki silaha geç (döngüsel)
        if (Input.GetKeyDown(KeyCode.Q) && !OyunDurdumu)
        {
            QileSilahDegistir();
        }

        // G tuþuyla bomba fýrlat
        if (Input.GetKeyDown(KeyCode.G) && !OyunDurdumu)
        {
            BombaAt();
        }

        // H tuþuyla can doldur
        if (Input.GetKeyDown(KeyCode.H) && !OyunDurdumu)
        {
            Saglik_doldur();
        }

        // Escape tuþuyla oyunu duraklat
        if (Input.GetKeyDown(KeyCode.Escape) && !OyunDurdumu)
        {
            PauseMenu();
        }
    }

    // Envanterden bir bomba harcayarak ileri doðru fýrlatýr
    void BombaAt()
    {
        if (PlayerPrefs.GetInt("Bomba_sayisi") != 0) // Bomba varsa
        {
            GameObject obje = Instantiate(Bomba, BombaPoint.transform.position, BombaPoint.transform.rotation);
            Rigidbody rg = obje.GetComponent<Rigidbody>();
            // Kameranýn ileri yönüne 90 derece döndürülmüþ bir açý hesapla (yay hareketi için)
            Vector3 acimiz = Quaternion.AngleAxis(90, Benimcam.transform.forward) * Benimcam.transform.forward;
            rg.AddForce(acimiz * 250f); // Hesaplanan yönde kuvvet uygula
            PlayerPrefs.SetInt("Bomba_sayisi", PlayerPrefs.GetInt("Bomba_sayisi") - 1); // Envanterden bir bomba düþ
            Bomba_Sayisi_Text.text = PlayerPrefs.GetInt("Bomba_sayisi").ToString();      // UI'ý güncelle
        }
        else
        {
            ItemYok(); // Bomba yoksa ses çal
        }
    }

    // Bomba kutusu toplanýnca çaðrýlýr — envantere bir bomba ekler
    public void Bomba_Al()
    {
        PlayerPrefs.SetInt("Bomba_sayisi", PlayerPrefs.GetInt("Bomba_sayisi") + 1);
        Bomba_Sayisi_Text.text = PlayerPrefs.GetInt("Bomba_sayisi").ToString(); // UI'ý güncelle
    }

    // Verilen indeksteki silahý aktifleþtirir, diðerlerini gizler
    void SilahDegistir(int siranumarasi)
    {
        degisimsesi.Play();
        foreach (GameObject silah in silahlar)
        {
            silah.SetActive(false); // Tüm silahlarý gizle
        }
        aktifsira = siranumarasi;
        silahlar[siranumarasi].SetActive(true); // Seçilen silahý göster
    }

    // Q tuþuyla sýradaki silaha geçer; son silahtan sonra baþa döner (döngüsel)
    void QileSilahDegistir()
    {
        degisimsesi.Play();
        foreach (GameObject silah in silahlar)
        {
            silah.SetActive(false); // Tüm silahlarý gizle
        }
        if (aktifsira == 3) // Son silahtaysa baþa dön
        {
            silahlar[0].SetActive(true);
            aktifsira = 0;
        }
        else // Deðilse bir sonraki silaha geç
        {
            aktifsira++;
            silahlar[aktifsira].SetActive(true);
        }
    }

    // Kullanýlmak istenen item (can/bomba) mevcut deðilse ses çalar
    void ItemYok()
    {
        Itemyok.Play();
    }

    // Can sýfýrlandýðýnda çaðrýlýr — Game Over ekranýný açar ve oyunu durdurur
    void GameOver()
    {
        GameOverCanvas.SetActive(true);
        Time.timeScale = 0;    // Zamaný durdur
        OyunDurdumu = true;    // Global bayraðý ayarla
        Cursor.visible = true; // Ýmleci göster
        // FPS kontrolcüsünün fare kilidini kaldýr
        GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().m_MouseLook.lockCursor = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // Ana menü sahnesine döner (Build index 0)
    public void AnaMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Escape tuþuyla çaðrýlýr — oyunu duraklatýr ve pause menüsünü açar
    public void PauseMenu()
    {
        PauseCanvas.SetActive(true);
        Time.timeScale = 0;    // Zamaný durdur
        OyunDurdumu = true;    // Global bayraðý ayarla
        Cursor.visible = true; // Ýmleci göster
        // FPS kontrolcüsünün fare kilidini kaldýr
        GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().m_MouseLook.lockCursor = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // Pause menüsünden "Devam Et" butonuna basýlýnca çaðrýlýr
    public void Continue()
    {
        Cursor.visible = false; // Ýmleci gizle
        // FPS kontrolcüsünün fare kilidini geri aç
        GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().m_MouseLook.lockCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
        PauseCanvas.SetActive(false); // Pause menüsünü kapat
        Time.timeScale = 1;           // Zamaný devam ettir
        OyunDurdumu = false;         
    }

    // Game Over veya Pause menüsünden "Baþtan Baþla" butonuna basýlýnca çaðrýlýr
    public void BastanBasla()
    {
        Cursor.visible = false;
        GameObject.FindWithTag("Player").GetComponent<FirstPersonController>().m_MouseLook.lockCursor = true;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Mevcut sahneyi yeniden yükle
        Time.timeScale = 1;
        OyunDurdumu = false;
    }
}