using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterDestruction : MonoBehaviour
{
    public List<Rigidbody> allParts = new List<Rigidbody>();

    public void Shatter()
    {
        foreach (var part in allParts)
        {
            part.isKinematic = false;
        }
    }
}
