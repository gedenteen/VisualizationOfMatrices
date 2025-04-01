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

        FindPossibleOffsetsOptimized();
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
    
    public List<Vector3> FindPossibleOffsetsOptimized()
    {
        Debug.Log("MatricesVisualizer: FindPossibleOffsetsOptimized: begin");
        
        // Создаем хэш-сет для хранения уникальных офсетов
        HashSet<Vector3> offsetCandidates = new HashSet<Vector3>(new Vector3EqualityComparer());
        bool isFirstPair = true;
        
        Debug.Log($"Model matrices count: {_modelMatrices.Count}, Space matrices count: {_spaceMatrices.Count}");
        
        // Для каждой матрицы модели
        for (int i = 0; i < _modelMatrices.Count; i++)
        {
            var modelMatrix = _modelMatrices[i];
            Debug.Log($"Processing model matrix {i} " +
                      $"(position({modelMatrix.m03}, {modelMatrix.m13}, {modelMatrix.m23}))");
            
            // Временное хранилище для офсетов, найденных для текущей матрицы модели
            HashSet<Vector3> currentOffsets = new HashSet<Vector3>(new Vector3EqualityComparer());
            
            // Для каждой матрицы пространства
            for (int j = 0; j < _spaceMatrices.Count; j++)
            {
                var spaceMatrix = _spaceMatrices[j];
                
                // Проверяем ориентацию (должна быть одинаковой)
                Vector3 modelForward = modelMatrix.GetColumn(2);
                Vector3 modelUp = modelMatrix.GetColumn(1);
                Vector3 spaceForward = spaceMatrix.GetColumn(2);
                Vector3 spaceUp = spaceMatrix.GetColumn(1);
                
                float forwardDot = Vector3.Dot(modelForward, spaceForward);
                float upDot = Vector3.Dot(modelUp, spaceUp);
                
                // Если ориентации совпадают (с учетом небольшой погрешности)
                if (forwardDot > 0.99f && upDot > 0.99f)
                {
                    // Вычисляем потенциальное смещение
                    Vector3 offset = new Vector3(
                        spaceMatrix.m03 - modelMatrix.m03,
                        spaceMatrix.m13 - modelMatrix.m13,
                        spaceMatrix.m23 - modelMatrix.m23
                    );
                    
                    // Добавляем в текущие офсеты
                    currentOffsets.Add(offset);
                    Debug.Log($"  Match found with space matrix {j}" +
                              $"(Position({spaceMatrix.m03}, {spaceMatrix.m13}, {spaceMatrix.m23}))");
                    // Debug.Log($"  Orientation match: Forward dot = {forwardDot:F3}, Up dot = {upDot:F3}");
                    Debug.Log($"  Calculated offset: ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})");
                }
                // else
                // {
                //     // Для отладки, показываем только некоторые неудачные сравнения, чтобы не засорять лог
                //     if (j % 10 == 0)
                //     {
                //         Debug.Log($"  No orientation match with space matrix {j}: Forward dot = {forwardDot:F3}, Up dot = {upDot:F3}");
                //     }
                // }
            }
            
            Debug.Log($"Found {currentOffsets.Count} potential offsets for model matrix {i}");
            
            // Если это первая пара, просто заполняем все кандидаты
            if (isFirstPair)
            {
                offsetCandidates.UnionWith(currentOffsets);
                isFirstPair = false;
                Debug.Log($"First model matrix: Initialized offset candidates with {offsetCandidates.Count} offsets");
            }
            else
            {
                int beforeCount = offsetCandidates.Count;
                // Оставляем только те офсеты, которые подходят для всех матриц модели
                offsetCandidates.IntersectWith(currentOffsets);
                Debug.Log($"After intersection: Offset candidates reduced from {beforeCount} to {offsetCandidates.Count}");
                
                // Если не осталось кандидатов, можем досрочно завершить
                if (offsetCandidates.Count == 0)
                {
                    Debug.Log("No offset candidates remain, early termination.");
                    return new List<Vector3>();
                }
            }
        }
        
        Debug.Log($"Found {offsetCandidates.Count} offset candidates. Proceeding to validation...");
        
        // Дополнительная проверка найденных офсетов для уверенности
        List<Vector3> validatedOffsets = new List<Vector3>();
        int validationCounter = 0;
        foreach (var offset in offsetCandidates)
        {
            validationCounter++;
            Debug.Log($"Validating offset {validationCounter}/{offsetCandidates.Count}: ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})");
            
            if (ValidateOffset(offset))
            {
                validatedOffsets.Add(offset);
                Debug.Log($"Validation PASSED: Offset ({offset.x:F3}, {offset.y:F3}, {offset.z:F3}) is valid");
            }
            else
            {
                Debug.Log($"Validation FAILED: Offset ({offset.x:F3}, {offset.y:F3}, {offset.z:F3}) is NOT valid");
            }
        }
        
        Debug.Log($"Final result: Found {validatedOffsets.Count} valid offsets out of {offsetCandidates.Count} candidates");
        foreach (var offset in validatedOffsets)
        {
            Debug.Log($"Valid offset: ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})");
        }
        
        return validatedOffsets;
    }

    // Валидация офсета для уверенности (проверяем, что все матрицы модели имеют соответствие)
    private bool ValidateOffset(Vector3 offset)
    {
        Debug.Log($"Starting validation for offset ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})");
        
        // Создаем хэш-сет позиций пространства для быстрого поиска
        HashSet<Vector3> spacePositions = new HashSet<Vector3>(new Vector3EqualityComparer());
        Dictionary<Vector3, Quaternion> spaceOrientations = new Dictionary<Vector3, Quaternion>(new Vector3EqualityComparer());
        
        // Подготовка данных пространства
        foreach (var spaceMatrix in _spaceMatrices)
        {
            Vector3 position = new Vector3(spaceMatrix.m03, spaceMatrix.m13, spaceMatrix.m23);
            spacePositions.Add(position);
            
            Quaternion rotation = Quaternion.LookRotation(
                spaceMatrix.GetColumn(2), spaceMatrix.GetColumn(1));
            spaceOrientations[position] = rotation;
        }
        
        Debug.Log($"Created space hash set with {spacePositions.Count} positions");
        
        // Проверяем каждую матрицу модели
        for (int i = 0; i < _modelMatrices.Count; i++)
        {
            var modelMatrix = _modelMatrices[i];
            
            // Позиция после применения смещения
            Vector3 offsetPosition = new Vector3(
                modelMatrix.m03 + offset.x,
                modelMatrix.m13 + offset.y,
                modelMatrix.m23 + offset.z
            );
            
            Debug.Log($"Validating model matrix {i}: Offset position ({offsetPosition.x:F3}, {offsetPosition.y:F3}, {offsetPosition.z:F3})");
            
            // Проверяем, есть ли такая позиция в пространстве
            bool positionMatch = ContainsWithinTolerance(spacePositions, offsetPosition);
            if (!positionMatch)
            {
                Debug.Log($"Validation FAILED for model matrix {i}: No matching position found in space");
                return false;
            }
            
            // Проверяем совпадение ориентации
            Quaternion modelRotation = Quaternion.LookRotation(
                modelMatrix.GetColumn(2), modelMatrix.GetColumn(1));
            Vector3 closestPosition = GetClosestPosition(spacePositions, offsetPosition);
            Quaternion spaceRotation = spaceOrientations[closestPosition];
            
            float angleDifference = Quaternion.Angle(modelRotation, spaceRotation);
            Debug.Log($"  Position match FOUND. Checking orientation: Angle difference = {angleDifference:F3} degrees");
            
            if (angleDifference > 5f) // Допуск в 5 градусов
            {
                Debug.Log($"Validation FAILED for model matrix {i}: Orientation mismatch, angle = {angleDifference:F3} degrees");
                return false;
            }
            
            Debug.Log($"Validation PASSED for model matrix {i}");
        }
        
        Debug.Log($"Full validation PASSED for offset ({offset.x:F3}, {offset.y:F3}, {offset.z:F3})");
        return true;
    }

    // Вспомогательный класс для сравнения векторов с учетом погрешности
    private class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        private const float Tolerance = 0.1f; // TODO: подобрать более подхоящее значение
        
        public bool Equals(Vector3 v1, Vector3 v2)
        {
            bool result = Vector3.Distance(v1, v2) < Tolerance;
            return result;
        }
        
        public int GetHashCode(Vector3 v)
        {
            // Округляем координаты для хэширования
            return Mathf.RoundToInt(v.x * 1000) ^ 
                   Mathf.RoundToInt(v.y * 1000) << 10 ^ 
                   Mathf.RoundToInt(v.z * 1000) << 20;
        }
    }

    // Проверяет, содержит ли множество вектор с учетом погрешности
    private bool ContainsWithinTolerance(HashSet<Vector3> set, Vector3 value)
    {
        foreach (var item in set)
        {
            float distance = Vector3.Distance(item, value);
            if (distance < 0.001f)
            {
                return true;
            }
        }
        return false;
    }

    // Находит ближайшую позицию в множестве
    private Vector3 GetClosestPosition(HashSet<Vector3> set, Vector3 target)
    {
        Vector3 closest = Vector3.zero;
        float minDistance = float.MaxValue;
        
        foreach (var item in set)
        {
            float distance = Vector3.Distance(item, target);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = item;
            }
        }
        
        return closest;
    }
}
