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
    internal Dictionary<int, int> amountOfEachNumber = new Dictionary<int, int>();
    private List<int> usedNumbers = new List<int>();
    private int cellSize;
    private Texture2D gridTexture, texture;
    private int destX, destY, sourceX, sourceY, finalResolution;

    public void Awake()
    {
        gridDimensions = Random.Range(minWidthHeight, maxWidthHeight + 1);
        usedNumbers.Clear();
        amountOfEachNumber.Clear();
        amountOfEachNumber.Add(0, 0);
        amountOfEachNumber.Add(1, 0);
        amountOfEachNumber.Add(2, 0);
        amountOfEachNumber.Add(3, 0);
        amountOfEachNumber.Add(4, 0);
        amountOfEachNumber.Add(5, 0);
        amountOfEachNumber.Add(6, 0);
        amountOfEachNumber.Add(7, 0);
        amountOfEachNumber.Add(8, 0);
        amountOfEachNumber.Add(9, 0);
        cellSize = Mathf.FloorToInt(Mathf.Min(Screen.width, Screen.height) / gridDimensions);
        layout = new UncannyMazeTile[gridDimensions, gridDimensions];
        gridTexture = new Texture2D(gridDimensions * cellSize, gridDimensions * cellSize, TextureFormat.RGB24, false);
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
                amountOfEachNumber[randomIndex] += 1;
                textureIndices[y, x] = randomIndex;
                layout[y, x] = new UncannyMazeTile(x, y, randomIndex, gridDimensions);
                texture = textures[randomIndex];
                Color[] pixels = texture.GetPixels();

                for (int j = 0; j < cellSize; j++)
                {
                    for (int i = 0; i < cellSize; i++)
                    {
                        destX = x * cellSize + i;
                        destY = (gridDimensions - y - 1) * cellSize + j;

                        if (destX < gridTexture.width && destY < gridTexture.height)
                        {
                            sourceX = Mathf.FloorToInt(i * (float)texture.width / cellSize);
                            sourceY = Mathf.FloorToInt(j * (float)texture.height / cellSize);

                            gridTexture.SetPixel(destX, destY, pixels[sourceY * texture.width + sourceX]);
                        }
                    }
                }
            }
        }

        gridTexture.Apply();

        // Create a RenderTexture to scale up the texture
        finalResolution = gridDimensions * cellSize; // Choose the desired final resolution
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
        if (renderTexture != null && mat != null && renderer != null)
        {
            RenderTexture.active = renderTexture;
            t.Apply();
            RenderTexture.active = null;
            mat.mainTexture = t;
            renderer.material = mat;
        }
    }
}
