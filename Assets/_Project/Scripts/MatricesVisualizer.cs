using System.Collections.Generic;
using UnityEngine;

public class MatricesVisualizer : MonoBehaviour
{
    [Header("References to other objects")]
    [SerializeField] private Transform _holderForCubes;
    
    [Header("References to assets")]
    [SerializeField] private TextAsset _modelJson;
    [SerializeField] private TextAsset _spaceJson;
    [SerializeField] private CubeController _prefabCubeController;
    [SerializeField] private Material _materialForModel;
    [SerializeField] private Material _materialForSpace;

    private List<Matrix4x4> _modelMatrices;
    private List<Matrix4x4> _spaceMatrices;

    void Start()
    {
        ParseJsons();
        VisualizeMatrices(_modelMatrices, _materialForModel);
        VisualizeMatrices(_spaceMatrices, _materialForSpace);
    }

    private void ParseJsons()
    {
        // Parse model.json
        MatrixData[] modelData = JsonHelper.FromJson<MatrixData>(_modelJson.text);
        _modelMatrices = new List<Matrix4x4>();
        foreach (var data in modelData)
        {
            _modelMatrices.Add(data.ToMatrix());
        }

        // Parse space.json
        MatrixData[] spaceData = JsonHelper.FromJson<MatrixData>(_spaceJson.text);
        _spaceMatrices = new List<Matrix4x4>();
        foreach (var data in spaceData)
        {
            _spaceMatrices.Add(data.ToMatrix());
        }

        // Debug        
        Debug.Log("MatricesVisualizator: ParseJsons: Model count = " + 
                  _modelMatrices.Count);
        Debug.Log("MatricesVisualizator: ParseJsons: Space count = " + 
                  _spaceMatrices.Count);
        Debug.Log("MatricesVisualizator: ParseJsons: _modelMatrices[0]:\n" +
                  _modelMatrices[0].ToString());
        Debug.Log("MatricesVisualizator: ParseJsons: _spaceMatrices[0]:\n" +
                  _spaceMatrices[0].ToString());
    }
    
    private void VisualizeMatrices(List<Matrix4x4> matrices, Material material)
    {
        foreach (var matrice in matrices)
        {
            CubeController cubeController = Instantiate(_prefabCubeController, _holderForCubes);
            cubeController.transform.position = new Vector3(matrice.m03, matrice.m13, matrice.m23);
            cubeController.transform.rotation = Quaternion.LookRotation(
                matrice.GetColumn(2), matrice.GetColumn(1));
            cubeController.SetMaterial(material);
        }
        
        Debug.Log("MatricesVisualizer: VisualizeMatrices: " +
                  $"Created {matrices.Count} cubes");
    }
}
