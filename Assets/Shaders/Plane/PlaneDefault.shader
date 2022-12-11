Shader "Unlit/PlaneDefault"
{
    Properties
    {
        _BaseMap ("Base Texture",2D) = "white"{}
        _BaseColor("Base Color",Color)=(1,1,1,1)
        _RimLightWidth("_OutLineWidth" ,Range(0,1)) = 0.2
        [Toggle]_IsSpecular("是否开启高光", Float) = 1
        [Toggle(_IS_OPEN_SHADOW_ON)]_IS_OPEN_SHADOW ("_IS_OPEN_SHADOW", Float) = 0
        [Toggle(_IS_ADDLIGHTS_ON)]_IS_ADDLIGHTS ("_IS_ADDLIGHTS", Float) = 0
        [Toggle(_IS_RimLight_ON)]_IS_RimLight ("_IS_RimLight_ON", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }
        HLSLINCLUDE
        #include "../Common/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
       
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half _IsSpecular;
        half _RimLightWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Tags{"LightMode"="UniversalForward"}

            HLSLPROGRAM //CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing
            #pragma multi_compile _ _IS_OPEN_SHADOW_ON
            #pragma multi_compile _ _IS_ADDLIGHTS_ON
            #pragma multi_compile _ _IS_RimLight_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float2 uv : TEXCOORD;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varings//这就是v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD;
                float3 positionWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
#ifdef _IS_RimLight_ON                
                float3 normalVS : TEXCOORD4;
                float4 scrPos : TEXCOORD5;
#endif           
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            float CorrectDepth(float rawDepth)
            {
                float persp = LinearEyeDepth(rawDepth ,_ZBufferParams);
                float ortho = (_ProjectionParams.z-_ProjectionParams.y)*(1-rawDepth)+_ProjectionParams.y;
                return lerp(persp,ortho,unity_OrthoParams.w);
            }

            Varings vert(Attributes IN)
            {
                Varings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                  
                OUT.uv=TRANSFORM_TEX(IN.uv,_BaseMap);
                OUT.positionWS = positionInputs.positionWS;
                OUT.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
#ifdef _IS_RimLight_ON                  
                OUT.normalVS = normalize(mul(normalInputs.normalWS, UNITY_MATRIX_IT_MV));
                OUT.scrPos = ComputeScreenPos(positionInputs.positionCS);
#endif                
                return OUT;
            }

            half4 frag(Varings IN):SV_Target
            {
#ifdef _IS_RimLight_ON                   
                float3 nVS = normalize(IN.normalVS);
                float d = SampleSceneDepth(IN.scrPos.xy/IN.scrPos.w + nVS.xy * 5 * _RimLightWidth / _ScreenParams.xy);
                d = CorrectDepth(d);
#endif 
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
#ifdef _IS_OPEN_SHADOW_ON                 
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(SHADOW_COORDS);
#else
                Light light = GetMainLight();
#endif                
                half3 n = normalize(IN.normalWS);
                half3 v = normalize(IN.viewDirWS);
                half3 h = normalize(light.direction + v);
                
                
                half nl = max(0.0,dot(light.direction ,n));
                half nh = max(0.0,dot(h ,n));
                
#ifdef _IS_OPEN_SHADOW_ON                  
                half atten = step(0.5, light.shadowAttenuation);
#else
                half atten = 1.0;
#endif  
                half3 diffuse = atten * lerp(0.5*baseMap.xyz ,baseMap.xyz ,nl) + (1 - atten) * 0.4 * baseMap.xyz* light.color;
                half3 specular = _IsSpecular * atten * light.color * step(0.8,pow(nh ,8));
                
                
                
#ifdef _IS_ADDLIGHTS_ON    
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light add_light = GetAdditionalLight(lightIndex, IN.positionWS);
                    half3 add_h = normalize(add_light.direction + v);
                
                
                    half add_nl = max(0.0,dot(add_light.direction ,n));
                    half add_nh = max(0.0,dot(add_h ,n));
                    diffuse += baseMap.xyz * add_nl* add_light.color * add_light.distanceAttenuation;
                    specular += _IsSpecular * add_light.color * add_light.distanceAttenuation * step(0.8,pow(add_nh ,8));
                }
#endif
                half3 color=diffuse*_BaseColor.xyz;
#ifdef _IS_RimLight_ON                 
    #if UNITY_REVERSED_Z
                    float aa = saturate(d - CorrectDepth(IN.scrPos.z/IN.scrPos.w));                
    #else
                    float aa = saturate(CorrectDepth(IN.scrPos.z/IN.scrPos.w) - d);
    #endif
                half4 final = lerp( half4(color ,1.0) ,half4(1,0,0,1) ,aa );
#else
                half4 final = half4(color ,1.0);     
#endif
                return final;
            }
            ENDHLSL  //ENDCG          
        }
        
        pass {
			Tags{ "LightMode" = "ShadowCaster" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile _IS_OPEN_SHADOW_ON
            #pragma multi_compile _IS_ADDLIGHTS_ON
 
			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			sampler2D _MainTex;
			float4 _MainTex_ST;
 
			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				return o;
			}
			float4 frag(v2f i) : SV_Target
			{
			    UNITY_SETUP_INSTANCE_ID(i);
				return half4(0.0,0.0,0.0,1.0);
			}
			ENDHLSL
		}
    }
}