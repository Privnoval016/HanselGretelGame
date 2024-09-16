using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public bool isGameOver;
    
    public bool isPaused;
    public GameObject player;
    

    public int level = 1;
    
    public float maxPlayerHealth = 3;
    public float playerHealth;
    
    public TextMeshProUGUI levelText;

    public GameObject[] liveIcons;

    public GameObject[] particlePrefabs;
    
    public GameObject deathScreen;
    
    private AudioSource audioSource;
    public AudioClip particleAudio;
    
    private bool allowReset = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        deathScreen.SetActive(false);
        
        playerHealth = maxPlayerHealth;
        isPaused = false;
        
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        levelText.text = "Wave: " + level;
        
        for (int i = 0; i < liveIcons.Length; i++)
        {
            if (i < playerHealth)
            {
                liveIcons[i].SetActive(true);
            }
            else
            {
                liveIcons[i].SetActive(false);
            }
        }
        
        if (playerHealth <= 0)
        {
            isGameOver = true;
        }
        
        if (isGameOver && allowReset)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    public void EndGame()
    {
        deathScreen.SetActive(true);
        allowReset = true;
    }

    public GameObject PlayParticle(Vector3 pos, int index)
    {
        audioSource.PlayOneShot(particleAudio);
        return Instantiate(particlePrefabs[index], pos, Quaternion.identity);
    }
    
    public void SwapToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    
}
