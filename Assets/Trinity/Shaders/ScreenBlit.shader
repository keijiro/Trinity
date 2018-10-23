Shader "Hidden/Trinity/ScreenBlit"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    HLSLINCLUDE

    sampler2D _MainTex;
    float4 _Margins;

    float4 _ScreenParams;

    void Vertex(
        uint vid : SV_VertexID,
        out float4 position : SV_Position,
        out float2 texcoord : TEXCOORD
    )
    {
        float2 p = float2(vid == 1, vid == 2);
        float4 m = _Margins * (_ScreenParams.zwzw - 1);
        float2 v = p + lerp(m.xy / 2, -m.zw - m.xy / 2, p);
        position = float4(v * 4 - 1, 1, 1);
        texcoord = p * 2;
    }

    half4 Fragment(
        float4 position : SV_Position,
        float2 texcoord : TEXCOORD
    ) : SV_Target
    {
        half mask = all(texcoord < 1) * all(texcoord > 0);
        return tex2D(_MainTex, texcoord) * mask;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
