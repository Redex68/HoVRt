Shader "Custom/InteriorMapping"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _CubeMap ("Room cubemap", Cube) =  "white" {}
        [Toggle(_RANDOMIZE)] _RANDOMIZE ("Randomize", Integer) = 0
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "InteriorMapping Pass"
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag
            #pragma shader_feature _RANDOMIZE

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"      
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float2 uvOS         : TEXCOORD0;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION; 
                float3 uvw          : TEXCOORD0;
                float3 viewDirWS    : TEXCOORD1;
            };            


            samplerCUBE _CubeMap;
            float4 _CubeMap_ST;

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // taken from Emil Persson's Cubemap based interior mapping (https://forum.unity.com/threads/interior-mapping.424676/)
                OUT.uvw = IN.positionOS * _CubeMap_ST.xyx * 0.9999 + _CubeMap_ST.zwz - 0.001;
                float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                OUT.viewDirWS = IN.positionOS.xyz - objCam.xyz;
                OUT.viewDirWS *= _CubeMap_ST.xyx;
                // Returning the output.
                return OUT;
            }

            float3 rand3(float co) {
                return frac(sin(co * float3(12.9898,78.233,43.2316)) * 43758.5453);
            }
            
            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {

                float3 roomUVW = frac(IN.uvw);
                // project 
                float3 pos = roomUVW * 2.0 - 1.0;
                float3 invDir = 1.0 / IN.viewDirWS;
                float3 k = abs(invDir) - pos * invDir;
                float kMin = min(min(k.x, k.y), k.z);
                pos += kMin * IN.viewDirWS;
            #ifdef _RANDOMIZE
                float3 flooredUV = floor(IN.uvw);
                float3 r = rand3(flooredUV.x + flooredUV.y + flooredUV.z);
                float2 cubeflip = floor(r.xy * 2.0) * 2.0 - 1.0;
                pos.xz *= cubeflip;
                pos.xz = r.z > 0.5 ? pos.xz : pos.zx;
            #endif
                half4 room = texCUBE(_CubeMap, pos.xyz);
                return half4(room.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}