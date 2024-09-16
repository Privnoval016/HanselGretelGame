using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : Interactable
{
    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        if (player.isHolding)
        {
            Destroy(player.heldObject.gameObject);
            GameManager.Instance.PlayParticle(player.holdPosition.position, 0);
            player.heldObject = null;
        }
    }
}
