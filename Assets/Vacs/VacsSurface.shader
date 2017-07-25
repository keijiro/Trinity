Shader "Vacs/Standard"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0, 1)) = 0
        _MainTex("Albedo Map", 2D) = "white"{}
        _NormalMap("Normal Map", 2D) = "bump"{}
        _OcclusionMap("Occlusion", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        CGPROGRAM

        #pragma surface Surface Standard vertex:VertexModifier addshadow nolightmap nolppv
        #pragma target 5.0

        struct AppData
        {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            uint vertexID : SV_VertexID;
        };

        struct Input
        {
            float2 uv_MainTex;
            half vface : VFACE;
        };

        half4 _Color;
        half _Smoothness;
        half _Metallic;
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _OcclusionMap;


#ifdef UNITY_COMPILER_HLSL
        StructuredBuffer<float4> _PositionBuffer;
        StructuredBuffer<float4> _NormalBuffer;
        StructuredBuffer<float4> _TangentBuffer;
#endif

        void VertexModifier(inout AppData v)
        {
#ifdef UNITY_COMPILER_HLSL
            v.vertex.xyz = _PositionBuffer[v.vertexID].xyz;
            v.normal.xyz = _NormalBuffer  [v.vertexID].xyz;
            v.tangent    = _TangentBuffer [v.vertexID];
#endif
        }

        void Surface(Input input, inout SurfaceOutputStandard o)
        {
            half4 albedo = tex2D(_MainTex, input.uv_MainTex);
            half4 normal = tex2D(_NormalMap, input.uv_MainTex);
            half occlusion = tex2D(_OcclusionMap, input.uv_MainTex).g;

            o.Albedo = albedo.rgb * _Color.rgb;
            o.Alpha = albedo.a * _Color.a;

            o.Smoothness = _Smoothness;
            o.Metallic = _Metallic;

            o.Normal = UnpackNormal(normal) * (input.vface < 0 ? -1 : 1);
            o.Occlusion = occlusion;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
