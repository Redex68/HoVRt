Shader "Custom/InteriorMapping"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _CubeMap ("Room cubemap", Cube) =  "white" {}
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags {
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalGBuffer" // THIS THING MIGHT FUCK US
        }

        Pass
        {
            Name "InteriorMapping Pass"
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            // #pragma fragment frag
            #pragma fragment deferredFrag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"      
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"       // FragmentOutput, etc.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"           // Light, etc.
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"           // Light, etc.

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float2 texcoord     : TEXCOORD0;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                // TODO: staticLightmapUV, dynamicLightmapUV
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionCS   : SV_POSITION; 
                float2 uv           : TEXCOORD0;
                float3 viewDirTS    : TEXCOORD1;
                half3 normalWS      : TEXCOORD2;
                // TODO: positionWS, tangentWS, vertexLighting, shadowCoord, dynamicLightmapUV, DECLARE_LIGHTMAP_OR_SH
                // see packages/com.unity.render-pipelines.universal/Shaders/LitGBufferPass.hlsl
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
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInput.normalWS;

                OUT.positionCS = vertexInput.positionCS;
                // taken from Emil Persson's Cubemap based interior mapping (https://forum.unity.com/threads/interior-mapping.424676/)
                OUT.uv = TRANSFORM_TEX(IN.texcoord, _CubeMap);
                float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                float3 viewDir = IN.positionOS.xyz - objCam.xyz;
                float tangentSign = IN.tangentOS.w * unity_WorldTransformParams.w;
                float3 bitangent = cross(IN.normalOS.xyz, IN.tangentOS.xyz) * tangentSign;
                OUT.viewDirTS = float3(
                    dot(viewDir, IN.tangentOS.xyz),
                    dot(viewDir, bitangent),
                    dot(viewDir, IN.normalOS.xyz)
                );
                // adjust for tiling
                OUT.viewDirTS *= _CubeMap_ST.xyx;
                // Returning the output.
                return OUT;
            }

            void InitializeInputData(Varyings IN, half3 normalTS, out InputData inputData) {
                inputData = (InputData)0;
                // #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                //     inputData.positionWS = input.positionWS;
                // #endif

                inputData.positionCS = IN.positionCS;
                // half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                // #if defined(_NORMALMAP) || defined(_DETAIL)
                //     float sgn = input.tangentWS.w;      // should be either +1 or -1
                //     float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                //     inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
                // #else
                    inputData.normalWS = IN.normalWS;
                // #endif

                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                // inputData.viewDirectionWS = viewDirWS;

                // #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                //     inputData.shadowCoord = input.shadowCoord;
                // #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                //     inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                // #else
                //     inputData.shadowCoord = float4(0, 0, 0, 0);
                // #endif

                inputData.fogCoord = 0.0; // we don't apply fog in the guffer pass

                // #ifdef _ADDITIONAL_LIGHTS_VERTEX
                //     inputData.vertexLighting = input.vertexLighting.xyz;
                // #else
                //     inputData.vertexLighting = half3(0, 0, 0);
                // #endif

                // #if defined(DYNAMICLIGHTMAP_ON)
                //     inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
                // #else
                //     inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                // #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                // inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
            } 

            half4 interiorMap(Varyings IN) {

                float2 roomUV = frac(IN.uv);
                float3 pos = float3(roomUV * 2.0 - 1.0, 1);
                //return half4(pos, 1.0);
                float3 invDir = 1.0 / IN.viewDirTS;
                float3 k = abs(invDir) - pos * invDir;
                float kMin = min(min(k.x, k.y), k.z);
                pos += kMin * IN.viewDirTS;

                half4 room = texCUBE(_CubeMap, pos);
                return half4(room.rgb, 1.0);
            }
            FragmentOutput deferredFrag(Varyings IN) {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(IN.uv, surfaceData); // may need uvs mapped to flat? maybe?
                surfaceData.albedo = interiorMap(IN);
                InputData inputData;
                InitializeInputData(IN, surfaceData.normalTS, inputData); // TODO: write this function

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
                
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
                half3 color = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
                FragmentOutput output = BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + color, surfaceData.occlusion);
                //output.GBuffer0 = interiorMap(IN);
                return output;
            }
            // TODO: Shadows, depth and normals
            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                return interiorMap(IN);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaLit

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECGLOSSMAP
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL
        }
    }
}