using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Bomba : MonoBehaviour
{
    public float guc = 10f;
    public float menzil = 5f;
    public float yukariguc = 1f;
    public ParticleSystem Patlamaefekt;
    public AudioSource PatlamaSesi;
   
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision!=null)
        {
            patlama();
            Destroy(gameObject, .5f);
        
        }
    }
    //Bomba patladýðýnda etraftaki cisimlere kuvvet uygulama,bomba sesi çalýnmasý,bombanýn belli bir menzile etki etmesi ayarlanýr
    void patlama()
    {
        Vector3 patlamapozisyon=transform.position;
        Instantiate(Patlamaefekt, transform.position, transform.rotation);
        PatlamaSesi.Play();
        Collider[] colliders = Physics.OverlapSphere(patlamapozisyon, menzil);

        foreach(Collider hit in colliders)
        {
            Rigidbody rb=hit.GetComponent<Rigidbody>();
            if(hit!=null && rb)
            {
                rb.AddExplosionForce(guc, patlamapozisyon, menzil,  1,ForceMode.Impulse);
                if (hit.gameObject.CompareTag("Dusman"))
                {
                    hit.transform.gameObject.GetComponent<Dusman>().oldun();

                }
            }
           
        }
    }
}
