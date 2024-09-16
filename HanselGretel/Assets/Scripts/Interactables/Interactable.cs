using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void Interact(PlayerController player)
    {
        Debug.Log("Interacting with " + gameObject.name);
    }
}
