using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Topper : Interactable
{
    public HoldableType type;
    public GameObject itemPrefab;
    
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        if (player.isHolding)
        {
            if (player.heldObject.type == type)
            {
                Destroy(player.heldObject.gameObject);
                player.heldObject = Instantiate(itemPrefab, player.transform).GetComponent<Holdable>();
                player.heldObject.transform.position = player.holdPosition.position;
                player.heldObject.transform.rotation = player.holdPosition.rotation;
                GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
                player.AddWitchMaterial(player.heldObject.gameObject);
            }
        }
    }
}
