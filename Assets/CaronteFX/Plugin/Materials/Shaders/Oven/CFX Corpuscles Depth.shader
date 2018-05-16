Shader "CaronteFX/Oven/Corpuscles Depth" {
  Properties {
  	corpuscleScale_("Corpuscle Scale", Float) = 1.0
  }
  
  SubShader 
  {
  	Pass 
    {
  		ZTest LEqual 
  
  		CGPROGRAM
  		#pragma target 5.0

  		#pragma vertex vert
  		#pragma fragment frag
  
  		#include "UnityCG.cginc"
      #include "CFX-COMMON.cginc"
  		
  		float _CorpuscleRadius;
  		float _CorpuscleScale;
  
      StructuredBuffer<float4> _BufQuadVertex;
      StructuredBuffer<float3> _BufCorpusclePosition;
  
      uniform float4 _LightColor0;
  
  		struct v2f 
  		{
  			float4 pos          : SV_POSITION;	
        float4 posEyeSpace  : TEXCOORD0;
  			float2 texCoord     : TEXCOORD1;
  		};
  
  		v2f vert (uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
  		{
  			v2f o;
  
        float4 scaledQuadVertex = _BufQuadVertex[vertexId] * _CorpuscleRadius * _CorpuscleScale;
        float4 posEyeSpace = float4(UnityObjectToViewPos(_BufCorpusclePosition[instanceId]), 1.0) + scaledQuadVertex;

        o.pos         = mul(UNITY_MATRIX_P, posEyeSpace);
        o.posEyeSpace = posEyeSpace;
        o.texCoord    = _BufQuadVertex[vertexId];
  
  			return o; 
  		}
  	
      struct fragOutput
      {
        float color : SV_TARGET;
        float depth : depth;
      };

      fragOutput frag (v2f i)
  		{
        fragOutput o;

  			// calculate eye-space sphere normal from texture coordinates
  			float3 N = CalculateSphereNormalFromTexCoord(i.texCoord);

        //calculate depth
        float3 pixelPos = i.posEyeSpace.xyz + (N * _CorpuscleRadius * _CorpuscleScale);
        float4 clipSpacePos = mul( UNITY_MATRIX_P, float4(pixelPos, 1.0) );

        float depth = clipSpacePos.z / clipSpacePos.w;
        o.depth = depth;

#if defined(UNITY_REVERSED_Z)
        o.color = 1.0 - depth;
#else
        o.color = depth;
#endif
        return o;
  		}

  		ENDCG
  	}// SubShader...

  }// Shader...

  Fallback off
}
