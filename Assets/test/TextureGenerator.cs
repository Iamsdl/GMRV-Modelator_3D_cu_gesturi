using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Texture3D texture = CreateTexture3D(256);
        UnityEditor.AssetDatabase.CreateAsset(texture, "Assets/test/t3d.asset");
    }

    Texture3D CreateTexture3D(int size)
    {
        Color[] colorArray = new Color[size * size * size];
        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, true);
        float r = 1.0f / (size - 1.0f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Color c = new Color(x*r, y*r, z*r, 1.0f);
                    colorArray[x + (y * size) + (z * size * size)] = c;
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
