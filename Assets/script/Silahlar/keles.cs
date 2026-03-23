using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

using UnityEngine;

// Silah sistemini yöneten ana sýnýf
public class keles : MonoBehaviour
{
    Animator animatorum; // Silahýn animasyon bileþeni

    [Header("OBJECTPOOLKOVANAYARLAR")]
    private const int KOVAN_POOL_SÝZE = 30; // Havuzda tutulacak maksimum kovan sayýsý
    public GameObject KovanPrefab;           // Kovan nesnesi prefabý
    public Transform Kovan_Point_Transform;  // Kovanýn fýrlatýlacagi nokta
    private Queue<GameObject> KovanPool;     // Kovan nesne havuzu (Queue yapýsý)

    [Header("OBJECTPOOLMERMÝAYARLAR")]
    private const int POOL_SÝZE = 30;       // Havuzda tutulacak maksimum mermi sayýsý

    public GameObject bullet_prefab;         // Mermi nesnesi prefabý
    public Transform Bullet_Point_transform; // Merminin çýkacaðý nokta (namlu ucu)
    public float Bulletspeed;               // Merminin hareket hýzý
    private Queue<GameObject> BulletPool;   // Mermi nesne havuzu (Queue yapýsý)


    [Header("AYARLAR")]
    public bool atesedebilirmi;             // Silahýn ateþ edip edemeyeceðini belirler
    public float disaridanAtesetmesiklik;   // Inspector'dan ayarlanan ateþ aralýðý (saniye)
    float iceridenatesetmesikligi;          // Bir sonraký ateþ zamanýný tutan iç deðiþken
    public float menzil;                    // Raycast'ýn gidebileceði maksimum mesafe
    public ParticleSystem efektim;          // Namlu alevi / duman efekti
    bool zoomvarmi;                         // Oyuncunun þu an zoom yapýp yapmadýðýný tutar
    public GameObject CrossHair;            // Ekrandaki crosshair izi 

    [Header("SESLER")]
    public AudioSource silahsesi;           // Ateþ etme sesi
    public AudioSource sarjordegistirme;    // Þarjör deðiþtirme sesi
    public AudioSource MermiBittiSesi;      // Mermi bittiðinde çýkan ses
    public AudioSource MermiAlmaSesi;       // Mermi kutusu alýndýðýnda çýkan ses


    [Header("Efektler")]
    public ParticleSystem Kanizi;           // Duþmana isabet halinde kan efekti
    public ParticleSystem Mermisicrama;     // Yüzeye isabet halinde mermi sýçrama efekti
    public ParticleSystem Mermiizi;         // Yüzeyde býrakýlan mermi izi efekti


    [Header("DÝÐERLERÝ")]
    public Camera benimcamim;              // Oyuncu kamerasý (Raycast ve zoom için)
    float FieldCamPos;                     // Kameranýn baþlangýç Field of View deðeri


    [Header("SÝLAH AYARLAR")]
    int toplammermiSayisi;                         // Envanterdeki toplam mermi sayýsý
    public int SarjorKapasite;                     // Þarjörün maksimum mermi kapasitesi
    int KalanMermiSayisi;                          // Þarjörde kalan mermi sayýsý
    public string Silahin_adi;                     // PlayerPrefs'te kullanýlacak silah adý anahtarý
    public TextMeshProUGUI ToplamMermi_text;        // Toplam mermiyi gösteren UI metni
    public TextMeshProUGUI KalanMermi_text;         // Kalan mermiyi gösteren UI metni
    public float DarbeGucu;                        // Düþmana verilen hasar miktarý

    public bool kovan_ciksinmi;                    // Ateþ edince kovan çýkýp çýkmayacaðýný belirler


    bool sarjordolduruyomu = true;                 // Þarjör doldurma iþleminin aktif olup olmadýðý

    public Mermi_Kutusu_Olustur Mermi_Kutusu_Olusturma_Yonetim; // Mermi kutusu yönetim scripti referansý


    void Start()
    {
        // Oyun baþladýðýnda PlayerPrefs'ten kaydedilmiþ mermi sayýsýný yükle
        toplammermiSayisi = PlayerPrefs.GetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
        kovan_ciksinmi = true;
        Baslangic_mermi_doldur();                  // Baþlangýçta þarjörü doldur
        MermiDoldurmaFonksiyon("NormalYazma");     // UI metnini güncelle
        FieldCamPos = benimcamim.fieldOfView;      // Varsayýlan FOV deðerini kaydet
        animatorum = GetComponent<Animator>();     // Animator bileþenini al
        BulletPoolBaslangicÝslemleri();            // Mermý havuzunu hazýrla
        KovanPoolBaslangicÝslemler();              // Kovan havuzunu hazýrla
    }

    // Baþlangýçta belirli sayýda kovan oluþturup havuza ekler
    void KovanPoolBaslangicÝslemler()
    {
        KovanPool = new Queue<GameObject>();
        for (int i = 0; i < KOVAN_POOL_SÝZE; i++)
        {
            GameObject Kovan = Instantiate(KovanPrefab, Vector3.zero, Quaternion.identity);
            Kovan.SetActive(false); // Baþlangýçta gizli tut
            KovanPool.Enqueue(Kovan);
        }
    }

    // Baþlangýçta belirli sayýda mermi oluþturup havuza ekler
    void BulletPoolBaslangicÝslemleri()
    {
        BulletPool = new Queue<GameObject>();
        for (int i = 0; i < POOL_SÝZE; i++)
        {
            GameObject bullet = Instantiate(bullet_prefab, Vector3.zero, Quaternion.identity);
            bullet.SetActive(false); // Baþlangýçta gizli tut
            BulletPool.Enqueue(bullet);
        }

    }

    void Update()
    {
        // Sol týk basýlý ve sað týk basýlý DEÐÝLSE (normal ateþ modu)
        if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
        {
            // Ateþ edebilir mi, bekleme süresi geçti mi ve mermi var mý?
            if (atesedebilirmi && Time.time > iceridenatesetmesikligi && KalanMermiSayisi != 0)
            {
                if (!GameKontrolcu.OyunDurdumu) // Oyun duraklatýlmamýþsa ateþ et
                {
                    Ateset(false); // Zoom olmadan ateþ et
                    iceridenatesetmesikligi = disaridanAtesetmesiklik + Time.time; // Sonraki ateþ zamanýný ayarla

                    // Havuzdan bir mermi nesnesi al
                    GameObject bullet = GetBulletFromPool();

                    if (bullet != null)
                    {
                        bullet.transform.position = Bullet_Point_transform.position; // Mermiyi namlu ucuna taþý

                        bullet.SetActive(true);
                        Rigidbody rb = bullet.GetComponent<Rigidbody>();
                        rb.linearVelocity = Bullet_Point_transform.forward * Bulletspeed; // Mermiyi ileri fýrlat

                        StartCoroutine(DisableBulletAfterDelay(bullet, 2f)); // 2 saniye sonra mermiyi havuza geri döndür
                    }
                }
            }
            // Mermi bittiyse ses çal
            if (KalanMermiSayisi == 0)
            {
                MermiBittiSesi.Play();
            }
        }

        // R tuþuna basýldýysa veya mermi bittiyse ve þarjör doldurmak mümkünse
        if ((Input.GetKey(KeyCode.R) || KalanMermiSayisi == 0) && sarjordolduruyomu)
        {
            benimcamim.fieldOfView = FieldCamPos; // Zoom varsa normal görüþe dön
            // Þarjör dolmamýþsa ve toplam mermi varsa yenileme animasyonunu baþlat
            if (KalanMermiSayisi < SarjorKapasite && toplammermiSayisi != 0)
            {
                animatorum.Play("sarjordegistir");
            }
        }

        // E tuþuna basýldýysa Raycast ile önündeki mermi kutusunu al
        if (Input.GetKeyDown(KeyCode.E))
        {
            MermiAl();
        }

        // Sað týkla zoom baþlat
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animatorum.SetBool("zoomyap", true);
            zoomvarmi = true;
        }

        // Sað týk býrakýldýðýnda zoom'u kapat ve normal görüþe dön
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            CrossHair.SetActive(true);           // Crosshair'i tekrar göster
            zoomvarmi = false;
            animatorum.SetBool("zoomyap", false);
            benimcamim.fieldOfView = FieldCamPos; // FOV'u sýfýrla
        }

        // Zoom aktifken sol týkla ateþ et
        if (zoomvarmi)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (atesedebilirmi && Time.time > iceridenatesetmesikligi && KalanMermiSayisi != 0)
                {
                    Ateset(true); // Zoom ile ateþ et
                    iceridenatesetmesikligi = disaridanAtesetmesiklik + Time.time;
                }
                if (KalanMermiSayisi == 0)
                {
                    MermiBittiSesi.Play();
                }
            }
        }
    }

    // Trigger alanýna giren nesneleri kontrol eder 
    private void OnTriggerEnter(Collider other)
    {
        // Mermi kutusuna çarpýldýysa
        if (other.gameObject.CompareTag("Mermi"))
        {
            MermiKaydet(other.transform.gameObject.GetComponent<mermikutusu>().Olusan_SilahinTuru, other.transform.gameObject.GetComponent<mermikutusu>().Olusan_MermiSayisi);
            Mermi_Kutusu_Olusturma_Yonetim.NoktalariKaldirma(other.transform.gameObject.GetComponent<mermikutusu>().Noktasi); // Spawn noktasýný serbest býrak
            Destroy(other.transform.parent.gameObject); // Mermi kutusunu yok et
        }

        // Can kutusuna çarpýldýysa
        if (other.gameObject.CompareTag("Cankutusu"))
        {
            Mermi_Kutusu_Olusturma_Yonetim.GetComponent<GameKontrolcu>().Saglik_Al(); // Oyuncuya can ver
            Health_Kutusu_Olustur.Health_Kutusu_Varmi = false;
            Destroy(other.transform.gameObject);
        }

        // Bomba kutusuna çarpýldýysa
        if (other.gameObject.CompareTag("BombaKutusu"))
        {
            Mermi_Kutusu_Olusturma_Yonetim.GetComponent<GameKontrolcu>().Bomba_Al(); // Oyuncuya bomba ver
            Bomba_Kutusu_Olustur.Bomba_Kutusu_Varmi = false;
            Destroy(other.transform.gameObject);
        }
    }


    // Ateþ etme iþleminin tüm mantýðýný yürütür
    void Ateset(bool yakinlasmavarmi)
    {
        AtesEtmeteknikÝslemleri(yakinlasmavarmi); // Ses, efekt, animasyon, kovan iþlemleri

        RaycastHit hit;

        // Kamera merkezinden ileriye Raycast gönder
        if (Physics.Raycast(benimcamim.transform.position, benimcamim.transform.forward, out hit, menzil))
        {
            // Düþmana isabet ettiyse
            if (hit.transform.gameObject.CompareTag("Dusman"))
            {
                Instantiate(Kanizi, hit.point, Quaternion.LookRotation(hit.normal)); // Kan efekti oluþtur
                hit.transform.GetComponent<Dusman>().Darbeal(DarbeGucu);              // Düþmana hasar ver
            }
            // Devrilebilir nesneye isabet ettiyse
            else if (hit.transform.gameObject.CompareTag("devrilebilirobje"))
            {
                Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                rb.AddForce((-hit.normal) * 50f); // Çarpma yönünde kuvvet uygula
            }
            // Diðer yüzeylere isabet ettiyse (duvar, zemin vb.)
            else
            {
                Instantiate(Mermiizi, hit.point, Quaternion.LookRotation(hit.normal));      // Mermi izi efekti
                Instantiate(Mermisicrama, hit.point, Quaternion.LookRotation(hit.normal));  // Sýçrama efekti
            }
        }

        KalanMermiSayisi--;                             // Bir mermi harca
        KalanMermi_text.text = KalanMermiSayisi.ToString(); // UI'ý güncelle
    }

    // Oyun baþlarken þarjörü mevcut mermiyle doldurur
    void Baslangic_mermi_doldur()
    {
        if (toplammermiSayisi <= SarjorKapasite)
        {
            // Toplam mermi þarjör kapasitesinden azsa hepsini þarjöre doldur
            KalanMermiSayisi = toplammermiSayisi;
            KalanMermiSayisi += toplammermiSayisi;
            toplammermiSayisi = 0;
            PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
            MermiDoldurmaFonksiyon("NormalYazma");
        }
        else
        {
            // Toplam mermi yeterliyse þarjörü tam doldur, kalaný envanterde býrak
            KalanMermiSayisi = SarjorKapasite;
            toplammermiSayisi -= SarjorKapasite;
            MermiDoldurmaFonksiyon("NormalYazma");
        }
    }

    // E tuþu veya tetik alanýyla mermi kutusunu Raycast ile alýr
    void MermiAl()
    {
        RaycastHit Hit;
        // 4 birim uzaklýktaki mermi kutusunu kontrol et
        if (Physics.Raycast(benimcamim.transform.position, benimcamim.transform.forward, out Hit, 4f))
        {
            if (Hit.transform.gameObject.CompareTag("Mermi"))
            {
                MermiKaydet(Hit.transform.gameObject.GetComponent<mermikutusu>().Olusan_SilahinTuru, Hit.transform.gameObject.GetComponent<mermikutusu>().Olusan_MermiSayisi);
                Mermi_Kutusu_Olusturma_Yonetim.NoktalariKaldirma(Hit.transform.gameObject.GetComponent<mermikutusu>().Noktasi);
                Destroy(Hit.transform.parent.gameObject);
            }
        }
    }

    // Þarjör doldurma durumuna göre mermi sayýsýný günceller ve UI'ý yeniler
    void MermiDoldurmaFonksiyon(string tur)
    {
        switch (tur)
        {
            case "MermiVar": // Þarjörde mermi varken doldurma
                if (toplammermiSayisi <= SarjorKapasite)
                {
                    int OlusanToplamDeger = KalanMermiSayisi + toplammermiSayisi;

                    if (OlusanToplamDeger > SarjorKapasite)
                    {
                        // Toplam kapasite aþýlýyorsa þarjörü doldur, fazlayý envantere býrak
                        KalanMermiSayisi = SarjorKapasite;
                        toplammermiSayisi = OlusanToplamDeger - SarjorKapasite;
                        PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
                    }
                    else
                    {
                        // Toplam kapasite aþýlmýyorsa hepsini þarjöre doldur
                        KalanMermiSayisi += toplammermiSayisi;
                        toplammermiSayisi = 0;
                        PlayerPrefs.SetInt(Silahin_adi + "_Mermi", 0);
                    }
                }
                else
                {
                    // Envanter kapasiteden fazlaysa þarjörü tam doldur
                    toplammermiSayisi -= SarjorKapasite - KalanMermiSayisi;
                    KalanMermiSayisi = SarjorKapasite;
                    PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
                }

                KalanMermi_text.text = KalanMermiSayisi.ToString();
                ToplamMermi_text.text = toplammermiSayisi.ToString();
                break;

            case "MermiYok": // Þarjör tamamen boþken doldurma
                if (toplammermiSayisi <= SarjorKapasite)
                {
                    // Envanterdeki tüm mermiler þarjöre girer
                    KalanMermiSayisi = toplammermiSayisi;
                    toplammermiSayisi = 0;
                    PlayerPrefs.SetInt(Silahin_adi + "_Mermi", 0);
                }
                else
                {
                    // Þarjörü tam doldur, kalaný envanterde tut
                    toplammermiSayisi -= SarjorKapasite;
                    KalanMermiSayisi = SarjorKapasite;
                    PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
                }

                KalanMermi_text.text = KalanMermiSayisi.ToString();
                ToplamMermi_text.text = toplammermiSayisi.ToString();
                break;

            case "NormalYazma": // Sadece UI metnini güncelle, hesaplama yapma
                ToplamMermi_text.text = toplammermiSayisi.ToString();
                KalanMermi_text.text = KalanMermiSayisi.ToString();
                break;
        }
    }

    // Animasyon eventi tarafýndan çaðrýlýr — þarjörü deðiþtirir
    void sarjordegistir()
    {
        if (!sarjordegistirme.isPlaying)
            sarjordegistirme.Play(); // Þarjör sesi çal

        // Þarjör dolmamýþsa ve mermi varsa doldurmayý yap
        if (KalanMermiSayisi < SarjorKapasite && toplammermiSayisi != 0)
        {
            if (KalanMermiSayisi != 0)
            {
                MermiDoldurmaFonksiyon("MermiVar"); // Þarjörde mermi varken doldur
            }
            else
            {
                MermiDoldurmaFonksiyon("MermiYok"); // Þarjör tamamen boþken doldur
            }
        }
    }

    // Ateþ etmenin teknik yan iþlemlerini yapar: kovan, ses, efekt, animasyon
    void AtesEtmeteknikÝslemleri(bool yakinlasmavarmi)
    {
        if (kovan_ciksinmi)
        {
            GameObject kovan = GetKovanFromPool(); // Havuzdan kovan al
            if (kovan != null)
            {
                kovan.transform.position = Kovan_Point_Transform.position; // Kovaný doðru noktaya taþý
                kovan.SetActive(true);
                Rigidbody rb = kovan.GetComponent<Rigidbody>();
                rb.AddRelativeForce(new Vector3(300f, 1, 0)); // Kovana yan kuvvet uygula (fýrlatma)

                StartCoroutine(DisableKovanAfterDelay(kovan, 1f)); // 1 saniye sonra kovani havuza geri döndür
            }
        }

        silahsesi.Play(); // Silah ateþ sesi çal
        efektim.Play();   // Namlu alevi efektini baþlat

        // Zoom durumuna göre farklý animasyon oynat
        if (!yakinlasmavarmi)
        {
            animatorum.Play("ateset");        // Normal ateþ animasyonu
        }
        if (yakinlasmavarmi)
        {
            animatorum.Play("zoomveateset"); // Zoom ile ateþ animasyonu
        }
    }

    // Alýnan mermi kutusunun türüne göre uygun PlayerPrefs deðerini artýrýr
    void MermiKaydet(string silahturu, int mermisayisi)
    {
        MermiAlmaSesi.Play();
        switch (silahturu)
        {
            case "Taramali":
                toplammermiSayisi += mermisayisi;
                PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
                ToplamMermi_text.text = toplammermiSayisi.ToString();
                break;
            case "Pompali":
                PlayerPrefs.SetInt("Pompali_Mermi", PlayerPrefs.GetInt("Pompali_Mermi") + mermisayisi);
                break;
            case "Sniper":
                PlayerPrefs.SetInt("Sniper_Mermi", PlayerPrefs.GetInt("Sniper_Mermi") + mermisayisi);
                break;
            case "Magnum":
                PlayerPrefs.SetInt("Magnum_Mermi", PlayerPrefs.GetInt("Magnum_Mermi") + mermisayisi);
                break;
        }
    }

    // Zoom animasyonu baþladýðýnda crosshair'i gizler
    void ScopAcma()
    {
        CrossHair.SetActive(false);
    }

    // Mermi havuzundan bir mermi nesnesi alýr ,havuz boþsa null döner
    private GameObject GetBulletFromPool()
    {
        if (BulletPool.Count > 0)
        {
            GameObject bullet = BulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        return null;
    }

    // Kullanýlan mermiyi havuza geri döndürür
    private void ReturnbulletPool(GameObject bullet)
    {
        bullet.SetActive(false);
        BulletPool.Enqueue(bullet);
    }

    // Belirli süre sonra mermiyi havuza geri dönduren coroutine
    private IEnumerator DisableBulletAfterDelay(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnbulletPool(bullet);
    }

    // Kovan havuzundan bir kovan nesnesý alýr, havuz boþsa null döner
    private GameObject GetKovanFromPool()
    {
        if (KovanPool.Count > 0)
        {
            GameObject Kovan = KovanPool.Dequeue();
            Kovan.SetActive(true);
            return Kovan;
        }
        return null;
    }

    // Kullanýlan kovaný havuza geri döndürür
    private void ReturnKovanPool(GameObject kovan)
    {
        kovan.SetActive(false);
        KovanPool.Enqueue(kovan);
    }

    // Belirli süre sonra kovaný havuza geri döndüren coroutine
    
    private IEnumerator DisableKovanAfterDelay(GameObject kovan, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnKovanPool(kovan); 
    }
}