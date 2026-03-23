using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health_Kutusu_Olustur : MonoBehaviour
{
    public List<GameObject> HealthKutusuPoint = new List<GameObject>();
    public GameObject Health_Kutusunun_kendisi;

    public static bool Health_Kutusu_Varmi;
    public float Kutu_Olusma_Süresi;

    
    int randomsayim;
    void Start()
    {
        Health_Kutusu_Varmi = false;
        StartCoroutine(Health_Kutusu_Yap());

    }



    IEnumerator Health_Kutusu_Yap()
    {
        while (true)
        {
            yield return new WaitForSeconds(Kutu_Olusma_Süresi);


            if (!Health_Kutusu_Varmi) { 
            randomsayim = Random.Range(0, 5);
            Instantiate(Health_Kutusunun_kendisi, HealthKutusuPoint[randomsayim].transform.position, HealthKutusuPoint[randomsayim].transform.rotation);
            Health_Kutusu_Varmi = true;
            }
        }
       


    }
   
}
