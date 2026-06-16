using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DisableMeshRendererOnStart : MonoBehaviour
{
    [SerializeField] private bool _disableOnStart = true;
    void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        mr.enabled = !_disableOnStart;
    }
}