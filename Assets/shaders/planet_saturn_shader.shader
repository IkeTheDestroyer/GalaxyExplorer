﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
Shader "Planets/Saturn"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RingsAlphaTex("Rings Alpha Tex", 2D) = "white" {}
		_SunDirection("Sun Direction", Vector) = (1,1,1,0)
		_LightAmount("Light amount (LightSide)", Vector) = (1,.2, 0, 0)
		[HDR]_AmbientColor("Ambient Color", Color) = (1,1,1,1)
		[HDR]_AlbedoMultiplier("Albedo Multiplier", Color) = (1,1,1,1)
		[HDR]_LightTint ("Sunlight Tint", Color) = (1,1,1,1)

		[HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		[HDR]_FresnelDarkSideColor("Fresnel Dark Side Color", Color) = (1,1,1,1)
		_FresnelTerm("Fresnel (Term, Offset)", Vector) = (0,0,0,0)

		_SpecParams("Specular (Power, Offset, Mask Multiplier)", Vector) = (60,0,0,0)
		_LightGammaCorrection("Light Gamma Correction (Multiplier, Power)", Vector) = (1,1,1,1)

		_OuterRingRadius("Outer Ring Radius", Float) = 1
		_InnerRingRadius("Inner Ring Radius", Float) = 0.7

		_TransitionAlpha("TransitionAlpha", Float) = 1
		_SRCBLEND("Source Blend", float) = 1
		_DSTBLEND("Destination Blend", float) = 0
	}

		SubShader
		{
			Tags
			{
				"RenderType" = "Opaque"
				"Queue" = "Geometry"
				"LightMode" = "ForwardBase"
			}

			Pass
			{
				Fog { Mode Off }
				Lighting Off
				Blend [_SRCBLEND] [_DSTBLEND]

				CGPROGRAM
				#pragma target 5.0
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile _ LOD_FADE_CROSSFADE

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "cginc/NearClip.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL0;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float  clipAmount : TEXCOORD1;
					float4 vertex : SV_POSITION;
					float3 normal : NORMAL0;
					float4 fresnel : TEXCOORD4;
					float specAmount : COLOR0;

					float ringsInteresectionRadius : TEXCOORD5;
					float3 ringsToIntersection : TEXCOORD6;
					float ringsUv : TEXCOORD7;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				sampler2D _RingsAlphaTex;
				float4 _RingsAlphaTex_ST;

				float4 _SunDirection;
				float4 _LightAmount;

				float3 _AmbientColor;
				float3 _AlbedoMultiplier;

				float4 _FresnelTerm;

				float3 _FresnelColor;
				float3 _FresnelDarkSideColor;

				float3 _SpecParams;

				float4 _LightGammaCorrection;
				float _TransitionAlpha;

				float _OuterRingRadius;
				float _InnerRingRadius;

				float3 _PlanetUp;
				float3 _PlanetCenter;
				float3 _PlanetRight;
				float3 _LightTint;

				v2f vert(appdata v)
				{
					float center = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

					v2f o;
					float3 wPos = mul(unity_ObjectToWorld, v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
					o.clipAmount = CalcVertClipAmount(wPos);

					float3 camToPixel = normalize(wPos - _WorldSpaceCameraPos);
					float3 camToSunDirection = normalize(_SunDirection.xyz - camToPixel);

					// We can do per vertex fresnel because this is a high'ish poly sphere :)
					min16float fresnel = saturate(dot(camToPixel, o.normal) * _FresnelTerm.x + _FresnelTerm.y);
					fresnel *= fresnel;

					o.fresnel.a = fresnel;

					min16float ndotl = -dot(o.normal, _SunDirection);
					min16float3 fresnelSideColor = lerp(_FresnelColor, _FresnelDarkSideColor, ndotl * .5f + .5f);

					o.fresnel.xyz = fresnelSideColor;

					min16float ndoth = saturate(dot(o.normal, camToSunDirection));
					o.specAmount = (pow(ndoth, (min16float)_SpecParams.x) + (min16float)_SpecParams.y) * (min16float)_SpecParams.z;

					// Rings shadows on Saturn
					min16float rayFromPointToPlaneIntersection = -(dot(wPos, _PlanetUp) - dot(_PlanetCenter, _PlanetUp)) / dot(_SunDirection, _PlanetUp);
					min16float3 rayFromPointIntersectionPos = wPos + rayFromPointToPlaneIntersection * _SunDirection;

					min16float3 toIntersection = rayFromPointIntersectionPos - wPos;
					o.ringsInteresectionRadius = length(rayFromPointIntersectionPos - _PlanetCenter);
					o.ringsToIntersection = toIntersection;

					min16float ringsUv = (o.ringsInteresectionRadius - (min16float)_InnerRingRadius) / ((min16float)_OuterRingRadius - (min16float)_InnerRingRadius);
					o.ringsUv = ringsUv;

					return o;
				}

				min16float4 frag(v2f i) : SV_Target
				{
					min16float ringsInteresectionRadius = i.ringsInteresectionRadius;
					min16float ringsUv = i.ringsUv;

					min16float hasRingsShadow = (ringsInteresectionRadius > (min16float)_InnerRingRadius && ringsInteresectionRadius < (min16float)_OuterRingRadius);
					min16float ringsTransmissive = tex2D(_RingsAlphaTex, min16float2(ringsUv, (min16float)1)).a;

#if LOD_FADE_CROSSFADE
					hasRingsShadow *= (min16float)unity_LODFade.x;
#endif

					min16float ringShadow = (min16float)1 - hasRingsShadow * ringsTransmissive;

					if (dot((min16float3)i.ringsToIntersection, (min16float3)_SunDirection) < 0)
					{
						ringShadow = 1;
					}

					min16float4 albedoSpec = tex2D(_MainTex, i.uv);

					min16float3 worldNormal = i.normal;

					min16float ndotl = saturate(dot(worldNormal, (min16float3)_SunDirection.xyz)) * ringShadow;

					min16float gammaIntensity = pow(ndotl * _LightGammaCorrection.x, _LightGammaCorrection.y);

					min16float3 lightAmount = gammaIntensity * lerp((min16float3)_LightAmount.x, 0, ndotl * (min16float).5 - (min16float).5) * _LightTint;
					lightAmount = (max(0, lightAmount) + (min16float3)_AmbientColor.xyz);

					min16float3 baseColor = albedoSpec.xyz * (min16float3)_AlbedoMultiplier * lightAmount;

					min16float fresnel = i.fresnel.a;
					
#if LOD_FADE_CROSSFADE
					fresnel *= (min16float)unity_LODFade.x;
#endif
					min16float3 fresnelSideColor = i.fresnel.xyz;

					min16float specAmount = i.specAmount * albedoSpec.a;
										
					min16float4 finalColor = min16float4(specAmount.xxx + lerp(baseColor, fresnelSideColor, fresnel), (min16float)_TransitionAlpha);

					return ApplyVertClipAmount(finalColor, i.clipAmount);
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
