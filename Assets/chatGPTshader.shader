Shader "Custom/SeamlessGridShader" {
    Properties {
        _MainTex0 ("Texture 0", 2D) = "white" {}
        _MainTex1 ("Texture 1", 2D) = "white" {}
        _MainTex2 ("Texture 2", 2D) = "white" {}
        _MainTex3 ("Texture 3", 2D) = "white" {}
        _MainTex4 ("Texture 4", 2D) = "white" {}
        _MainTex5 ("Texture 5", 2D) = "white" {}
        _MainTex6 ("Texture 6", 2D) = "white" {}
        // Add more texture properties as needed (_MainTex3, _MainTex4, etc.)
        // Make sure to adjust the number of properties to match NUM_IMAGES

        _GridRows ("Grid Rows", Range(5,8)) = 5
        _GridCols ("Grid Columns", Range(5,8)) = 5
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        // Number of images to display in the grid
        // Adjust this value based on your requirement
        #define NUM_IMAGES 7

        // Custom properties to hold the textures
        sampler2D _MainTex0;
        sampler2D _MainTex1;
        sampler2D _MainTex2;
        sampler2D _MainTex3;
        sampler2D _MainTex4;
        sampler2D _MainTex5;
        sampler2D _MainTex6;
        // Add more sampler2D properties as needed (_MainTex3, _MainTex4, etc.)
        // Make sure to adjust the number of properties to match NUM_IMAGES

        // Custom properties to hold the grid size
        int _GridRows;
        int _GridCols;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            // Calculate the row and column indices for the grid cell
            float row = floor(IN.uv_MainTex.y * _GridRows);
            float col = floor(IN.uv_MainTex.x * _GridCols);

            // Randomly select an image index between 0 and NUM_IMAGES - 1
            float imageIndex = frac(sin(row * 10.0 + col * 20.0) * 43758.5453) * NUM_IMAGES;

            // Sample the texture based on the selected image index
            fixed4 color;
            if (imageIndex < 1.0) {
                color = tex2D(_MainTex0, IN.uv_MainTex);
            } else if (imageIndex < 2.0) {
                color = tex2D(_MainTex1, IN.uv_MainTex);
            } else if (imageIndex < 3.0) {
                color = tex2D(_MainTex2, IN.uv_MainTex);
            } else if (imageIndex < 4.0) {
                color = tex2D(_MainTex3, IN.uv_MainTex);
            } else if (imageIndex < 5.0) {
                color = tex2D(_MainTex4, IN.uv_MainTex);
            } else if (imageIndex < 6.0) {
                color = tex2D(_MainTex5, IN.uv_MainTex);
            } else {
                color = tex2D(_MainTex6, IN.uv_MainTex);
            }

            o.Albedo = color.rgb;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
