Shader "CaronteFX/Oven/Corpuscles Diffuse" {
  Properties {
  	corpuscleColor_("Corpuscle Color", Color) = (0.5, 0.5, 0.5, 1)
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
  		float4 _CorpuscleColor;
  
      StructuredBuffer<float4> _BufQuadVertex;
      StructuredBuffer<float3> _BufCorpusclePosition;
  
      uniform float4 _LightColor0;
  
  		struct vs_output 
  		{
  			float4 pos          : SV_POSITION;	
        float4 posEyeSpace  : TEXCOORD0;
  			float2 texCoord     : TEXCOORD1;
        float4 lightDirEye  : TEXCOORD2;
  		};
  
      vs_output vert (uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
  		{
        vs_output o;
  
        float4 scaledQuadVertex = _BufQuadVertex[vertexId] * _CorpuscleRadius * _CorpuscleScale;
        float4 posEyeSpace = float4( UnityObjectToViewPos( _BufCorpusclePosition[instanceId] ), 1.0) + scaledQuadVertex;

        o.pos         = mul(UNITY_MATRIX_P, posEyeSpace);
        o.posEyeSpace = posEyeSpace;
        o.texCoord    = _BufQuadVertex[vertexId];
        o.lightDirEye = mul(UNITY_MATRIX_V, float4(normalize(_WorldSpaceLightPos0.xyz), 0.0));
  
  			return o; 
  		}
  	
      float4 frag (vs_output i) : SV_TARGET
  		{
  			// calculate eye-space sphere normal from texture coordinates
        float3 N = CalculateSphereNormalFromTexCoord(i.texCoord);

        float3x3 viewToWorld = transpose((float3x3)UNITY_MATRIX_V);
        float3 worldN = mul(viewToWorld, N);
        float3 ambient = ShadeSH9(half4(worldN, 1.0));

        float diffuseCoef = max(0.0, dot(N, i.lightDirEye));
        float3 diffuse = _CorpuscleColor * _LightColor0.rgb * diffuseCoef;

        return fixed4(ambient + diffuse, 1.0);
  		}

  		ENDCG
  	}// SubShader...

  }// Shader...

  Fallback off
}
