using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : Interactable
{
    public GameObject itemPrefab;

    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        if (!player.isHolding)
        {
            GameObject item = Instantiate(itemPrefab, player.transform);
            player.heldObject = item.GetComponent<Holdable>();
            item.transform.position = player.holdPosition.position;
            item.transform.rotation = player.holdPosition.rotation;
            
            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
            
            player.AddWitchMaterial(item);
        }
    }
}