Shader "Unlit/EffectHpUI"
{
    Properties
    {
        _BaseMap ("Base Texture",2D) = "white"{}
        _BaseColor("血条颜色",Color)=(1,1,1,1)
        _BgColor("血条面板颜色",Color)=(1,1,1,1)
        _ShowRatio("血条显示比例",Range(0.0,1.0)) =0.9
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
       
       
        UNITY_INSTANCING_BUFFER_START(Props)
             UNITY_DEFINE_INSTANCED_PROP(float, _ShowRatio)
        UNITY_INSTANCING_BUFFER_END(Props)      
         
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        half4 _BgColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            ZTest off
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM //CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float2 uv : TEXCOORD;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varings vert(Attributes IN)
            {
                Varings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                float3 frontDir = normalize(mul(float3(0, 0, 1), UNITY_MATRIX_IT_MV));
                float3 upDir = normalize(mul(float3(0, 1, 0), UNITY_MATRIX_IT_MV));
                float3 rightDir = normalize(cross(upDir, frontDir));
                
                float3 center = float3(0 ,0 ,0);
                float3 pos = IN.positionOS.xyz - center;
                pos = float3(center + pos.x * rightDir + pos.y * upDir + pos.z * frontDir);
               
                VertexPositionInputs positionInputs = GetVertexPositionInputs(pos);
                OUT.positionCS = positionInputs.positionCS;
                  
                OUT.uv=TRANSFORM_TEX(IN.uv,_BaseMap);
                OUT.positionWS = positionInputs.positionWS;
                return OUT;
            }

            float4 frag(Varings IN):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float showRatio = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowRatio);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                if(baseMap.a < 0.1)
                    discard;
                half2 showUv = 1.11111 * IN.uv - half2(0.05555,0.05555);
                half3 showColor = _BaseColor.xyz;
                if(showUv.x < 0 || showUv.x > showRatio)
                {
                    showColor = float3(0,0,0);
                }
                
                if(showUv.x > showRatio && showUv.x < 1)
                {
                    showColor = _BgColor.xyz;
                }
                
                
                if(showUv.y < 0.15 || showUv.y > 0.85)
                {
                    showColor = float3(0,0,0);
                }
                
               
                half4 color=baseMap + half4(showColor ,0.0);
                return color;
            }
            ENDHLSL  //ENDCG          
        }
    }
}