#ifndef INCLUDE_CFX_BASIC
#define INCLUDE_CFX_BASIC

static const uint WIDTH = 256;

//-----------------------------------------------------------------------------------
//
// getDualIdxFromMonoIdx:
//
//-----------------------------------------------------------------------------------
uint2 getDualIdxFromMonoIdx(uint monoIdx)
{
  uint2 dualIndex;

  dualIndex.x = monoIdx % WIDTH;
  dualIndex.y = monoIdx / WIDTH;

  return dualIndex;

}//getDualIdxFromMonoIdx...


 //-----------------------------------------------------------------------------------
 //
 // setQuaternion
 //
 //-----------------------------------------------------------------------------------
void setQuaternion(inout float4x4 tr, in float4 q)
{
  const float X = q.x;
  const float Y = q.y;
  const float Z = q.z;
  const float W = q.w;

  const float xx = X * X;
  const float xy = X * Y;
  const float xz = X * Z;
  const float xw = X * W;
  const float yy = Y * Y;
  const float yz = Y * Z;
  const float yw = Y * W;
  const float zz = Z * Z;
  const float zw = Z * W;

  tr._m00 = 1 - 2 * (yy + zz);
  tr._m01 = 2 * (xy + zw);
  tr._m02 = 2 * (xz - yw);

  tr._m10 = 2 * (xy - zw);
  tr._m11 = 1 - 2 * (xx + zz); 
  tr._m12 = 2 * (yz + xw);

  tr._m20 = 2 * (xz + yw);
  tr._m21 = 2 * (yz - xw);
  tr._m22 = 1 - 2 * (xx + yy);

}//setQuaternion...


 //-----------------------------------------------------------------------------------
 //
 // setTranslation
 //
 //-----------------------------------------------------------------------------------
void setTranslation(inout float4x4 tr, in float3 t)
{
  tr._m30 = t.x;
  tr._m31 = t.y;
  tr._m32 = t.z;

}//setTranslation...


 //-----------------------------------------------------------------------------------
 //
 // setScale
 //
 //-----------------------------------------------------------------------------------
void setScale(inout float4x4 tr, in float3 s)
{
  tr._m00 *= s.x;
  tr._m01 *= s.x;
  tr._m02 *= s.x;

  tr._m10 *= s.y;
  tr._m11 *= s.y;
  tr._m12 *= s.y;

  tr._m20 *= s.z;
  tr._m21 *= s.z;
  tr._m22 *= s.z;

}//setScale...


 //-----------------------------------------------------------------------------------
 //
 // getTRS
 //
 //-----------------------------------------------------------------------------------
float4x4 getTRS(in float3 t, in float4 q, in float3 s)
{
  float4x4 tr = float4x4( 0, 0, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 0,
                          0, 0, 0, 1 );

  setQuaternion(tr, q);
  setTranslation(tr, t);
  setScale(tr, s);

  return tr;
}//getTRS...


//-----------------------------------------------------------------------------------
//
// adjoint
//
//-----------------------------------------------------------------------------------
void adjoint(in float3x3 a, out float3x3 result)
{
  result._m00 = a._m11 * a._m22 - a._m12 * a._m21;
  result._m01 = a._m02 * a._m21 - a._m01 * a._m22;
  result._m02 = a._m01 * a._m12 - a._m02 * a._m11;

  result._m10 = a._m12 * a._m20 - a._m10 * a._m22;
  result._m11 = a._m00 * a._m22 - a._m02 * a._m20;
  result._m12 = a._m02 * a._m10 - a._m00 * a._m12;

  result._m20 = a._m10 * a._m21 - a._m11 * a._m20;
  result._m21 = a._m01 * a._m20 - a._m00 * a._m21;
  result._m22 = a._m00 * a._m11 - a._m01 * a._m10;

}//adjoint...


//-----------------------------------------------------------------------------------
//
// inverse3x3
//
//-----------------------------------------------------------------------------------
void inverse3x3(in float3x3 a, out float3x3 result)
{
  float det = determinant(a);
  
  float invDet = 0.0;
  if (det != 0.0)
  {
    invDet = 1.0 / det;
  }

  float3x3 adjointMatrix;
  adjoint(a, adjointMatrix);

  result = adjointMatrix * invDet;
}




#endif //INCLUDE_CFX_BASIC...

