Shader "Luna/ImageTransition"
{
    // Defines the properties that can be set in the Material inspector window.
    Properties
    {
        _Base_Map ("Base Texture",2D) = "white"{}
        _Buffer ("Buffer Texture",2D) = "white"{}
        _BaseColor("Base Color",Color)=(1,1,1,1)
        _Duration("Duration",Float)=1
        _TransitionTime("Last Time",Float)=0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
            "RenderType"="Opaque"
        }

        // Shared code block. Code in this block is shared between all passes in this subshader.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Base_Map_ST;
            float4 _Buffer_ST;
            half4 _BaseColor;
            float _Duration;
            float _TransitionTime;
        CBUFFER_END
        ENDHLSL
    
        Pass
        {
            Tags{"LightMode"="UniversalForward"}

            // HLSL code block. Unity SRP uses HLSL as the shader language.
            HLSLPROGRAM 
            #pragma vertex vert
            #pragma fragment frag

            // Use Attributes struct as input to vertex shader.
            struct Attributes
            {
                float4 positionOS : POSITION; // positionOS contains vertex positions in object space.
                float2 uv : TEXCOORD;
            };

            // Varyings struct as output from vertex shader and input to fragment shader.
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD;
            };

            TEXTURE2D(_Base_Map);
            SAMPLER(sampler_Base_Map);
            TEXTURE2D(_Buffer); 
            SAMPLER(sampler_Buffer);

            //  The vertex shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _Base_Map);
                return OUT;
            }

            // The fragment shader.
            half4  frag(Varyings IN):SV_Target
            {   
                half4 tex = SAMPLE_TEXTURE2D(_Base_Map, sampler_Base_Map, IN.uv);
                half4 buffer = SAMPLE_TEXTURE2D(_Buffer, sampler_Buffer, IN.uv);

                // Smoothly transition the color over time.
                //half4 color = lerp(buffer, tex, saturate(abs(_Time.y - _TransitionTime - 1) / _Duration));
                half4 color = lerp(buffer, tex, saturate((_Time.y - _TransitionTime) / _Duration));
                
                return color * _BaseColor;
            }
            ENDHLSL      
        }
    }
}