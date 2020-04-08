#ifndef _AC2_MATH_INCLUDED
#define _AC2_MATH_INCLUDED

#ifndef PI
#define PI 3.1415926535
#endif

//-----------------------------------------------------------------------------------------
// RaySphereIntersection 求直线与球交点
// 球方程 (P-C)^2 = R^2
// 射线方程 P = P0 + d * s
// 返回两个交点的根 d1，d2，其中d1 < d2，如果d < 0 ，说明交点在射线后方
//-----------------------------------------------------------------------------------------
float2 RaySphereIntersection(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius)
{
	rayOrigin -= sphereCenter;
	float a = dot(rayDir, rayDir);
	float b = 2.0 * dot(rayOrigin, rayDir);
	float c = dot(rayOrigin, rayOrigin) - (sphereRadius * sphereRadius);
	float d = b * b - 4 * a * c;
	if (d < 0)
	{
		return -1;
	}
	else
	{
		d = sqrt(d);
		return float2(-b - d, -b + d) / (2 * a);
	}
}

//-----------------------------------------------------------------------------------------
// Phase Functions
//-----------------------------------------------------------------------------------------
//From PragueCUNI-Elek-Oskar09
//原本的Rayleigh Phase在0度和180度的地方一样，这个可以让其在180度的地方小一些
float RayleighPhase(float cosAngle)
{
    return (3.0 / (16.0 * PI)) * (1 + (cosAngle * cosAngle));
}

float RayleighPhaseAdHoc(float cosAngle)
{
    return (8.0 / 10) * (7.0/ 5 + 0.5 * cosAngle);
}

float MiePhaseHG(float cosAngle,float g)
{
    float oneMinusG = 1 - g;
    float g2 = g * g;
    return (1.0 / (4.0 * PI)) * oneMinusG * oneMinusG / pow( abs(1.0 + g2 - 2.0*g*cosAngle), 3.0 / 2.0);
}

float MiePhaseHGCS(float cosAngle,float g)
{
    float g2 = g * g;
    return (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g2)) / (2.0 * (2.0 + g2))) * ((1 + cosAngle * cosAngle) / (pow( abs(1 + g2 - 2 * g*cosAngle), 3.0 / 2.0)));
}

//-----------------------------------------------------------------------------------------
// Particle Density at height
//-----------------------------------------------------------------------------------------
float ParticleDensity(float height,float scaleHeight)
{
    return exp(-height / scaleHeight);
}

float2 ParticleDensity(float height,float2 scaleHeight)
{
    return exp(-height.xx / scaleHeight.xy);
}

#endif