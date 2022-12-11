Shader "Unlit/EffectAroundX"
{
    Properties
    {
        _BaseMap ("Base Texture",2D) = "white"{}
        _BaseColor("Base Color",Color)=(1,1,1,1)
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
       
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Tags{"LightMode"="UniversalForward"}
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
            struct Varings//这就是v2f
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
                
                float3 x = normalize(mul(float3(1, 0, 0) ,UNITY_MATRIX_MV));
                float3 z = normalize(cross(x ,float3(0,1,0)));
                
                float3 frontDir = normalize(mul(z ,UNITY_MATRIX_IT_MV));
                float3 upDir = normalize(cross(frontDir, float3(1,0,0)));
                
                float3 center = float3(0 ,0 ,0);
                float3 pos = IN.positionOS.xyz - center;
                pos = float3(center + pos.x * float3(1,0,0) + pos.y * upDir + pos.z * frontDir);
               
                VertexPositionInputs positionInputs = GetVertexPositionInputs(pos);
                OUT.positionCS = positionInputs.positionCS;
                  
                OUT.uv=TRANSFORM_TEX(IN.uv,_BaseMap);
                OUT.positionWS = positionInputs.positionWS;
                return OUT;
            }

            float4 frag(Varings IN):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 color=baseMap * _BaseColor;
                return color;
            }
            ENDHLSL  //ENDCG          
        }
    }
}