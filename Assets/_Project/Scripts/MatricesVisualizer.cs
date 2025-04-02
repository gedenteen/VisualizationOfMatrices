using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MatricesVisualizer : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float _delayInSeconds = 0.001f;
    [SerializeField] private float _tolerance = 0.001f;
    
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
        Debug.Log("MatricesVisualizer: ParseJsons: Model count = " + 
                  _modelMatrices.Count);
        Debug.Log("MatricesVisualizer: ParseJsons: Space count = " + 
                  _spaceMatrices.Count);
        // Debug.Log("MatricesVisualizer: ParseJsons: _modelMatrices[0]:\n" +
        //           _modelMatrices[0].ToString());
        // Debug.Log("MatricesVisualizer: ParseJsons: _spaceMatrices[0]:\n" +
        //           _spaceMatrices[0].ToString());
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
        for (int i = 0; i < _cubesOfModel.Count; i++)
        {
            _cubesOfModel[i].gameObject.SetActive(false);
        }
    }

    private void ChangePositionAndActivateCubeOfModel(int index, Matrix4x4 matrix)
    {
        CubeController cube = _cubesOfModel[index];
        cube.transform.position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        cube.transform.rotation = Quaternion.LookRotation(
            matrix.GetColumn(2), matrix.GetColumn(1));
        cube.gameObject.SetActive(true);
    }

    private bool ContainsMatrix(List<Matrix4x4> matrices, Matrix4x4 target, 
        float tolerance)
    {
        foreach (var m in matrices)
        {
            if (MatrixApproximatelyEqual(m, target, tolerance))
            {
                return true;
            }
        }
        return false;
    }

    private bool MatrixApproximatelyEqual(Matrix4x4 a, Matrix4x4 b, 
        float tolerance)
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (Mathf.Abs(a[row, col] - b[row, col]) > tolerance)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public async UniTask FindPossibleOffsets(bool withVisualization)
    {
        if (_modelMatrices.Count == 0)
        {
            Debug.Log($"MatricesVisualizer: FindPossibleOffsets: No model matrices available");
            return;
        }
        List<Matrix4x4> possibleOffsets = new List<Matrix4x4>();
    
        Matrix4x4 baseModel = _modelMatrices[0];
        Matrix4x4 invBase = baseModel.inverse;
    
        for (int i = 0; i < _spaceMatrices.Count; i++)
        {
            Matrix4x4 spaceMatrix = _spaceMatrices[i];
            Matrix4x4 candidateOffset = spaceMatrix * invBase;
            bool valid = true;
            int j = 0;

            if (withVisualization)
            {
                DeactivateAllCubesOfModel();
            }
        
            for ( ; j < _modelMatrices.Count; j++)
            {
                Matrix4x4 modelMatrix = _modelMatrices[j];
                Matrix4x4 result = candidateOffset * modelMatrix;

                if (withVisualization)
                {
                    ChangePositionAndActivateCubeOfModel(j, result);
                    await UniTask.WaitForSeconds(_delayInSeconds);
                }

                if (!ContainsMatrix(_spaceMatrices, result, _tolerance))
                {
                    valid = false;
                    break;
                }
            }
        
            if (valid)
            {
                // Debug.Log($"MatricesVisualizer: FindPossibleOffsets: found valid offset, i={i}, j={j}");
                possibleOffsets.Add(candidateOffset);
            }
        
            // Debug.Log($"MatricesVisualizer: FindPossibleOffsets: end of iteration, i={i}, j={j}");
        }
    
        Debug.Log($"MatricesVisualizer: FindPossibleOffsets: Found {possibleOffsets.Count} valid offsets");
        // foreach (var offset in possibleOffsets)
        // {
        //     Debug.Log($"MatricesVisualizer: FindPossibleOffsets: Offset:\n{offset}");
        // }

        if (possibleOffsets.Count > 0)
        {
            JsonHelper.SavePossibleOffsetsToFile(possibleOffsets);
        }
    }
}
