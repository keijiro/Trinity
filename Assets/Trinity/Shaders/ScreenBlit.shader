Shader "Hidden/Trinity/ScreenBlit"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float _Displace;
    float _VFlip;

    v2f_img vert_screen(uint vid : SV_VertexID)
    {
        float x = vid == 1 ? 2 : 0;
        float y = vid >  1 ? 2 : 0;
        v2f_img o;
        o.pos = float4(x * 2 - 1, 1 - y * 2, 0, 1);
        o.uv = float2(x, lerp(y, 1 - y, _VFlip));
        o.uv.x = o.uv.x / 3 + _Displace;
        return o;
    }

    v2f_img vert_monitor(uint vid : SV_VertexID)
    {
        float x = (vid & 1) ? 1 : 0;
        float y = (vid > 1) - (vid > 4);
        v2f_img o;
        o.pos = float4(x * 2 - 1, 1 - y * 2 / 3, 0, 1);
        o.uv = float2(x, lerp(y, 1 - y, _VFlip));
        return o;
    }

    float4 frag(v2f_img i) : SV_Target
    {
        return tex2D(_MainTex, i.uv);
    }

    ENDCG

    SubShader
    {
        ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_screen
            #pragma fragment frag
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_monitor
            #pragma fragment frag
            ENDCG
        }
    }
}
