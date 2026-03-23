using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mermi_Kutusu_Olustur : MonoBehaviour
{
    public List<GameObject> MermiKutusuPoint = new List<GameObject>();
    public GameObject Mermi_Kutusunun_kendisi;

    public static bool Mermi_Kutusu_Varmi;
    public float Kutu_Olusma_Süresi;

    //Mermi kutusunun oluþacaðý noktalarýn indexleri
    List<int> noktalar = new List<int>();
    int randomsayim;
    void Start()
    {
        Mermi_Kutusu_Varmi = false;
        StartCoroutine(Mermi_Kutusu_Yap());

    }


    //Random noktalarda Mermi kutusu oluþturur
    IEnumerator Mermi_Kutusu_Yap()
    {
        while (true)
        {
            yield return new WaitForSeconds(Kutu_Olusma_Süresi);

            randomsayim = Random.Range(0, 4);
            if (!noktalar.Contains(randomsayim))
            {
                noktalar.Add(randomsayim);
            }
            else
            {
                randomsayim = Random.Range(0, 4);
                continue;
            }

               GameObject objem= Instantiate(Mermi_Kutusunun_kendisi, MermiKutusuPoint[randomsayim].transform.position, MermiKutusuPoint[randomsayim].transform.rotation);
               objem.transform.gameObject.GetComponentInChildren<mermikutusu>().Noktasi = randomsayim;

        }
       


    }
    public void NoktalariKaldirma(int deger)
    {
        noktalar.Remove(deger);

    }
}
