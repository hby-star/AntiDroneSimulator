void SphereDistortionUv_float(float2 uv, float2 screenPos, float distortionCurve, float distortionIntensity, float maxDistortion, out half2 newScreenPos)
{
    float2 cartesianUv = -float2(uv.x * 2 - 1, uv.y * 2 - 1);
    float distFromCenter = distance(cartesianUv, float2(0, 0));
    if (distFromCenter > 1)
    {
        newScreenPos = screenPos;
        return;
    }

    float outherDistortionAmount = 1 - distFromCenter;
    float innerDistortionAmount = distFromCenter;
    float distortAmount = min(pow(innerDistortionAmount, distortionCurve), outherDistortionAmount); //mix inner and outer distortion
    
    distortAmount *= distortionIntensity * .2;
    distortAmount = min(distortAmount, maxDistortion);
    
    float2 distorsionDir = cartesianUv * distortAmount; 
    
    newScreenPos = screenPos + distorsionDir;
}