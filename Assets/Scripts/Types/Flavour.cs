using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFlavour", menuName = "Asset/Flavour")]
public class Flavour : ScriptableObject
{
    public string _name;
    [SerializeField] private List<Flavour> weakAgainst = new List<Flavour>();
    [SerializeField] private List<Flavour> strongAgainst = new List<Flavour>();

    public bool isStrongToFlavour(Flavour flavour)
    {
        return strongAgainst.Contains(flavour);
    }
    public bool isWeakToFlavour(Flavour flavour)
    {
        return weakAgainst.Contains(flavour);
    }
}