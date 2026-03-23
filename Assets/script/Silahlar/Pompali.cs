using System.Collections;
using TMPro;
using Unity.VisualScripting;

using UnityEngine;

// Pompalý tüfeðin tüm sistemini yöneten sýnýf
public class Pompali : MonoBehaviour
{
    Animator animatorum; // Silahýn animasyon bileþeni

    [Header("AYARLAR")]
    public bool atesedebilirmi;            // Silahýn ateþ edip edemeyeceðini belirler
    public float disaridanAtesetmesiklik;  // Inspector'dan ayarlanan ateþ aralýðý (saniye)
    float iceridenatesetmesikligi;         // Bir sonraki ateþ zamanýný tutan iç deðiþken
    public float menzil;                   // Raycast'ýn ulaþabileceði maksimum mesafe
    public ParticleSystem efektim;         // Namlu alevi / duman efekti
    bool zoomvarmi;                        // Oyuncunun þu an zoom yapýp yapmadýðýný tutar
    public GameObject CrossHair;           // Ekrandaki crosshair izi 

    [Header("SESLER")]
    public AudioSource silahsesi;          // Ateþ etme sesi
    public AudioSource sarjordegistirme;   // Þarjör deðiþtirme sesi
    public AudioSource MermiBittiSesi;     // Mermi bittiðinde çýkan ses
    public AudioSource MermiAlmaSesi;      // Mermi kutusu alýndýðýnda çýkan ses


    [Header("Efektler")]
    public ParticleSystem Kanizi;          // Düþmana isabet halinde kan efekti
    public ParticleSystem Mermisicrama;    // Yüzeye isabet halinde mermi sýçrama efekti
    public ParticleSystem Mermiizi;        // Yüzeyde býrakýlan mermi izi efekti

    [Header("DÝÐERLERÝ")]
    public Camera benimcamim;             // Oyuncu kamerasý (Raycast ve zoom için)
    float FieldCamPos;                    // Kameranýn baþlangýç Field of View deðeri
    float YaklasmaPov = 50;              // Zoom yapýldýðýnda kullanýlacak FOV deðeri
                                         // NOT: Sniper'daki 20'ye kýyasla daha az yakýnlaþtýrma (pompalýya özgü)

    [Header("SÝLAH AYARLAR")]
    int toplammermiSayisi;                        // Envanterdeki toplam mermi sayýsý
    public int SarjorKapasite;                    // Þarjörün maksimum mermi kapasitesi
    int KalanMermiSayisi;                         // Þarjörde kalan mermi sayýsý
    public string Silahin_adi;                    // PlayerPrefs'te kullanýlacak silah adý anahtarý
    public TextMeshProUGUI ToplamMermi_text;       // Toplam mermiyi gösteren UI metni
    public TextMeshProUGUI KalanMermi_text;        // Kalan mermiyi gösteren UI metni
    public float DarbeGucu;                       // Düþmana verilen hasar miktarý

    public bool kovan_ciksinmi;                   // Ateþ edince kovan çýkýp çýkmayacaðýný belirler
    public GameObject KovanCikisNoktasi;          // Kovanýn fýrlatýlacaðý nokta
    public GameObject KovanObjesi;               // Kovan prefabý (Object Pool YOK — direkt Instantiate)

    bool sarjordolduruyomu = true;                // Þarjör doldurma iþleminin aktif olup olmadýðý

    public Mermi_Kutusu_Olustur Mermi_Kutusu_Olusturma_Yonetim; // Mermi kutusu yönetim scripti referansý


    void Start()
    {
        // Kaydedilmiþ mermi sayýsýný PlayerPrefs'ten yükle
        toplammermiSayisi = PlayerPrefs.GetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
        kovan_ciksinmi = true;
        Baslangic_mermi_doldur();                // Baþlangýçta þarjörü doldur
        MermiDoldurmaFonksiyon("NormalYazma");   // UI metnini güncelle
        FieldCamPos = benimcamim.fieldOfView;    // Varsayýlan FOV deðerini kaydet
        animatorum = GetComponent<Animator>();   // Animator bileþenini al
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
                }
            }
            // Mermi bittiyse ses çal
            if (KalanMermiSayisi == 0)
            {
                MermiBittiSesi.Play();
            }
        }

        // R tuþuna basýldýysa veya mermi bittiyse þarjör deðiþtirme animasyonunu baþlat
        if ((Input.GetKey(KeyCode.R) || KalanMermiSayisi == 0) && sarjordolduruyomu)
        {
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

        // Sað týk basýldýysa zoom animasyonunu baþlat
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animatorum.SetBool("zoomyap", true);
            zoomvarmi = true;
         
        }

        // Sað týk býrakýldýðýnda zoom'u kapat ve normal görüþe dön
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            CrossHair.SetActive(true);            // Crosshair'i tekrar göster
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

    // Trigger alanýna giren nesneleri kontrol eder (otomatik toplama)
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
        AtesEtmeteknikÝslemleri(yakinlasmavarmi); // Kovan, ses, animasyon iþlemleri
        RaycastHit hit;
        silahsesi.Play();  // Silah ateþ sesi çal
        efektim.Play();    // Namlu alevi efektini baþlat

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
                Instantiate(Mermiizi, hit.point, Quaternion.LookRotation(hit.normal));     // Mermi izi efekti
                Instantiate(Mermisicrama, hit.point, Quaternion.LookRotation(hit.normal)); // Sýçrama efekti
            }
        }

        KalanMermiSayisi--;                              // Bir mermi harca
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

    // E tuþu ile önündeki mermi kutusunu Raycast ile alýr (4 birim menzil)
    void MermiAl()
    {
        RaycastHit Hit;
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
            sarjordegistirme.Play(); // Þarjör deðiþtirme sesi çal

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

    // Ateþ etmenin kovan ve animasyon iþlemlerini yapar
   
    void AtesEtmeteknikÝslemleri(bool yakinlasmavarmi)
    {
        if (kovan_ciksinmi)
        {
            // Kovan nesnesini çýkýþ noktasýnda oluþtur
            GameObject obje = Instantiate(KovanObjesi, KovanCikisNoktasi.transform.position, KovanCikisNoktasi.transform.rotation);
            Rigidbody rbd = obje.GetComponent<Rigidbody>();
            rbd.AddRelativeForce(new Vector3(-200f, 1, 0)); // Kovana sola doðru kuvvet uygula (fýrlatma)
        }

        // Zoom durumuna göre farklý animasyon oynat
        if (!yakinlasmavarmi)
        {
            animatorum.Play("ateset");       // Normal ateþ animasyonu
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
                PlayerPrefs.SetInt("Taramali_Mermi", PlayerPrefs.GetInt("Taramali_Mermi") + mermisayisi);
                break;
            case "Pompali":
                // Pompalý mermisi alýndýysa bu silahýn toplam sayýsýný artýr ve UI'ý güncelle
                toplammermiSayisi += mermisayisi;
                PlayerPrefs.SetInt(Silahin_adi + "_Mermi", toplammermiSayisi);
                MermiDoldurmaFonksiyon("NormalYazma");
                break;
            case "Sniper":
                PlayerPrefs.SetInt("Sniper_Mermi", PlayerPrefs.GetInt("Sniper_Mermi") + mermisayisi);
                break;
            case "Magnum":
                PlayerPrefs.SetInt("Magnum_Mermi", PlayerPrefs.GetInt("Magnum_Mermi") + mermisayisi);
                break;
        }
    }

    // Zoom yapýldýðýnda FOV'u daraltýr ve crosshair'i gizler
  
    void KameraYaklastir()
    {
        benimcamim.fieldOfView = YaklasmaPov; // FOV'u daralt (yakýnlaþtýr)
        CrossHair.SetActive(false);           // Crosshair'i gizle
    }
}