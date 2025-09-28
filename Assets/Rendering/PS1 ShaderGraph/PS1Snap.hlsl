#ifndef PS1_SNAP_INCLUDED
#define PS1_SNAP_INCLUDED

// Minimal include for matrices and _ScreenParams
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

// float precision version
void PS1Snap_float(float3 posOS, float pixelStep, float intensity, out float3 outPosOS)
{
    if (intensity <= 0.0001 || pixelStep <= 0.0001) { outPosOS = posOS; return; }

    float4 posWS = mul(unity_ObjectToWorld, float4(posOS, 1.0));
    float4 posCS = mul(UNITY_MATRIX_VP, posWS);

    float w = max(abs(posCS.w), 1e-6);
    float2 ndc = posCS.xy / w;

    float2 pixelSizeNDC = 2.0 / max(_ScreenParams.xy, 1.0);
    float2 stepNDC = pixelSizeNDC * pixelStep;

    float2 snappedNDC = floor(ndc / stepNDC + 0.5) * stepNDC;
    float2 finalNDC = lerp(ndc, snappedNDC, saturate(intensity));

    float4 snappedCS = posCS;
    snappedCS.xy = finalNDC * w;

    float4 snappedWS = mul(UNITY_MATRIX_I_VP, snappedCS);
    float4 snappedOS = mul(unity_WorldToObject, snappedWS);

    outPosOS = snappedOS.xyz / max(snappedOS.w, 1e-6);
}

// half precision version
void PS1Snap_half(half3 posOS, half pixelStep, half intensity, out half3 outPosOS)
{
    if (intensity <= half(0.0001) || pixelStep <= half(0.0001)) { outPosOS = posOS; return; }

    float4 posWS = mul(unity_ObjectToWorld, float4(posOS, 1.0));
    float4 posCS = mul(UNITY_MATRIX_VP, posWS);

    float w = max(abs(posCS.w), 1e-6);
    float2 ndc = posCS.xy / w;

    float2 pixelSizeNDC = 2.0 / max(_ScreenParams.xy, 1.0);
    float2 stepNDC = pixelSizeNDC * (float)pixelStep;

    float2 snappedNDC = floor(ndc / stepNDC + 0.5) * stepNDC;
    float2 finalNDC = lerp(ndc, snappedNDC, saturate((float)intensity));

    float4 snappedCS = posCS;
    snappedCS.xy = finalNDC * w;

    float4 snappedWS = mul(UNITY_MATRIX_I_VP, snappedCS);
    float4 snappedOS = mul(unity_WorldToObject, snappedWS);

    outPosOS = (half3)(snappedOS.xyz / max(snappedOS.w, 1e-6));
}

#endif // PS1_SNAP_INCLUDED