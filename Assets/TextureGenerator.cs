using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public Texture2D[] textures;
    public int rows = 5;
    public int cols = 5;

    void Start()
    {
        if (textures.Length == 0)
        {
            Debug.LogError("Please assign textures to the TextureGenerator script in the inspector.");
            return;
        }

        int cellWidth = textures[0].width;
        int cellHeight = textures[0].height;

        int textureWidth = cols * cellWidth;
        int textureHeight = rows * cellHeight;

        // Create a new Texture2D array to store the copies of the original textures
        Texture2D[] copiedTextures = new Texture2D[textures.Length];

        // Copy each texture into the new Texture2D array
        for (int i = 0; i < textures.Length; i++)
        {
            copiedTextures[i] = new Texture2D(textures[i].width, textures[i].height, TextureFormat.RGBA32, false);
            copiedTextures[i].SetPixels(textures[i].GetPixels());
            copiedTextures[i].Apply();
        }

        // Create the final grid texture
        Texture2D gridTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int randomIndex = Random.Range(0, copiedTextures.Length);
                Texture2D texture = copiedTextures[randomIndex];
                Color[] pixels = texture.GetPixels();

                int startX = x * cellWidth + (cellWidth - texture.width) / 2;
                int startY = (rows - y - 1) * cellHeight + (cellHeight - texture.height) / 2;

                for (int j = 0; j < texture.height; j++)
                {
                    for (int i = 0; i < texture.width; i++)
                    {
                        gridTexture.SetPixel(startX + i, startY + j, pixels[j * texture.width + i]);
                    }
                }
            }
        }

        gridTexture.Apply();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = gridTexture;
            renderer.material = material;
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on the plane.");
        }
    }
}
