using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MatricesVisualizer : MonoBehaviour
{
    // [Header("References to other objects")]
    // [SerializeField] private float _delayInSeconds = 0.1f;
    
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
    private List<CubeController> _cubesOfModel;

    void Start()
    {
        ParseJsons();
        _cubesOfModel = new List<CubeController>(_modelMatrices.Count);
        // _cubesOfModel.Capacity = _modelMatrices.Count;
        
        CreateCubes(_modelMatrices, _materialForModel, _cubesOfModel);
        CreateCubes(_spaceMatrices, _materialForSpace, null);
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
    
    private void CreateCubes(List<Matrix4x4> matrices, Material material, 
        List<CubeController> listOfCubeControllers)
    {
        foreach (var matrix in matrices)
        {
            CubeController cubeController = Instantiate(_prefabCubeController, _holderForCubes);
            cubeController.transform.position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            cubeController.transform.rotation = Quaternion.LookRotation(
                matrix.GetColumn(2), matrix.GetColumn(1));
            cubeController.SetMaterial(material);

            if (listOfCubeControllers != null)
            {
                listOfCubeControllers.Add(cubeController);
            }
        }
        
        Debug.Log("MatricesVisualizer: CreateCubes: " +
                  $"Created {matrices.Count} cubes");
    }

    private void DeactivateAllCubesOfModel()
    {
        foreach (var cubeController in _cubesOfModel)
        {
            cubeController.gameObject.SetActive(false);
        }
    }
}
