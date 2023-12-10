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
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"


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
                float2 staticLightmapUV   : TEXCOORD1;
                float2 dynamicLightmapUV  : TEXCOORD2;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionCS   : SV_POSITION; 
                float2 uv           : TEXCOORD0;
                float3 viewDirTS    : TEXCOORD1;
                half3 normalWS      : TEXCOORD2;
                half4 tangentWS     : TEXCOORD3;
                float3 positionWS   : TEXCOORD4;
                half3 vertexLighting: TEXCOORD5;
                float4 shadowCoord  : TEXCOORD6;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
                float2 dynamicLightmapUV: TEXCOORD8;
                // TODO: positionWS, vertexLighting, shadowCoord, dynamicLightmapUV, DECLARE_LIGHTMAP_OR_SH
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
                // send the tangent
                OUT.tangentWS = half4(normalInput.tangentWS.xyz, tangentSign);
                // Returning the output.
                return OUT;
            }

            void InitializeInputData(Varyings IN, half3 normalTS, out InputData inputData) {
                inputData = (InputData)0;
                    inputData.positionWS = IN.positionWS;

                inputData.positionCS = IN.positionCS;
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                // #if defined(_NORMALMAP) || defined(_DETAIL)
                //     float sgn = input.tangentWS.w;      // should be either +1 or -1
                //     float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                //     inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
                // #else
                    inputData.normalWS = IN.normalWS;
                // #endif

                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = viewDirWS;

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = IN.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif

                inputData.fogCoord = 0.0; // we don't apply fog in the guffer pass

                #ifdef _ADDITIONAL_LIGHTS_VERTEX
                    inputData.vertexLighting = IN.vertexLighting.xyz;
                #else
                    inputData.vertexLighting = half3(0, 0, 0);
                #endif

                #if defined(DYNAMICLIGHTMAP_ON)
                    inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.dynamicLightmapUV, IN.vertexSH, inputData.normalWS);
                #else
                    inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(IN.staticLightmapUV);
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
                
                // return half4(pos, 1.0);
                // pos = abs(pos);
                // bool3 mask = pos.xyz >= max(pos.yzx, pos.zxy);
                // return half4(mask * sign(IN.viewDirTS), 1.0);
                return half4(room.rgb, 1.0);
            }
            float3 interiorMapNormal(Varyings IN) {
                float2 roomUV = frac(IN.uv);
                float3 pos = float3(roomUV * 2.0 - 1.0, 1);
                float3 invDir = 1.0 / IN.viewDirTS;
                float3 k = abs(invDir) - pos * invDir;
                float kMin = min(min(k.x, k.y), k.z);
                pos += kMin * IN.viewDirTS;
                pos = abs(pos);
                bool3 mask = pos.xyz >= max(pos.yzx, pos.zxy);
                float3 normalTS = mask * -sign(IN.viewDirTS);
                float3 bitangent = IN.tangentWS.w * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
                float3 normalWS = TransformTangentToWorld(normalTS, half3x3(IN.tangentWS.xyz, bitangent.xyz, IN.normalWS.xyz));
                return normalWS;
            }
            FragmentOutput deferredFrag(Varyings IN) {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(IN.uv, surfaceData); // may need uvs mapped to flat? maybe?
                surfaceData.albedo = interiorMap(IN);
                InputData inputData;
                InitializeInputData(IN, surfaceData.normalTS, inputData); 
                inputData.normalWS = interiorMapNormal(IN);

                BRDFData brdfData;
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
                
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
                half3 color = GlobalIllumination(brdfData, inputData.bakedGI, surfaceData.occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
                FragmentOutput output = BRDFDataToGbuffer(brdfData, inputData, surfaceData.smoothness, surfaceData.emission + color, surfaceData.occlusion);

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
        
        
    }
}