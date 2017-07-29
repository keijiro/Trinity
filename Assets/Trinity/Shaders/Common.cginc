#include "UnityCG.cginc"

float2 GradNoiseHash(float2 p)
{
    p = frac(mul(float2x2(1.2989833, 7.8233198, 6.7598192, 3.4857334), p));
    p = ((2384.2345 * p - 1324.3438) * p + 3884.2243) * p - 4921.2354;
    return normalize(frac(p) * 2 - 1);
}

float GradNoise(float2 p)
{
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(GradNoiseHash(ip), fp);
    float d01 = dot(GradNoiseHash(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(GradNoiseHash(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(GradNoiseHash(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
}

float Random(float x, float y)
{
    return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
}

