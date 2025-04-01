using UnityEngine;

[System.Serializable]
public class MatrixData
{
    public float m00, m01, m02, m03;
    public float m10, m11, m12, m13;
    public float m20, m21, m22, m23;
    public float m30, m31, m32, m33;

    public Matrix4x4 ToMatrix()
    {
        Matrix4x4 mat = new Matrix4x4();
        mat.m00 = m00;
        mat.m01 = m01;
        mat.m02 = m02;
        mat.m03 = m03;
        mat.m10 = m10;
        mat.m11 = m11;
        mat.m12 = m12;
        mat.m13 = m13;
        mat.m20 = m20;
        mat.m21 = m21;
        mat.m22 = m22;
        mat.m23 = m23;
        mat.m30 = m30;
        mat.m31 = m31;
        mat.m32 = m32;
        mat.m33 = m33;
        return mat;
    }
}
