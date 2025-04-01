using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    public void SetMaterial(Material mat)
    {
        _meshRenderer.material = mat;
    }
}
