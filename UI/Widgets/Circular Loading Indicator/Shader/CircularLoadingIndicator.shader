Shader "Luna/CircularLoadingIndicator"
{
    // Defines the properties that can be set in the Material inspector window.
    Properties
    {
        _BaseColor("Base Color",Color) = (1,1,1,1)
        _Radius("Radius", Float) = 0.25
        _Thickness("Thickness", Float) = 0.05
        _Speed("Speed", Float) = 1
    }
    
    SubShader
    {
        // Shared code block. Code in this block is shared between all passes in this subshader.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Radius;
            float _Thickness;
            float _Speed;
        CBUFFER_END
        ENDHLSL
        
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGB
        
        Pass
        {
            // HLSL code block. Unity SRP uses HLSL as the shader language.
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "CircularLoadingIndicator.hlsl"

            // Use Attributes struct as input to vertex shader.
            struct Attributes
            {
                float4 positionOS : POSITION; // positionOS contains vertex positions in object space.
                float2 uv : TEXCOORD0;
            };

            // Varyings struct as output from vertex shader and input to fragment shader.
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            //  The vertex shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Transform vertex positions from object space to homogenous clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // The fragment shader.
            half4 frag(Varyings IN) : SV_Target
            {
                float4 color = CircularLoadingIndicator(IN.uv, _Time.y * 50, _Radius, _Thickness, 0.1 * _Speed);
                return _BaseColor * color;
            }
            ENDHLSL
        }
    }
}
