// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CaronteFX/Internal/Vertex Colors"
{
  SubShader {   
    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      
      // vertex input: position, color
      struct appdata {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
      };

      struct v2f {
        float4 pos : SV_POSITION;
        fixed4 color : COLOR;
      };

      // In order to work with older unity versions, this is a copy of UnityObjectToClipPos of newer versions.
      // Tranforms position from object to homogenous space
      inline float4 CFXUnityObjectToClipPos(in float3 pos){
#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
        // More efficient than computing M*VP matrix product
        return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#else
        return UnityObjectToClipPos(float4(pos, 1.0));
#endif
      }

      v2f vert(appdata v){
        v2f o;
        o.pos = CFXUnityObjectToClipPos(v.vertex);
        o.color = v.color;
        return o;
      }

      fixed4 frag(v2f i) : SV_Target{ return i.color; }

      ENDCG
    }
  }
  Fallback "Diffuse"
}
