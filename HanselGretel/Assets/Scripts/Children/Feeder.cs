using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Feeder : Interactable
{
    public enum ChildType
    {
        Hansel,
        Gretel
    }
    
    public ChildType type;
    
    public List<HoldableType> requiredItems = new List<HoldableType>();
    public List<int> requiredAmounts = new List<int>();
    
    public int spawnIndex;
    
    public Vector3 spawnPosition;

    public float timeToLeave;
    private float leaveTimer;

    public GameObject[] models;

    public GameObject cage;

    private Animator anim;

    public Image leaveUI;
    public Image leaveBGUi;
    public Image[] requireUI;
    
    private Vector3 leavePos;
    
    private Vector3 originalScale;
    
    private Coroutine killCoroutine;

    private GameObject cageSpawnParticle, cageDespawnParticle;
    
    public AudioClip deathClip;
    
    private AudioSource audioSource;

    private bool playedDeath;
    

    public enum ChildState
    {
        Entering,
        Idle,
        Exiting,
        Failed,
        Revolt
    }
    
    public ChildState state;
    
    private NavMeshAgent agent;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        SetRequiredItems();
        
        state = ChildState.Entering;

        Vector3 startPos = FindClosestStart();
        transform.position = startPos;
        agent.SetDestination(spawnPosition);

        cage.SetActive(false);
        
        audioSource = GetComponent<AudioSource>();
        
        requireUI[0].gameObject.SetActive(false);
        requireUI[1].gameObject.SetActive(false);
        
        originalScale = models[(int) type].transform.localScale;

        leavePos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        leaveBGUi.transform.parent.rotation = Quaternion.LookRotation(leaveBGUi.transform.parent.position - Camera.main.transform.position);

        
        switch (state)
        {
            case ChildState.Entering:
                
                leaveBGUi.gameObject.SetActive(false);
                requireUI[(int) type].gameObject.SetActive(false);
                anim.SetBool("beScared", false);
                CheckIfAtSpawn();
                break;
            case ChildState.Idle:

                cageSpawnParticle ??= GameManager.Instance.PlayParticle(transform.position, 0);
                cage.SetActive(true);
                
                requireUI[(int) type].gameObject.SetActive(true);
                
                
                leaveBGUi.gameObject.SetActive(true);
                leaveUI.fillAmount = leaveTimer / timeToLeave;
                
                TextMeshProUGUI[] slots = requireUI[(int) type].GetComponentsInChildren<TextMeshProUGUI>();
                
                
                for (int i = 0; i < slots.Length; i++)
                {
                    if (requiredAmounts[i] <= 0)
                    {
                        //slots[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        slots[i].gameObject.SetActive(true);
                    }
                    
                    slots[i].text = "x " + requiredAmounts[i];
                }
                
                anim.SetBool("beScared", true);
                
                models[(int) type].transform.rotation = 
                    Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 90, 0), Time.deltaTime * 0.1f);
                
                CheckToDestroy();
                CheckToLeave();
                break;
            case ChildState.Exiting:
                
                leaveBGUi.gameObject.SetActive(false);
                
                
                requireUI[(int) type].gameObject.SetActive(false);
                
                if (Vector3.Distance(transform.position, SpawnManager.Instance.deathPoints[spawnIndex]) < 1)
                {
                    if (!playedDeath)
                    {
                        audioSource.PlayOneShot(deathClip);
                        playedDeath = true;
                    }
                    Destroy(gameObject, 2f);
                }
                
                if (type == ChildType.Hansel)
                {
                    models[1].transform.localScale = new Vector3(originalScale.x * 4, originalScale.y, originalScale.z * 4);
                }
                
                break;
            
            case ChildState.Failed:
                
                leaveBGUi.gameObject.SetActive(false);
                
                anim.SetBool("beScared", false);
                
                requireUI[(int) type].gameObject.SetActive(false);

                agent.SetDestination(leavePos);
                
                if (Vector3.Distance(transform.position, leavePos) < 1)
                {
                    Destroy(gameObject);
                }
                
                break;
            
            case ChildState.Revolt:
                killCoroutine ??= StartCoroutine(KillPlayer());
                break;
        }
    }
    
    private IEnumerator KillPlayer()
    {
        cageDespawnParticle ??= GameManager.Instance.PlayParticle(transform.position, 0);
        cage.SetActive(false);
        
        requireUI[(int) type].gameObject.SetActive(false);
        leaveBGUi.gameObject.SetActive(false);
        
        anim.SetBool("beScared", false);
        agent.SetDestination(GameManager.Instance.player.transform.position);
        
        yield return new WaitForSeconds(3.5f);

        GameManager.Instance.player.GetComponent<PlayerController>().death = true;
        agent.SetDestination(SpawnManager.Instance.playerDeathPoint.position);
        
        while (Vector3.Distance(transform.position, SpawnManager.Instance.playerDeathPoint.position) > 1f)
        {
            yield return null;
        }
        
        anim.SetBool("beScared", true);
        agent.velocity = Vector3.zero;
    }
    
    
    public void SetRequiredItems()
    {
        requiredItems.Clear();
        models[0].SetActive(false);
        models[1].SetActive(false);
        
        switch (type)
        {
            case ChildType.Hansel:
                AddHanselItems();
                break;
            case ChildType.Gretel:
                AddGretelItems();
                break;
        }

        timeToLeave = 0;
        foreach (var item in requiredItems)
        {
            timeToLeave += Random.Range(10, 15) / (1 + GameManager.Instance.level * 0.1f);
            timeToLeave *= 1.75f;
        }
    }
    
    private void AddHanselItems()
    {
        models[0].SetActive(true);
        
        // adds a number of each required item to the list based on the biases (each item has a bias value at the same index)
        
        for (int i = 0; i < SpawnManager.Instance.hanselRequireItems.Length; i++)
        {
            if (Random.value < SpawnManager.Instance.dontSpawnItemChance)
            {
                if (i < SpawnManager.Instance.hanselRequireItems.Length - 1)
                {
                    requiredAmounts.Add(0);
                    continue;
                }
                
                if (requiredItems.Count != 0)
                {
                    requiredAmounts.Add(0);
                    break;
                }
            }
            
            int numItemsToAdd = (int) SpawnManager.Instance.hanselAmounts[i];
            
            numItemsToAdd = (int) (numItemsToAdd * Random.Range(SpawnManager.Instance.randomRequireItemFactor, 1));
            
            numItemsToAdd = Mathf.Max(1, numItemsToAdd);
            
            for (int j = 0; j < numItemsToAdd; j++)
            {
                requiredItems.Add(SpawnManager.Instance.hanselRequireItems[i]);
            }
            
            requiredAmounts.Add(numItemsToAdd);
        }
        
        anim = models[0].GetComponent<Animator>();
        
    }
    
    private void AddGretelItems()
    {
        models[1].SetActive(true);
        
        // adds a number of each required item to the list based on the biases (each item has a bias value at the same index)
        
        for (int i = 0; i < SpawnManager.Instance.gretelRequireItems.Length; i++)
        {
            if (Random.value < SpawnManager.Instance.dontSpawnItemChance)
            {
                if (i < SpawnManager.Instance.gretelRequireItems.Length - 1)
                {
       
                    requiredAmounts.Add(0);
                    continue;
                }
                
                if (requiredItems.Count != 0)
                {
        
                    requiredAmounts.Add(0);
                    break;
                }
            }
            
            int numItemsToAdd = (int) SpawnManager.Instance.gretelAmounts[i];
            
            numItemsToAdd = (int) (numItemsToAdd * Random.Range(SpawnManager.Instance.randomRequireItemFactor, 1));
            
            numItemsToAdd = Mathf.Max(1, numItemsToAdd);
            
            for (int j = 0; j < numItemsToAdd; j++)
            {
                requiredItems.Add(SpawnManager.Instance.gretelRequireItems[i]);
            }
            
            requiredAmounts.Add(numItemsToAdd);
            

        }
        
        anim = models[1].GetComponent<Animator>();
        
    }
    
    public override void Interact(PlayerController player)
    {
        if (state != ChildState.Idle) return;
        
        base.Interact(player);
        
        if (player.isHolding)
        {
            if (requiredItems.Contains(player.heldObject.type))
            {
                requiredItems.Remove(player.heldObject.type);

                int index = 0;
                if (type == ChildType.Gretel)
                {
                    index = Array.IndexOf(SpawnManager.Instance.gretelRequireItems, player.heldObject.type);
                }
                else
                {
                    index = Array.IndexOf(SpawnManager.Instance.hanselRequireItems, player.heldObject.type);
                }
                
                requiredAmounts[index]--;
                
                cageDespawnParticle ??= GameManager.Instance.PlayParticle(transform.position, 0);
                
                Destroy(player.heldObject.gameObject);
                player.heldObject = null;
            }
        }
    }
    
    
    private void CheckToDestroy()
    {
        if (requiredItems.Count == 0)
        {
            
            SpawnManager.Instance.numEnemiesKilled++;
            SpawnManager.Instance.enemies[spawnIndex] = null;
            agent.SetDestination(SpawnManager.Instance.deathPoints[spawnIndex]);

            GameManager.Instance.player.GetComponent<PlayerController>().AddWitchMaterial(cage);
            state = ChildState.Exiting;
        }
    }
    
    
    private Vector3 FindClosestStart()
    {
        Vector3 closest = Vector3.zero;
        float closestDist = Mathf.Infinity;
        
        foreach (Transform t in SpawnManager.Instance.sideSpawnPoints)
        {
            float dist = Vector3.Distance(t.position, spawnPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = t.position;
            }
        }
        
        return closest;
    }
    
    private void CheckIfAtSpawn()
    {
        if (Vector3.Distance(transform.position, spawnPosition) < 1)
        {
            state = ChildState.Idle;
        }
    }
    
    
    private void CheckToLeave()
    {
        leaveTimer += Time.deltaTime;
        
        if (leaveTimer >= timeToLeave)
        {
            GameManager.Instance.playerHealth--;
            SpawnManager.Instance.enemies[spawnIndex] = null;
            agent.SetDestination(leavePos);
            
            cageDespawnParticle ??= GameManager.Instance.PlayParticle(transform.position, 0);
            cage.SetActive(false);
            
            state = ChildState.Failed;
        }
    }
}
