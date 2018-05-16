// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CaronteFX/Oven/Fluid Compositor" {
  Properties{
    _MainTex("Texture", 2D) = "white" {}
  }
  
  SubShader 
  {

  	Pass 
    {
      //Standard image effect
      Cull Off 
      ZWrite Off 
      ZTest Always
  
  		CGPROGRAM
  		#pragma target 5.0
  		#pragma vertex vert
  		#pragma fragment frag
  
  		#include "UnityCG.cginc"
      #include "CFX-COMMON.cginc"
  		
  		float4 _FluidColor;
      
      sampler2D _MainTex;
      sampler2D _CameraDepthTexture;
      sampler2D _FluidDepthTexture;

      float4 _MainTex_TexelSize;
      float4 _LightColor0;

      struct vertInput
      {
        float4 vertex   : POSITION;
        float2 texCoord : TEXCOORD0;
      };
  
  		struct v2f 
  		{
  			float4 vertex       : SV_POSITION;	
        float2 texCoord     : TEXCOORD0;
        float4 lightDirEye  : TEXCOORD1;
  		};
  
  		v2f vert (vertInput i)
  		{
  			v2f o;
        o.vertex      = UnityObjectToClipPos(i.vertex);
        o.texCoord    = i.texCoord;
        o.lightDirEye = mul(UNITY_MATRIX_V, float4(normalize(_WorldSpaceLightPos0.xyz), 0.0));

        return o;
  		}

      struct fragOutput
      {
        float4 color : SV_TARGET;
        float depth  : DEPTH;
      };
  	
      fragOutput frag (v2f i)
  		{
        fragOutput o;

        float2 texCoord = i.texCoord;
// Flip sampling of the Texture: 
// The main Texture texel size will have negative Y).
#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
        {
          texCoord.y = 1.0 - texCoord.y;
        }
#endif

        float4 mainColor = tex2D(_MainTex, i.texCoord);
#if UNITY_REVERSED_Z
        float mainDepth = 1.0 - tex2D(_CameraDepthTexture, texCoord);
#else
        float mainDepth = tex2D(_CameraDepthTexture, texCoord);
#endif

        //reconstruct eye space pos from depth
        float fluidDepth = tex2D(_FluidDepthTexture, texCoord);
        float zNear = _ProjectionParams.y;
        float zFar  = _ProjectionParams.z;
        float3 eyePos = DepthToEyepos(texCoord, fluidDepth, zNear, zFar);

        //calculate normal using finite differences
        float3 ddx = ddx_fine(eyePos);
        float3 ddy = ddy_fine(eyePos);
        float3 N = normalize(cross(ddy, ddx));

        float3 diffuse = ShadeDiffuseTerm(_FluidColor, _LightColor0.rgb, N, i.lightDirEye);
        float3 ambient = ShadeAmbientTerm(_FluidColor, N);

        float4 fluidColor = float4(diffuse + ambient, 1.0);
        //float4 fluidColor = float4(N * 0.5 + 0.5, 1.0);

        bool fluidCulled = (mainDepth <= fluidDepth);

        o.color = (fluidCulled) ? mainColor : fluidColor;
        o.depth = (fluidCulled) ? mainDepth : fluidDepth;

        return o;
  		}

  		ENDCG
  	}// SubShader...

  }// Shader...

  Fallback off
}
