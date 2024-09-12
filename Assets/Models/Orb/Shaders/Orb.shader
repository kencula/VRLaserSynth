Shader "Custom/Orb"
{
    Properties
    {
        _Color("Color", Color) = (0,0.9,0.9,1)
        _Effect("Effect", Float) = 0.1
        _Brightness("Brightness", Float) = 0.5
        _Size("Size", Float) = 0.5
        _Definition("Definition", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparency" "LightMode" = "ForwardBase" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float outNorm : TEXCOORD3;
            };

            float4 _Color;
            float _Effect;
            float _Size;
            float _Brightness;
            float _Definition;

            v2f vert (appdata_base v)
            {
                float3 norm = normalize(v.normal);
                v.vertex.xyz += sin(_Time + v.vertex.x * v.vertex.y * v.vertex.z * 500) * norm * _Effect * _Size;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = norm;
                o.viewDir = WorldSpaceViewDir(v.vertex);
                o.outNorm = dot(norm, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = _Color;
                col.xyz += sin(_Time) * i.normal / 5;
                col.xyz += min(dot(i.normal, i.viewDir) * _Brightness * 0.1, 0.3) + 0.1;
                col.xyz *= 1 - min(i.outNorm, 1) * _Definition * 0.5;

                return col;
            }
            ENDCG
        }
    }
}
