#ifndef _SCATTERING_CORE_INCLUDE
    #define _SCATTERING_CORE_INCLUDE
    
    #include "ACMath.hlsl"
    
    uniform float _MieG;
    uniform float3 _ScatteringR;
    uniform float3 _ScatteringM;
    uniform float3 _ExtinctionR;
    uniform float3 _ExtinctionM;
    uniform float _DistanceScale;
    
    TEXTURE2D(_LightShaft);
    SAMPLER(sampler_LightShaft);
    
    void ApplyScattering(inout half4 color, float3 positionWS, float4 screenPos)
    {
        float height = _WorldSpaceCameraPos.y;
        float3 viewDir = positionWS - _WorldSpaceCameraPos.xyz;
        float distance = length(viewDir);
        viewDir /= distance;
        float cosAngle = dot(viewDir, _MainLightPosition.xyz);
        
        float3 scatCoef = _ScatteringR + _ScatteringM;
        float3 scatAngularCoef = _ScatteringR * RayleighPhase(cosAngle) + _ScatteringM * MiePhaseHG(cosAngle, _MieG);
        
        cosAngle = dot(viewDir, _MainLightPosition.xyz);
        
        float3 extinction = exp( - (_ExtinctionR + _ExtinctionM) * distance * _DistanceScale);
        float3 inscattering = _MainLightColor.xyz * scatAngularCoef / scatCoef * (1 - extinction);
        
        #if defined(_LIGHT_SHAFT)
            half occulusion = SAMPLE_TEXTURE2D(_LightShaft, sampler_LightShaft, screenPos.xy/screenPos.w).x;
            //occulusion = occulusion * occulusion * occulusion;
            inscattering.xyz *= occulusion;
        #endif
        
        #if defined(_AP_DEBUG_INSCATTERING)
            color.xyz = inscattering;
        #elif defined(_AP_DEBUG_EXTINCTION)
            color.xyz = extinction;
        #else
            color.xyz = color.xyz * extinction + inscattering;
        #endif        
    }
    
    #if defined(_AERIAL_PERSPECTIVE)
        #define APPLY_SCATTERING(color, positionWS, screenUv) ApplyScattering(color, positionWS.xyz, screenUv);
        
    #else
        #define APPLY_SCATTERING(color, positionWS, screenUv)
    #endif
    
    
#endif