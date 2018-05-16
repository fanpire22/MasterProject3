#ifndef INCLUDE_CFX_COMMON
#define INCLUDE_CFX_COMMON

//-----------------------------------------------------------------------------------
//
// getDualIdxFromMonoIdx:
//
//-----------------------------------------------------------------------------------
uint2 getDualIdxFromMonoIdx(uint monoIdx, uint width)
{
  uint2 dualIndex;

  dualIndex.x = monoIdx % width;
  dualIndex.y = monoIdx / width;

  return dualIndex;

}//getDualIdxFromMonoIdx...

//-----------------------------------------------------------------------------------
//
// CalculateSphereNormalFromTexCoord:
//
//-----------------------------------------------------------------------------------
//Calculate eye-space sphere normal from texture coordinates. 
//Input texCoord assumed to be in [-1,1] range
float3 CalculateSphereNormalFromTexCoord(float2 texCoord)
{
  float3 N;
  N.xy = texCoord;
  float r2 = dot(N.xy, N.xy);
  clip(1.0 - r2);
  N.z = sqrt(1.0 - r2);

  return N;
}//...

//-----------------------------------------------------------------------------------
//
// DepthToEyepos:
//
//-----------------------------------------------------------------------------------
//Calculate eye space position from depth
float3 DepthToEyepos(float2 texCoord, float depth, float zNear, float zFar)
{
  // Convert texture coordinates to homogeneous space
  float2 xy = texCoord * 2.0 - 1.0;
  float a = zFar / (zFar - zNear);
  float b = zFar*zNear / (zNear - zFar);

  float rd = b / (depth - a);

  return float3(xy.x, xy.y, -1.0) * rd;

}//DepthToEyepos...

//-----------------------------------------------------------------------------------
//
// ShadeDiffuseTerm:
//
//-----------------------------------------------------------------------------------
//Calculate eye space position from depth
float3 ShadeDiffuseTerm(float3 surfaceColor, float3 lightColor, float3 surfaceNormalEye, float3 lightDirEye)
{
  float diffuseCoef = saturate( dot(surfaceNormalEye, normalize(lightDirEye)) );
  return diffuseCoef * surfaceColor * lightColor;

}//ShadeDiffuseTerm...

//-----------------------------------------------------------------------------------
//
// ShadeAmbientTerm:
//
//-----------------------------------------------------------------------------------
float3 ShadeAmbientTerm(float4 surfaceColor, float3 surfaceNormalEye)
{
  #if UNITY_SHOULD_SAMPLE_SH
    float3x3 viewToWorld = transpose((float3x3)UNITY_MATRIX_V);
    half3 worldNormal = mul(viewToWorld, surfaceNormalEye);
    #if (SHADER_TARGET < 30)
      return ShadeSH9(half4(worldNormal, 1.0)) * surfaceColor;
    #else
      return ShadeSH3Order(half4(worldNormal, 1.0)) * surfaceColor;
    #endif
  #else
    return UNITY_LIGHTMODEL_AMBIENT * surfaceColor;
  #endif

}//ShadeAmbientTerm...


#endif //INCLUDE_CFX_COMMON...

