using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Dusman : MonoBehaviour
{
    NavMeshAgent ajan;
    GameObject Hedef;
    public float health;
    public float dusmandarbegucu;
    GameObject Anakontrolcum;
    Animator Animatorum;
    GameObject Silah;
  


    //düþmanlarin hedefe doðru ilerlemesi,animasyonlarýnýn oynatýlmasý.
    void Start()
    {
        Animatorum = GetComponent<Animator>();
        ajan = GetComponent<NavMeshAgent>();
        Anakontrolcum = GameObject.FindWithTag("AnaKontrolcum");
        
       
    }
    public void HedefBelirle(GameObject objem)
    {
        Hedef = objem;
    }

    void Update()
    {
       ajan.SetDestination(Hedef.transform.position); 
    }
   public void Darbeal(float darbegucu)
    {
        health -= darbegucu;
        if (health <= 0)
        {
            oldun();
            gameObject.tag = "Untagged";
            
        }
    }
    public void oldun()
    {
        Animatorum.SetTrigger("Olme"); //ölme animasyonunu oynatýr.
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); //düþman öldüðünde raycast ile etkileþimi kapatýr.
        Destroy(gameObject,3f); //düþmaný yok eder.
        Anakontrolcum.GetComponent<GameKontrolcu>().DusmanSayisiGuncelle();
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("hedefimiz"))
        {
            oldun();
           
            Anakontrolcum.GetComponent<GameKontrolcu>().CanAzalt(dusmandarbegucu); //Düþmanlarýn hedefe ulaþtýðýnda canýmýzý azaltmalarý saðlanýr.
          
        }
    }
}
