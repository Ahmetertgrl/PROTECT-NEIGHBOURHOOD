using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomba_Kutusu_Olustur : MonoBehaviour
{
    public List<GameObject> BombaKutusuPoint = new List<GameObject>();
    public GameObject Bomba_Kutusunun_kendisi;

    public static bool Bomba_Kutusu_Varmi;
    public float Kutu_Olusma_Süresi;

    
    int randomsayim;
    void Start()
    {
        Bomba_Kutusu_Varmi = false;
        StartCoroutine(Bomba_Kutusu_Yap());

    }


    //belli süre aralýklarla bomba kutusu oluþturma coroutine
    IEnumerator Bomba_Kutusu_Yap()
    {
        while (true)
        {
            yield return new WaitForSeconds(Kutu_Olusma_Süresi);


            if (!Bomba_Kutusu_Varmi) { 
            randomsayim = Random.Range(0, 5);
            Instantiate(Bomba_Kutusunun_kendisi, BombaKutusuPoint[randomsayim].transform.position, BombaKutusuPoint[randomsayim].transform.rotation);
            Bomba_Kutusu_Varmi = true;
            }
        }
       


    }
   
}
