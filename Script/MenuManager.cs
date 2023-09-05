using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] clip;
    public Canvas menuCanvas;
    public Canvas creditsCanvas;
    public Canvas mapSelectCanvas;
    public Canvas blockedMap;

    void Start()
    {
        source = GetComponent<AudioSource>();
        creditsCanvas.enabled = false; 
        mapSelectCanvas.enabled = false; 
        menuCanvas.enabled = true;
        blockedMap.enabled = false;
    }

    public void SelectMap()
    {
        creditsCanvas.enabled = false;
        mapSelectCanvas.enabled = true;
        menuCanvas.enabled = false;
    }

    public void Credits()
    {
        creditsCanvas.enabled = true;
        mapSelectCanvas.enabled = false;
        menuCanvas.enabled = false;
    }

    public void Return()
    {
        source.PlayOneShot(clip[0]);
        creditsCanvas.enabled = false;
        mapSelectCanvas.enabled = false;
        menuCanvas.enabled = true;
        blockedMap.enabled = false;
    }

    public void BlockedMap()
    {
        blockedMap.enabled = !blockedMap.enabled;
    }

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void MouseEnter()
    {
        source.PlayOneShot(clip[3]);
    }

    public void MouseClickPlay()
    {
        source.PlayOneShot(clip[1]);
    }

    public void MouseClick()
    {
        source.PlayOneShot(clip[2]);
    }

    public void ReturnBlock()
    {
        source.PlayOneShot(clip[0]);
    }
}