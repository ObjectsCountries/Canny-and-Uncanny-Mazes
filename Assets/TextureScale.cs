using UnityEngine;

public static class TextureScale
{
    public static void Bilinear(Texture2D texture, int newWidth, int newHeight)
    {
        Color[] newPixels = new Color[newWidth * newHeight];

        for (int y = 0; y < newHeight; y++)
        {
            float v = (float)y / (float)newHeight;
            int y1 = (int)Mathf.Floor(v * texture.height);
            int y2 = Mathf.Min(y1 + 1, texture.height - 1);
            float yPercent = v * texture.height - y1;

            for (int x = 0; x < newWidth; x++)
            {
                float u = (float)x / (float)newWidth;
                int x1 = (int)Mathf.Floor(u * texture.width);
                int x2 = Mathf.Min(x1 + 1, texture.width - 1);
                float xPercent = u * texture.width - x1;

                Color c00 = texture.GetPixel(x1, y1);
                Color c10 = texture.GetPixel(x2, y1);
                Color c01 = texture.GetPixel(x1, y2);
                Color c11 = texture.GetPixel(x2, y2);

                Color newColor = Color.Lerp(Color.Lerp(c00, c10, xPercent), Color.Lerp(c01, c11, xPercent), yPercent);
                newPixels[y * newWidth + x] = newColor;
            }
        }

        texture.Resize(newWidth, newHeight);
        texture.SetPixels(newPixels);
    }
}
