Shader "Hidden/Trinity/PostFx"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
        _LineColor("", Color) = (0, 0, 0, 1)
        _FillColor1("", Color) = (0, 0, 1)
        _FillColor2("", Color) = (1, 0, 0)
        _FillColor3("", Color) = (1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

    float4 _LineColor;
    float3 _FillColor1;
    float3 _FillColor2;
    float3 _FillColor3;

    float _ColorThreshold;
    float _DepthThreshold;
    float _DitherStrength;

    fixed Dither(float2 uv)
    {
        const float3x3 pattern = float3x3(0, 7, 3, 6, 5, 2, 4, 1, 8) / 9 - 0.5;
        uint2 iuv = uint2(uv * _MainTex_TexelSize.zw) % 3;
        return pattern[iuv.x][iuv.y] * _DitherStrength / 3;
    }

    fixed4 frag(v2f_img i) : SV_Target
    {
        float4 duv = float4(0, 0, _MainTex_TexelSize.xy);

        float c11 = tex2D(_MainTex, i.uv + duv.xy).g;
        float c12 = tex2D(_MainTex, i.uv + duv.zy).g;
        float c21 = tex2D(_MainTex, i.uv + duv.xw).g;
        float c22 = tex2D(_MainTex, i.uv + duv.zw).g;

        float d11 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + duv.xy);
        float d12 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + duv.zy);
        float d21 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + duv.xw);
        float d22 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv + duv.zw);

        float g_c = length(float2(c11 - c22, c12 - c21));
        g_c = saturate((g_c - _ColorThreshold) * 40);

        float g_d = length(float2(d11 - d22, d12 - d21));
        g_d = saturate((g_d - _DepthThreshold) * 40);

        fixed edge = max(g_c, g_d);

        fixed luma = dot(tex2D(_MainTex, i.uv).rgb, 1.0 / 3) + Dither(i.uv);
        fixed3 fill = luma > 2.0 / 3 ? _FillColor3 : (luma > 1.0 / 3 ? _FillColor2 : _FillColor1);

        return fixed4(lerp(fill, _LineColor.rgb, edge * _LineColor.a), 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
