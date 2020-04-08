Shader "AtmosphericScattering/LightShaft"
{
    Properties
    {
        _DitheringTex ("Texture", 2D) = "white" { }
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            
            #pragma target 3.5
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            //TEXTURE2D(_MainTex);
            //SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_DitheringTex);
            SAMPLER(_PointRepeatSampler);
            uniform float3 _FrustumCorners[4];
            uniform float _DistanceScale;
            
            struct appdata
            {
                float3 vertex: POSITION;
                float2 uv: TEXCOORD0;
                uint vertexId: SV_VertexID;
            };
            
            struct v2f
            {
                float4 positionCS: SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 frustrumCornerDirWS: TEXCOORD1;
                //float4 screenPos : TEXCOORD2;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex);
                o.frustrumCornerDirWS = _FrustumCorners[v.vertexId];
                o.uv = v.uv;
                //o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }
            
            real SampleShadowMap(float3 positionWS)
            {
                float4 shadowCoords = TransformWorldToShadowCoord(positionWS);
                return MainLightRealtimeShadow(shadowCoords);
                //return SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoords.xyz);
            }
            
            half4 frag(v2f i): SV_Target
            {
                #if !defined(_MAIN_LIGHT_SHADOWS)
                    return half4(1, 1, 1, 1);
                #endif
                
                float linear01Depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv), _ZBufferParams);
                
                half offset = SAMPLE_TEXTURE2D(_DitheringTex, _PointRepeatSampler, i.uv.xy * _ScreenParams.xy / 8);
                //return half4(offset.xxx,1);
                
                // [branch]if (linear01Depth >= 0.99999) return half4(1, 1, 1, 1);
                //else
                // {
                    float3 positionWS = _WorldSpaceCameraPos + i.frustrumCornerDirWS * linear01Depth;
                    float3 rayStart = _WorldSpaceCameraPos;
                    float3 rayDir = positionWS - rayStart;
                    float rayLength = length(rayDir);
                    rayDir /= rayLength;
                    
                    // TODO 是否从rayStart + 0.5step 开始
                    int sampleCount = 16;
                    float step = rayLength / sampleCount;
                    float totalAttenuation = 0;
                    float3 p = rayStart + rayDir * step * offset;
                    
                    for (int i = 0; i < sampleCount; ++i)
                    {
                        //float3 p = rayStart + rayDir * step * i;
                        // Sample Shadow
                        real attenuation = SampleShadowMap(p);
                        totalAttenuation += attenuation;
                        p += rayDir * step;
                    }
                    totalAttenuation /= sampleCount;
                    return half4(totalAttenuation.xxx, 1);
                    // }
                }
                ENDHLSL
                
            }
        }
    }
