using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }
    
    public static void SavePossibleOffsetsToFile(List<Matrix4x4> possibleOffsets)
    {
        if (possibleOffsets == null)
        {
            Debug.Log("JsonHelper: SavePossibleOffsetsToFile: " +
                "possibleOffsets is null");
            return;
        }
        
        MatrixData[] arrayForJson = new MatrixData[possibleOffsets.Count];
        for (int i = 0; i < possibleOffsets.Count; i++)
        {
            arrayForJson[i] = ConvertMatrixToData(
                possibleOffsets[i]);
        }
        
        Wrapper<MatrixData> wrapper = new Wrapper<MatrixData>();
        wrapper.array = arrayForJson;
        string json = JsonUtility.ToJson(wrapper, true);
    
        string directory = Path.Combine(Application.dataPath, 
            "_Project", "Data");
        // Directory.CreateDirectory(directory);
        string filePath = Path.Combine(directory, "possibleOffsets.json");
        File.WriteAllText(filePath, json);
    
        Debug.Log($"JsonHelper: SavePossibleOffsetsToFile: " +
            $"Saved json at {filePath}");
    }
    
    private static MatrixData ConvertMatrixToData(Matrix4x4 matrix)
    {
        MatrixData md = new MatrixData();
        md.m00 = matrix.m00;
        md.m01 = matrix.m01;
        md.m02 = matrix.m02;
        md.m03 = matrix.m03;
        md.m10 = matrix.m10;
        md.m11 = matrix.m11;
        md.m12 = matrix.m12;
        md.m13 = matrix.m13;
        md.m20 = matrix.m20;
        md.m21 = matrix.m21;
        md.m22 = matrix.m22;
        md.m23 = matrix.m23;
        md.m30 = matrix.m30;
        md.m31 = matrix.m31;
        md.m32 = matrix.m32;
        md.m33 = matrix.m33;
        return md;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
