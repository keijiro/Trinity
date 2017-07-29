#include "UnityCG.cginc"

float GradNoiseHash(float p)
{
    p = frac(7.8233139 * p);
    p = ((2384.2345 * p - 1324.3438) * p + 3884.2243) * p - 4921.2354;
    return frac(p) * 2 - 1;
}

float GradNoise(float p)
{
    float ip = floor(p);
    float fp = frac(p);
    float d0 = GradNoiseHash(ip    ) *  fp;
    float d1 = GradNoiseHash(ip + 1) * (fp - 1);
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    return lerp(d0, d1, fp);
}

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

float UVRandom(float x, float y)
{
    return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
}

// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint Hash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(Hash(seed)) / 4294967295.0; // 2^32-1
}
