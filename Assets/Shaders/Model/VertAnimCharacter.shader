Shader "Unlit/VertAnimCharacter"
{
    Properties
    {
        _BaseMap ("Base Texture",2D) = "white"{}
        _BaseColor("Base Color",Color)=(1,1,1,1)
        _AnimMap ("VAT 位置图",2D) = "black"{}
        _NormalMap("VAT 法线图",2D) = "black"{}
        _PlayPos("播放位置" ,Range(0,1)) = 0.1
        [Toggle]_IsSpecular("是否开启高光", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "RenderType"="Opaque"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
       
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        float4 _AnimMap_ST;
        half4 _BaseColor;
        half _IsSpecular;
        CBUFFER_END
        UNITY_INSTANCING_BUFFER_START(Props)
             UNITY_DEFINE_INSTANCED_PROP(float, _PlayPos)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        
        TEXTURE2D(_AnimMap);
        SAMPLER(sampler_AnimMap);
        ENDHLSL

        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            Cull Front

            HLSLPROGRAM //CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            TEXTURE2D(_NormalMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float2 uv : TEXCOORD;
                float2 uv2 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varings//这就是v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varings vert(Attributes IN)
            {
                Varings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                float playPos =UNITY_ACCESS_INSTANCED_PROP(Props, _PlayPos);
                //half3 h = tex2Dlod(_AnimMap, float4(IN.uv2,1,1)).xyz;
                half3 h = SAMPLE_TEXTURE2D_LOD(_AnimMap, sampler_AnimMap ,float2(IN.uv2.x ,playPos) ,0.0).rgb;
                //half3 h = half3(1.0,1.0,1.0);
                h = h*half3(8,8,8) - half3(4,4,0);
                
                half3 n = SAMPLE_TEXTURE2D_LOD(_NormalMap, sampler_AnimMap ,float2(IN.uv2.x ,playPos) ,0.0).rgb;
                
                n = 2 * (n - half3(0.5,0.5,0.5));
                //h = h*half3(1,-1,1);
                //h = IN.positionOS.xyz + h;
                //VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz + h);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(h);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(n);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                //OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv=TRANSFORM_TEX(IN.uv,_BaseMap);
                OUT.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                return OUT;
            }

            float4 frag(Varings IN):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                //half3 ddx_positionWS = ddx(IN.positionWS);
                //half3 ddy_positionWS = ddy(IN.positionWS);
                
                //float4 SHADOW_COORDS = TransformWorldToShadowCoord(IN.positionWS);
                //Light light = GetMainLight(SHADOW_COORDS);
                Light light = GetMainLight();
                half3 n = normalize(IN.normalWS);
                //half3 n = normalize(-cross(ddx_positionWS ,ddy_positionWS));
                half3 v = normalize(IN.viewDirWS);
                half3 h = normalize(light.direction + v);
                
                
                half nl = max(0.0,dot(light.direction ,n));
                half nh = max(0.0,dot(h ,n));
                
                //half atten = step(0.5, light.shadowAttenuation);
                half atten = 1.0;
                half3 diffuse = lerp(0.2 ,1,atten) * lerp(0.2*baseMap.xyz ,baseMap.xyz ,nl);
                half3 specular = _IsSpecular * atten * light.color * step(0.9,pow(nh ,8));
                
                
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light add_light = GetAdditionalLight(lightIndex, IN.positionWS);
                    half3 add_h = normalize(add_light.direction + v);
                
                
                    half add_nl = max(0.0,dot(add_light.direction ,n));
                    half add_nh = max(0.0,dot(add_h ,n));
                    diffuse += baseMap.xyz * add_nl* add_light.color * add_light.distanceAttenuation;
                    specular += _IsSpecular * add_light.color * add_light.distanceAttenuation * step(0.9,pow(add_nh ,8));
                }
                half3 color=diffuse*_BaseColor.xyz;
                           
                return half4(color ,1.0);
            }
            ENDHLSL  //ENDCG          
        }
        
        pass {
			Tags{ "LightMode" = "ShadowCaster" }
			
			Cull OFF
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
 
			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD;
                float2 uv2 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
 
			struct Varings
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varings vert(Attributes v)
			{
				Varings o = (Varings)0;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float playPos =UNITY_ACCESS_INSTANCED_PROP(Props, _PlayPos);
                half3 h = SAMPLE_TEXTURE2D_LOD(_AnimMap, sampler_AnimMap ,float2(v.uv2.x ,playPos) ,0.0).rgb;
                h = h*half3(8,8,8) - half3(4,4,0);
                
				o.pos = mul(UNITY_MATRIX_MVP,float4(h ,1.0));
				//o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				return o;
			}
			float4 frag(Varings i) : SV_Target
			{
			    UNITY_SETUP_INSTANCE_ID(i);
				return half4(0.0,0.0,0.0,1.0);
			}
			ENDHLSL
		}
    }
}