using System;
using UnityEngine;

public class boskovan : MonoBehaviour
{
    AudioSource yeredusmesesi;
   //kovan yere düþünce ses çýkmasýný saðlar
    void Start()
    {
        yeredusmesesi = GetComponent<AudioSource>();
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("yol"))
        {
            yeredusmesesi.Play();
           
          

        }
    }
    
}
