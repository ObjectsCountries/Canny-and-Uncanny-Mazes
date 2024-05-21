using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//shoutouts to chatGPT for all this (i made a few tweaks though)
public class TextureGeneratorUncanny : MonoBehaviour
{
    public Texture2D[] textures;
    public int minWidthHeight;
    public int maxWidthHeight;
    public Texture2D whiteBG;
    internal int gridDimensions;
    internal int[,] textureIndices;
    internal Texture2D finalTexture;
    internal RenderTexture renderTexture;
    private Material mat;
    new private MeshRenderer renderer;
    public UncannyMazeTile[,] layout;

    public void Awake()
    {
        gridDimensions = Random.Range(minWidthHeight, maxWidthHeight + 1);
        List<int> usedNumbers = new List<int>();
        int cellSize = Mathf.FloorToInt(Mathf.Min(Screen.width, Screen.height) / gridDimensions);
        layout = new UncannyMazeTile[gridDimensions, gridDimensions];
        Texture2D gridTexture = new Texture2D(gridDimensions * cellSize, gridDimensions * cellSize, TextureFormat.RGB24, false);
        textureIndices = new int[gridDimensions, gridDimensions];
        int randomIndex;
        for (int y = 0; y < gridDimensions; y++)
        {
            for (int x = 0; x < gridDimensions; x++)
            {
                if (gridDimensions == 4)
                {
                    do
                    {
                        randomIndex = Random.Range(0, textures.Length);
                    } while (usedNumbers.Contains(randomIndex) && usedNumbers.Distinct().Count() != 10);
                    usedNumbers.Add(randomIndex);
                }
                else
                {
                    randomIndex = Random.Range(0, textures.Length);
                }
                textureIndices[y, x] = randomIndex + 1;
                layout[y, x] = new UncannyMazeTile(x, y, randomIndex, gridDimensions);
                Texture2D texture = textures[randomIndex];
                Color[] pixels = texture.GetPixels();

                for (int j = 0; j < cellSize; j++)
                {
                    for (int i = 0; i < cellSize; i++)
                    {
                        int destX = x * cellSize + i;
                        int destY = (gridDimensions - y - 1) * cellSize + j;

                        if (destX < gridTexture.width && destY < gridTexture.height)
                        {
                            int sourceX = Mathf.FloorToInt(i * (float)texture.width / cellSize);
                            int sourceY = Mathf.FloorToInt(j * (float)texture.height / cellSize);

                            gridTexture.SetPixel(destX, destY, pixels[sourceY * texture.width + sourceX]);
                        }
                    }
                }
            }
        }

        gridTexture.Apply();

        // Create a RenderTexture to scale up the texture
        int finalResolution = gridDimensions * cellSize; // Choose the desired final resolution
        renderTexture = new RenderTexture(finalResolution, finalResolution, 24);
        Graphics.Blit(gridTexture, renderTexture);

        // Create a new Texture2D from the RenderTexture
        finalTexture = new Texture2D(finalResolution, finalResolution, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        finalTexture.filterMode = FilterMode.Trilinear;
        finalTexture.ReadPixels(new Rect(0, 0, finalResolution, finalResolution), 0, 0, false);
        finalTexture.Apply();
        RenderTexture.active = null;

        renderer = GetComponent<MeshRenderer>();
        mat = new Material(Shader.Find("KT/Blend Unlit"));
        mat.mainTexture = finalTexture;
        renderer.material = mat;
    }

    internal void changeTexture(Texture2D t)
    {
        RenderTexture.active = renderTexture;
        t.Apply();
        RenderTexture.active = null;
        mat.mainTexture = t;
        renderer.material = mat;
    }
}
