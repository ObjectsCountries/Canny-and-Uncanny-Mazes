using UnityEngine;

public class CubeTextureRandomizer : MonoBehaviour
{
    public Texture2D[] textures;
    public int rows = 5;
    public int cols = 5;

    void Start()
    {
        if (textures.Length == 0)
        {
            Debug.LogError("Please assign textures to the CubeTextureRandomizer script in the inspector.");
            return;
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the cube.");
            return;
        }

        Material material = new Material(Shader.Find("Custom/SeamlessGridShader"));
        material.SetInt("_GridRows", rows);
        material.SetInt("_GridCols", cols);
        material.SetInt("_NumImages", textures.Length);

        for (int i = 0; i < textures.Length; i++)
        {
            material.SetTexture("_MainTex" + i, textures[i]);
        }

        renderer.material = material;
    }
}
