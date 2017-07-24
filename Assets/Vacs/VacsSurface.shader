Shader "Vacs/Standard"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        CGPROGRAM

        #pragma surface Surface Standard vertex:VertexModifier nolightmap addshadow
        #pragma target 3.0

        struct AppData
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            uint vid : SV_VertexID;
        };

        struct Input
        {
            half vface : VFACE;
        };

        half3 _Color;

#ifdef UNITY_COMPILER_HLSL
        StructuredBuffer<float4> _PositionBuffer;
        StructuredBuffer<float4> _NormalBuffer;
        StructuredBuffer<float4> _TangentBuffer;
#endif

        void VertexModifier(inout AppData v)
        {
#ifdef UNITY_COMPILER_HLSL
            v.vertex.xyz = _PositionBuffer[v.vid].xyz;
            v.normal.xyz = _NormalBuffer[v.vid].xyz;
            v.tangent.xyz = _TangentBuffer[v.vid];
#else
            v.vertex.xyz = float3(
                UVRandom(v.texcoord.x, v.texcoord.y),
                UVRandom(v.texcoord.x+1, v.texcoord.y+1),
                UVRandom(v.texcoord.x+2, v.texcoord.y+2)
            );
#endif
        }

        void Surface(Input input, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color;
            o.Normal = float3(0, 0, (input.vface < 0) ? -1 : 1);
        }

        ENDCG
    }
    FallBack "Diffuse"
}
