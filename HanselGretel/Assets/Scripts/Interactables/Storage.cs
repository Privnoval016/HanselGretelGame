using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : Interactable
{
    public GameObject item;
    public Transform holdPosition;
    
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        
        if (!player.isHolding && item != null)
        {
            player.heldObject = item.GetComponent<Holdable>();
            item.transform.SetParent(player.transform);
            item.transform.position = player.holdPosition.position;
            item.transform.rotation = player.holdPosition.rotation;
            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
            player.AddWitchMaterial(item);
            item = null;
        }
        else if (player.isHolding && item == null)
        {
            item = player.heldObject.gameObject;
            item.transform.SetParent(transform);
            item.transform.position = holdPosition.position;
            item.transform.rotation = holdPosition.rotation;
            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
            player.RemoveWitchMaterial(item);
            player.heldObject = null;
        }
    }
}
