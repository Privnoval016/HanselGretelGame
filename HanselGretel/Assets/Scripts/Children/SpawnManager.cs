using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    
    public Transform[] spawnPoints;
    public Transform[] sideSpawnPoints;
    [HideInInspector] public Vector3[] deathPoints;

    public Transform playerDeathPoint;
    
    private int maxEnemies;
    public GameObject[] enemies;
    public GameObject spawnPrefab;
    
    [Header("Spawn Rates")]
    [Range(0, 1)] public float spawnRate;

    [Range(0, 1)] public float dontSpawnItemChance;
    [Range(0, 1)] public float randomRequireItemFactor;
    private float effectiveSpawnRate;
    
    public int numEnemiesKilled;
    
    [Header("Hansel Stats")]
    public HoldableType[] hanselRequireItems;
    public float[] hanselAmounts;
    public float[] hanselLevelUpAmounts;
    
    [Header("Gretel Stats")]
    public HoldableType[] gretelRequireItems;
    public float[] gretelAmounts;
    public float[] gretelLevelUpAmounts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        maxEnemies = spawnPoints.Length;
        enemies = new GameObject[maxEnemies];
        
        deathPoints = new Vector3[spawnPoints.Length];
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            deathPoints[i] = spawnPoints[i].position + new Vector3(0, 0, 15f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.GetComponent<Feeder>().state = Feeder.ChildState.Revolt;
                }
            }
            
            return;
        }
        
        CheckForSpawn();
    }
    
    private void CheckForSpawn()
    {
        int previousLevel = GameManager.Instance.level;
        GameManager.Instance.level = Mathf.Max(1, numEnemiesKilled / 5 + 1);
        
        if (previousLevel != GameManager.Instance.level)
        {
            // level up the children
            for (int i = 0; i < hanselAmounts.Length; i++)
            {
                hanselAmounts[i] += hanselLevelUpAmounts[i];
            }
            
            for (int i = 0; i < gretelAmounts.Length; i++)
            {
                gretelAmounts[i] += gretelLevelUpAmounts[i];
            }
        }
        
        
        // modifies the spawn rate based on the current level (non-linear)
        effectiveSpawnRate = Mathf.Max(spawnRate * (1 + GameManager.Instance.level * 0.1f), 1);
        // Spawns enemies at an empty spawn point if they are available based on the spawn rate
        
        if (ActiveEnemies() < maxEnemies)
        {
            if (UnityEngine.Random.value < effectiveSpawnRate * Time.deltaTime)
            {
                for (int i = 0; i < maxEnemies; i++)
                {
                    if (enemies[i] == null)
                    {
                        enemies[i] = Instantiate(spawnPrefab, transform);
                        enemies[i].GetComponent<Feeder>().spawnPosition = spawnPoints[i].position;
                        enemies[i].GetComponent<Feeder>().spawnIndex = i;
                        enemies[i].GetComponent<Feeder>().type = (Feeder.ChildType) UnityEngine.Random.Range(0, 2);
                        break;
                    }
                }
            }
        }
    }
    
    private int ActiveEnemies()
    {
        int count = 0;
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                count++;
            }
        }

        return count;
    }
}
