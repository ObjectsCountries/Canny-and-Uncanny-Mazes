//thank you chatGPT
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public Texture2D[] textures;
    public static int rows = 5;
    public static int cols = 5;

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

        // Create the final grid texture
        Texture2D gridTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill the entire texture with light gray pixels
        Color[] grayColor = new Color[textureWidth * textureHeight];
        for (int i = 0; i < grayColor.Length; i++)
        {
            grayColor[i] = Color.grey; // You can adjust the color if needed
        }
        gridTexture.SetPixels(grayColor);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int randomIndex = Random.Range(0, textures.Length);
                Texture2D texture = textures[randomIndex];
                Color[] pixels = texture.GetPixels();

                int startX = x * cellWidth + (cellWidth - texture.width) / 2;
                int startY = (rows - y - 1) * cellHeight + (cellHeight - texture.height) / 2;

                for (int j = 0; j < texture.height; j++)
                {
                    for (int i = 0; i < texture.width; i++)
                    {
                        int destX = startX + i;
                        int destY = startY + j;

                        if (destX < textureWidth && destY < textureHeight)
                        {
                            gridTexture.SetPixel(destX, destY, pixels[j * texture.width + i]);
                        }
                    }
                }
            }
        }

        gridTexture.Apply();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("KT/Mobile/DiffuseTint"));
            material.mainTexture = gridTexture;
            renderer.material = material;
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on the plane.");
        }
    }
}
