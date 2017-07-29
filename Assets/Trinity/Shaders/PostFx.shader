Shader "Hidden/Trinity/PostFx"
{
    Properties
    {
        _MainTex("", 2D) = "white" {}
        _OverlayTex("", 2D) = "black" {}
    }

    CGINCLUDE

    #include "Common.cginc"
    #include "SimplexNoise2D.hlsl"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

    float4 _LineColor;
    float3 _FillColor1;
    float3 _FillColor2;
    float3 _FillColor3;

    float _ColorThreshold;
    float _DepthThreshold;

    sampler2D _OverlayTex;
    float4 _OverlayColor;

    float _ScanlineNoise;
    float _BlockDisplace;
    float _OverlayShuffle;
    float _OverlaySlits;
    float _OverlayWiper;
    float _Progress;

    // Select color for posterization
    fixed3 SelectColor(float x, fixed3 c1, fixed3 c2, fixed3 c3)
    {
        return x < 1 ? c1 : (x < 2 ? c2 : c3);
    }

    // Invertion filter
    fixed Invert(fixed input, fixed intensity)
    {
        return lerp(input, 1 - input, intensity);
    }

    // Dithering with the 3x3 Bayer matrix
    fixed Dither3x3(float2 uv)
    {
        const float3x3 pattern = float3x3(0, 7, 3, 6, 5, 2, 4, 1, 8) / 9 - 0.5;
        uint2 iuv = uint2(uv * _MainTex_TexelSize.zw) % 3;
        return pattern[iuv.x][iuv.y];
    }

    // Edge detection with the Roberts cross operator
    fixed DetectEdge(float2 uv)
    {
        float4 duv = float4(0, 0, _MainTex_TexelSize.xy);

        float c11 = tex2D(_MainTex, uv + duv.xy).g;
        float c12 = tex2D(_MainTex, uv + duv.zy).g;
        float c21 = tex2D(_MainTex, uv + duv.xw).g;
        float c22 = tex2D(_MainTex, uv + duv.zw).g;

        float g_c = length(float2(c11 - c22, c12 - c21));
        g_c = saturate((g_c - _ColorThreshold) * 40);

        float d11 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + duv.xy);
        float d12 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + duv.zy);
        float d21 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + duv.xw);
        float d22 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv + duv.zw);

        float g_d = length(float2(d11 - d22, d12 - d21));
        g_d = saturate((g_d - _DepthThreshold) * 40);

        return max(g_c, g_d);
    }

    // Moving slits animation
    fixed Slits(float2 uv)
    {
        float x = uv.x;
        float gn = snoise(float2(x * 20, _Progress * 1.6)) +
                   snoise(float2(x * 30, _Progress * 1.1)) / 2;
        return abs(gn) < _OverlaySlits;
    }

    // Wiping animation
    fixed Wiper(float2 uv, float offs)
    {
        float t = _Progress + offs;

        uint wave = floor(t) * 100 + offs * 10000;
        float param = frac(t);

        float y1 = smoothstep(Random(wave + 0) / 2, Random(wave + 1) / 2 + 0.5, param);
        float y2 = smoothstep(Random(wave + 2) / 2, Random(wave + 3) / 2 + 0.5, param);
        float y3 = smoothstep(Random(wave + 4) / 2, Random(wave + 5) / 2 + 0.5, param);

        float thresh = lerp(lerp(y1, y2, saturate(uv.y * 2)), y3, saturate(uv.y * 2 - 1));
        return frac(t / 2) < 0.5 ? uv.x > thresh : uv.x < thresh;
    }

    fixed4 frag(v2f_img i) : SV_Target
    {
        float2 uv = i.uv;

        // Scanline noise
        {
            float y = uv.y * 93.4731 - _Progress * 388.332;
            uv.x += GradNoise(y) * _ScanlineNoise * 0.1;
        }

        // Block diaplace
        {
            float2 p = floor(uv * float2(80, 20)) * float2(1.92132, 2.13724);
            p += float2(_Progress * 4, 0);
            float2 n = snoise_grad(p).xy * 0.05;
            uv += n * n * (n < 0 ? -1 : 1) * _BlockDisplace;
        }

        // Wrapping around
        uv = frac(uv);

        // Shuffle the overlay texture
        float2 uv_ovr = uv;
        {
            float speed = Random(floor(uv.x * 9) + 1000);
            speed = (speed > 0.5 ? 1 : -1) * (abs(speed - 0.5) + 0.1) * 0.5;
            uv_ovr.x = frac(frac(uv.x * 9) / 9 + speed * _Progress);
            uv_ovr.x = lerp(uv.x, uv_ovr.x, _OverlayShuffle);
        }

        // Sample textures
        fixed3 c_src = tex2D(_MainTex, uv).rgb; // source color
        fixed c_ovr = tex2D(_OverlayTex, uv_ovr).a; // overlay mask

        // Edge detection and posterization
        fixed edge = DetectEdge(uv);
        fixed luma = LinearRgbToLuminance(c_src);
        fixed sel = luma * 3 + Dither3x3(i.uv);
        fixed3 fill = SelectColor(sel, _FillColor1, _FillColor2, _FillColor3);
        fixed3 c_out = lerp(fill, _LineColor.rgb, edge * _LineColor.a);

        // Overlay animations
        c_ovr = Invert(c_ovr, Slits(i.uv));
        c_ovr = Invert(c_ovr, Wiper(i.uv, 0) * _OverlayWiper);
        c_ovr = Invert(c_ovr, Wiper(i.uv, 1.0 / 3) * _OverlayWiper);
        c_ovr = Invert(c_ovr, Wiper(i.uv, 2.0 / 3) * _OverlayWiper);

        // Color invertion with overlay
        fixed3 c_inv = saturate(_OverlayColor.rgb - c_out + c_out.ggr);
        c_out = lerp(c_out, c_inv, c_ovr * _OverlayColor.a);

        return fixed4(GammaToLinearSpace(c_out), 1);
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
