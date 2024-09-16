using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Cooker : Interactable
{
    public GameObject heldObject;
    public HoldableType objectType;
    public GameObject cookedObjectPrefab;
    public float cookTime;
    public Transform objectPutPosition;

    public Image timerUI;
    public Image cookUI;

    public bool destroyAfterTime;
    public float destroyTime;
    
    public Image destroyUI;
    
    private float cookTimer, destroyTimer;
    

    public enum CookerState
    {
        Empty,
        Cooking,
        Done
    }
    
    private CookerState state;
    
    private void Start()
    {
        timerUI.transform.parent.LookAt(Camera.main.transform);
        //timerUI.transform.rotation = Quaternion.Euler(timerUI.transform.rotation.eulerAngles.x, timerUI.transform.rotation.eulerAngles.y, 0);
        
        state = CookerState.Empty;
        cookTimer = 0;
    }
    
    private void Update()
    {
        switch (state)
        {
            case CookerState.Empty:
                
                timerUI.gameObject.SetActive(false);
                
                destroyTimer = 0;
                break;
            case CookerState.Cooking:
                
                timerUI.gameObject.SetActive(true);
                destroyUI.gameObject.SetActive(false);
                
                cookUI.fillAmount = cookTimer / cookTime;
                
                cookTimer += Time.deltaTime;
                if (cookTimer >= cookTime)
                {
                    state = CookerState.Done;
                    cookTimer = 0;
                    Destroy(heldObject);
                    heldObject = Instantiate(cookedObjectPrefab, transform);
                    GameManager.Instance.PlayParticle(objectPutPosition.position, 2);
                    
                    heldObject.transform.position = objectPutPosition.position;
                }
                
                break;
            case CookerState.Done:
                
                cookUI.fillAmount = 1;
                
                if (destroyAfterTime)
                {
                    destroyUI.gameObject.SetActive(true);
                    destroyUI.fillAmount = destroyTimer / destroyTime;
                    
                    destroyTimer += Time.deltaTime;
                    if (destroyTimer >= destroyTime)
                    {
                        Destroy(heldObject);
                        GameManager.Instance.PlayParticle(objectPutPosition.position, 1);

                        heldObject = null;
                        state = CookerState.Empty;
                    }
                }
                else
                {
                    timerUI.gameObject.SetActive(false);
                }
                
                break;
        }
    }

    public override void Interact(PlayerController player)
    {
        base.Interact(player);

        switch (state)
        {
            case CookerState.Empty:
                PutObject(player);
                break;
            case CookerState.Cooking:
                break;
            case CookerState.Done:
                TakeObject(player);
                break;
        }
    }
    
    private void PutObject(PlayerController player)
    {
        if (player.isHolding && player.heldObject.type == objectType)
        {
            heldObject = player.heldObject.gameObject;
            player.RemoveWitchMaterial(heldObject);
            heldObject.transform.position = objectPutPosition.position;
            heldObject.transform.rotation = objectPutPosition.rotation;
            heldObject.transform.SetParent(transform);
            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);

            player.heldObject = null;
            state = CookerState.Cooking;
        }
    }
    
    private void TakeObject(PlayerController player)
    {
        if (!player.isHolding)
        {
            heldObject.transform.SetParent(player.gameObject.transform);
            player.heldObject = heldObject.GetComponent<Holdable>();
            player.heldObject.transform.position = player.holdPosition.position;
            player.heldObject.transform.rotation = player.holdPosition.rotation;
            player.AddWitchMaterial(heldObject);

            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
            
            heldObject = null;
            state = CookerState.Empty;
        }
    }
}
