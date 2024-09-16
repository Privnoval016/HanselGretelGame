using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HoldableType
{
    PancakeBatter,
    Pancake,
    DecoratedPancake,
    Crayfish,
    MilkEmpty,
    MilkFull,
    Candy
}
public class Holdable : MonoBehaviour
{
    public HoldableType type;
}
