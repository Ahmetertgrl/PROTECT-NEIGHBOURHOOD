using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnaMenuKontrol : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject Cikispanel;
    public Slider LoadingSlider;

    //Play butonuna basýldýðýnda oyunun yüklenmesini saðlar
    public void OyunaBasla()
    {
        StartCoroutine(SahneYuklemeLoading());
        Time.timeScale = 1f;
    }
     IEnumerator SahneYuklemeLoading()
     {
         AsyncOperation operation = SceneManager.LoadSceneAsync(1);
         LoadingPanel.SetActive(true);
         while (!operation.isDone)
         {
             float ilerleme = Mathf.Clamp01(operation.progress / .9f);
             LoadingSlider.value = ilerleme;
             yield return null;
         }
     }
 

    //butonlara görev atama
    public void OyundanCik()
    {
        Cikispanel.SetActive(true);
        

    }
     
    public void EvetCikilsin()
    {
        Application.Quit();

    }
    public void HayirCikilmasin()
    {
        Cikispanel.SetActive(false);

    }

}
