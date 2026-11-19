// Opaque prop shader for the demo scene.
//
// The props used Shader.Find("Standard"), which is Built-in only and rendered
// the whole arena magenta in URP. Dropping the material entirely fixed that but
// left everything Unity's default light grey, and these are additive effects
// authored on black: on a near-white floor they wash out to nothing.
//
// So: a hand-written opaque shader with a flat tint and one cheap directional
// term for shape. It uses only UnityCG.cginc and UnityObjectToWorldNormal,
// which the engine ships in every pipeline, so it resolves in Built-in, URP and
// HDRP alike.
Shader "Taproot/VFX Prop"
{
    Properties
    {
        _Color ("Colour", Color) = (0.2,0.2,0.24,1)
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

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; float shade : TEXCOORD0; };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 n = normalize(UnityObjectToWorldNormal(v.normal));
                float3 l = normalize(float3(0.4, 0.9, -0.35));
                o.shade = saturate(dot(n, l)) * 0.65 + 0.35;   // keep shadows lit enough to read
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(_Color.rgb * i.shade, 1);
            }
            ENDCG
        }
    }
}
