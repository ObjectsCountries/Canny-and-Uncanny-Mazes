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
    internal Texture2D finalTexture = null;
    internal RenderTexture renderTexture = null;
    private Material mat;
    new private MeshRenderer renderer;
    public List<List<UncannyMazeTile>> layout = null;
    List<UncannyMazeTile> tempList = null;
    internal Dictionary<int, int> amountOfEachNumber = new Dictionary<int, int>();
    private List<int> usedNumbers = new List<int>();
    private int cellSize;
    private Texture2D gridTexture = null;
    private Texture2D texture = null;
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
        if (layout == null)
        {
            layout = new List<List<UncannyMazeTile>>();
        }
        else
        {
            layout.Clear();
        }
        if (gridTexture == null)
        {
            gridTexture = new Texture2D(gridDimensions * cellSize, gridDimensions * cellSize, TextureFormat.RGB24, false);
        }
        else
        {
            gridTexture.Resize(gridDimensions * cellSize, gridDimensions * cellSize, TextureFormat.RGB24, false);
        }
        int randomIndex;
        if (tempList == null)
        {
            tempList = new List<UncannyMazeTile>();
        }
        for (int y = 0; y < gridDimensions; y++)
        {
            tempList.Clear();
            for (int x = 0; x < gridDimensions; x++)
            {
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, textures.Length);
                } while (usedNumbers.Contains(randomIndex) && usedNumbers.Distinct().Count() < 10);
                usedNumbers.Add(randomIndex);
                amountOfEachNumber[randomIndex] += 1;
                tempList.Add(new UncannyMazeTile(x, y, randomIndex, gridDimensions));
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
            layout.Add(new List<UncannyMazeTile>(tempList));
        }

        gridTexture.Apply();

        // Create a RenderTexture to scale up the texture
        finalResolution = gridDimensions * cellSize; // Choose the desired final resolution
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(finalResolution, finalResolution, 24);
        }
        else
        {
            renderTexture.Release();
            renderTexture.width = finalResolution;
            renderTexture.height = finalResolution;
            renderTexture.depth = 24;
        }
        Graphics.Blit(gridTexture, renderTexture);

        // Create a new Texture2D from the RenderTexture
        if (finalTexture == null)
        {
            finalTexture = new Texture2D(finalResolution, finalResolution, TextureFormat.RGB24, false);
        }
        else
        {
            finalTexture.Resize(finalResolution, finalResolution, TextureFormat.RGB24, false);
        }
        RenderTexture.active = renderTexture;
        finalTexture.filterMode = FilterMode.Trilinear;
        finalTexture.ReadPixels(new Rect(0, 0, finalResolution, finalResolution), 0, 0, false);
        finalTexture.Apply();
        RenderTexture.active = null;

        renderer = GetComponent<MeshRenderer>();
        if (mat == null)
        {
            mat = new Material(Shader.Find("KT/Blend Unlit"));
        }
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
