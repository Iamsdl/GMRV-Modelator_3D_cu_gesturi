using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TextureGenerator : MonoBehaviour
{
    public static Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        //Texture3D texture = CreateTexture3D(32, points, out float max);
        //UnityEditor.AssetDatabase.CreateAsset(texture, "Assets/HDRP test/t3d.asset");
    }

    public static Texture3D CreateTexture3D(int textureSize, Vector3[] points)
    {
        Vector3 min = GetMin(points);
        Translate(points, -min);
        Vector3 max = GetMax(points);
        Vector3 maxinv = new Vector3(1 / max.x, 1 / max.y, 1 / max.z);
        Scale(points, maxinv);

        Scale(points, 0.5f * textureSize);
        Translate(points, Vector3.one * textureSize * 0.25f);

        Color[] colorArray = new Color[textureSize * textureSize * textureSize];
        Texture3D texture = new Texture3D(textureSize, textureSize, textureSize, TextureFormat.RGBA32, true);
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                for (int z = 0; z < textureSize; z++)
                {
                    Vector3 voxel = new Vector3(x, y, z);
                    float[] distances = new float[points.Length - 1];
                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        if (IsInSegment(voxel, points[i], points[i + 1]))
                        {
                            distances[i] = DistanceToLine(voxel, points[i], points[i + 1]);
                        }
                        else
                        {
                            distances[i] = Mathf.Min(Vector3.Distance(voxel, points[i]), Vector3.Distance(voxel, points[i + 1]));
                        }
                    }

                    Vector3 sum = Vector3.zero;

                    for (int i = 0; i < points.Length - 1; i++)
                    {
                        sum += (points[i + 1] - points[i]) / (distances[i] + 1);
                    }

                    sum /= textureSize;
                    Color c = new Color((sum.x + 1) / 2, (sum.y + 1) / 2, (sum.z + 1) / 2);
                    SetColor(colorArray, x, y, z, textureSize, c);
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }

    private static bool IsInSegment(Vector3 voxel, Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 vector1 = startPoint - voxel;
        Vector3 normal1 = startPoint - endPoint;
        Vector3 vector2 = endPoint - voxel;
        Vector3 normal2 = endPoint - startPoint;

        return Vector3.Dot(vector1, normal1) > 0 && Vector3.Dot(vector2, normal2) > 0;

        //float dot = Vector3.Dot(vector1, vector2);
        //return dot < -0.5f;
    }

    private void NormalizeDistances(float[] distances, float sum)
    {
        if (sum == 0)
        {
            sum = 1;
        }
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] /= sum;
        }
    }

    private float GetSum(float[] distances)
    {
        float sum = 0;
        for (int i = 0; i < distances.Length; i++)
        {
            sum += distances[i];
        }
        return sum;
    }

    private static float DistanceToLine(Vector3 point, Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 direction = (endPoint - startPoint).normalized;
        return Vector3.Cross((endPoint - point), direction).magnitude;
    }

    private static void Scale(Vector3[] points, Vector3 max)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Vector3.Scale(points[i], max);
        }
    }
    private static void Scale(Vector3[] points, float max)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] *= max;
        }
    }

    private static void Translate(Vector3[] points, Vector3 value)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += value;
        }
    }

    public static Vector3 GetMin(Vector3[] points)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        for (int i = 0; i < points.Length; i++)
        {
            min = Vector3.Min(min, points[i]);
        }
        return min;
    }
    public static Vector3 GetMax(Vector3[] points)
    {
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < points.Length; i++)
        {
            max = Vector3.Max(max, points[i]);
        }
        return max;
    }

    private static void SetColor(Color[] colorArray, int x, int y, int z, int size, Color c)
    {
        colorArray[x + (y * size) + (z * size * size)] = c;
    }

    // Update is called once per frame
    void Update()
    {

    }



}

public static class Vector3Extentions
{
    public static float MaxComponent(this Vector3 a)
    {
        return Mathf.Max(a.x, a.y, a.z);
    }
}