

//Top, Left, Right; Bottom, Back, Front
void Calculate6DLight_float(float3 lightDir, float3 lightCol, float3 sixPntTLR, float3 sixPntBBF, float3 tangentWS, float3 bitangentWS, float3 normalWS, float ShadowMultiplier, out float3 lightMult)
{
    //float3 spRLT = float3(sixPntTLR.z, sixPntTLR.y, sixPntTLR.x);
    float3 spBBF = float3(sixPntBBF.x, sixPntBBF.z, sixPntBBF.y);
    float3 spTLR = sixPntTLR;
    //float3 spBBF = sixPntBBF;

    float3x3 tangentTransform_World_dir = float3x3(tangentWS.rgb, bitangentWS.rgb, normalWS.rgb);
    float3 worldToTangent_dir = TransformWorldToTangent(lightDir, tangentTransform_World_dir);

    float add1a = spTLR.r * saturate(worldToTangent_dir.r);
    float add2a = spTLR.g * saturate(worldToTangent_dir.r * -1);
    float add3a = spTLR.b * saturate(worldToTangent_dir.g);

    float add1b = spBBF.r * saturate(worldToTangent_dir.g * -1);
    float add2b = spBBF.g * saturate(worldToTangent_dir.b);
    float add3b = spBBF.b * saturate(worldToTangent_dir.b * -1);

    float sceneLightsPow = lerp(1, (add1a + add2a + add3a + add1b + add2b + add3b), ShadowMultiplier);
    lightMult = clamp(sceneLightsPow, 0, 1) * lightCol;
}

// Main Light
void MainLight_float(out half3 Direction, out half3 Color)
{
#if SHADERGRAPH_PREVIEW
	Direction = half3(0.5, 0.5, 0);
	Color = 1;
#else
    Light light = GetMainLight();
    Direction = light.direction;
    Color = light.color;
#endif
}

// Additional Lights
void AdditionalLights_half(half3 MainLightColor, half3 WorldPosition, float3 sixPntTLR, float3 sixPntBBF, float3 tangentWS, float3 bitangentWS, float3 normalWS, float ShadowMultiplier, out half3 OutColor)
{
    OutColor = MainLightColor;
#if !SHADERGRAPH_PREVIEW
    
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        
        half3 Direction = 1;
        half3 Color = 1;
        Calculate6DLight_float(light.direction, light.color, sixPntTLR, sixPntBBF, tangentWS, bitangentWS, normalWS, ShadowMultiplier, Color);
        OutColor += light.distanceAttenuation * Color;
    }
#endif
}