using System.Collections.Generic;
using UnityEngine;

public class MatricesVisualizer : MonoBehaviour
{
    [SerializeField] private TextAsset _modelJson;
    [SerializeField] private TextAsset _spaceJson;

    private List<Matrix4x4> _modelMatrices;
    private List<Matrix4x4> _spaceMatrices;

    void Start()
    {
        ParseJsons();
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
}
