Shader "Custom/CheckerboardWithLines"
{
    Properties
    {
        _ColorA ("Color Casilla A", Color) = (1,1,1,1)
        _ColorB ("Color Casilla B", Color) = (0,0,0,1)
        _LineColor ("Color Linea", Color) = (0,0,0,1)
        _GridX ("Cuadrados X", Float) = 8
        _GridY ("Cuadrados Y", Float) = 8
        _LineWidth ("Grosor Linea", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _ColorA;
            fixed4 _ColorB;
            fixed4 _LineColor;
            float _GridX;
            float _GridY;
            float _LineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 gridUV = i.uv * float2(_GridX, _GridY);
                float2 cell = floor(gridUV);
                float2 localUV = frac(gridUV);

                float checker = fmod(cell.x + cell.y, 2.0);
                fixed4 baseColor = lerp(_ColorA, _ColorB, checker);

                float gridLineMask = 0.0;
                gridLineMask += step(localUV.x, _LineWidth);
                gridLineMask += step(localUV.y, _LineWidth);
                gridLineMask += step(1.0 - localUV.x, _LineWidth);
                gridLineMask += step(1.0 - localUV.y, _LineWidth);
                gridLineMask = saturate(gridLineMask);

                return lerp(baseColor, _LineColor, gridLineMask);
            }
            ENDCG
        }
    }
}