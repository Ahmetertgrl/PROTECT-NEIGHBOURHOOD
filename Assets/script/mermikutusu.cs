using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class mermikutusu : MonoBehaviour
{
    //Random nokatalardan random mermi sayýsý ile mermi kutusu olusturma
    string[] silahlar = { "Magnum", "Pompali", "Sniper", "Taramali" };
    int[] mermisayisi = { 10, 5, 20, 30 };

    public string Olusan_SilahinTuru;
    public int Olusan_MermiSayisi;

    public List<Sprite> Silah_resimleri = new List<Sprite>();
    public Image Silahresmi;
    public int Noktasi;
  

    void Start()
    {
        int gelenanahtar = Random.Range(0, silahlar.Length);
        Olusan_SilahinTuru = silahlar[gelenanahtar];
        Olusan_MermiSayisi = mermisayisi[Random.Range(0, mermisayisi.Length)];

        Silahresmi.sprite = Silah_resimleri[gelenanahtar];
        

       
    }

    void Update()
    {
       
    }

}
