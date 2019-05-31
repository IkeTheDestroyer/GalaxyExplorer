Shader "GalaxyExplorer/TractorBeam"
{
    Properties
    {
        _BaseColor("Base color", Color) = (1.0, 1.0, 1.0, 1.0)
        _ActiveColor("Active color", Color) = (0.0, 0.0, 1.0, 1.0)
        _Size("Point Size", Float) = .02
        _Speed("Rotation Speed", Float) = 2.0
        _Coverage("Coverage", Range(0,1)) = 1
        [Toggle]_Active("Active", Float) = 0
        [HideInInspector]_SelfTime("Time", Float) = 0.0
        _ClipFadeDistance("Camera clip fade distance units", Float) = .1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4                // "LessEqual"
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "LightMode" = "ForwardBase"}

        pass
        {

            ZWrite Off
            ZTest[_ZTest]
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing

            // We only target the HoloLens (and the Unity editor), so take advantage of shader model 5.
            #pragma target 5.0
            #pragma only_renderers d3d11

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            float4 _BaseColor;
            float4 _ActiveColor;
            float _Size;
            float _Active;
            float _Speed;
            float _Coverage;
            float _ClipFadeDistance;
            float _SelfTime;
            float3 _Points[10];
            float3 _Normals[10];
            float3 _Tangents[10];
            float3 _BiNormals[10];
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 randoms : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 clipPos : SV_POSITION;
                float4 randoms : TEXCOORD1;
                float4 color : COLOR;
//                float2 proximity_size: TEXCOORD2;
//                float3 normal : NORMAL;
//                float3 randBlend : TEXCOORD0;
//                float clip : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // inverseW is to counteract the effect of perspective-correct interpolation so that the lines
            // look the same thickness regardless of their depth in the scene.
            struct g2f
            {
                float4 clipPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 randoms : TEXCOORD01;
                float4 color : COLOR;
//                float2 proximity_size : TEXCOORD2;
//                float3 normal : NORMAL;
//                float3 randBlend :TEXCOORD0;
//                float clip : TEXCOORD1;
//                float4 light :TEXCOORD2;
//                float4 color : COLOR;
                // LIGHTING_COORDS(2,3)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float4x4 rotationMatrix(float3 normalizedAxis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                float oc = 1.0 - c;
                return float4x4(oc * normalizedAxis.x * normalizedAxis.x + c, oc * normalizedAxis.x * normalizedAxis.y - normalizedAxis.z * s, oc * normalizedAxis.z * normalizedAxis.x + normalizedAxis.y * s,                                      0.0,
                                oc * normalizedAxis.x * normalizedAxis.y + normalizedAxis.z * s,                           oc * normalizedAxis.y * normalizedAxis.y + c,           oc * normalizedAxis.y * normalizedAxis.z - normalizedAxis.x * s,  0.0,
                                oc * normalizedAxis.z * normalizedAxis.x - normalizedAxis.y * s,                           oc * normalizedAxis.y * normalizedAxis.z + normalizedAxis.x * s,  oc * normalizedAxis.z * normalizedAxis.z + c,           0.0,
                                0.0,                                                         0.0,                                0.0,                                1.0);
            }

            v2g vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                v2g o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

//              rotate aroud the local forward axis
                float angle = _Time.x*v.randoms.x*_Speed;
                float3x3 zAxisRotMat = float3x3(
                    cos(angle), -sin(angle), 0,
                    sin(angle),  cos(angle), 0,
                    0         ,  0         , 1
                );
                    
                float3 rotated = mul(zAxisRotMat, v.vertex);

                float uShift = v.uv.y-(1-_Coverage);
                float a = step(0,uShift);
                float u = saturate(uShift)*10*.9;
                
                int indexA = floor(u);
                int indexB = ceil(u);
                float blend = 1-(indexB-u);
                float3 pA = _Points[indexA];
                float3 pB = _Points[indexB];
                float3 samlpePoint = lerp(_Points[indexA], _Points[indexB], blend);
                float3 sampleNormal = normalize(lerp(_Normals[indexA], _Normals[indexB], blend));
                float3 sampleTangent = normalize(lerp(_Tangents[indexA], _Tangents[indexB], blend));
                float3 sampleBiNormal = normalize(lerp(_BiNormals[indexA], _BiNormals[indexB], blend));
                float3x3 rotMat = float3x3(-sampleBiNormal, -sampleNormal, sampleTangent);
                float3 worldPos = samlpePoint + mul(rotMat, -rotated*v.randoms.y*3);
                
                o.clipPos = UnityWorldToClipPos(worldPos);
                o.randoms = v.randoms;
                o.color = float4(1,1,1, a);
                
//				float size = _Size * clamp(v.randoms.y, .2, 1) * (1+.2*o.proximity_size.x+.2*_Active);
//				size *= size;
//				o.proximity_size.y = size;
				
//                o.clipPos = UnityObjectToClipPos(rotated);
//                o.randoms = v.randoms;
//                o.normal = UnityObjectToWorldNormal(v.normal);
//                o.randBlend = float3(v.randBlend.xy, step(v.randBlend.z, _Blend));
//                o.clip = saturate((-UnityObjectToViewPos(v.vertex).z-_ProjectionParams.y)/_ClipFadeDistance);

                return o;
            }

            float3 TransformHSV(
                float3 c,  // color to transform
                float h,          // hue shift (in degrees)
                float s,          // saturation multiplier (scalar)
                float v           // value multiplier (scalar)
            ) {
                float vsu = v*s*cos(h*UNITY_PI/180);
                float vsw = v*s*sin(h*UNITY_PI/180);

                float3 ret;
                ret.r = (.299*v + .701*vsu + .168*vsw)*c.r
                    +   (.587*v - .587*vsu + .330*vsw)*c.g
                    +   (.114*v - .114*vsu - .497*vsw)*c.b;
                ret.g = (.299*v - .299*vsu - .328*vsw)*c.r
                    +   (.587*v + .413*vsu + .035*vsw)*c.g
                    +   (.114*v - .114*vsu + .292*vsw)*c.b;
                ret.b = (.299*v - .300*vsu + 1.25*vsw)*c.r
                    +   (.587*v - .588*vsu - 1.05*vsw)*c.g
                    +   (.114*v + .886*vsu - .203*vsw)*c.b;
                return ret;
            }


            [maxvertexcount(4)]
            void geom(point v2g i[1], inout TriangleStream<g2f> triStream)
            {
                g2f o;
                o.randoms = i[0].randoms;
                o.color = i[0].color;
//                o.proximity_size = i[0].proximity_size;
                
                float4 center = i[0].clipPos;
//                float4 stepper = step(float4(.5,.5,.5,.5), i[0].randoms);
                
				float4 up = float4(0, 1, 0, 0) * UNITY_MATRIX_P._22;
				float4 right = float4(1, 0, 0, 0) * UNITY_MATRIX_P._11;
                float size = o.randoms.z*_Size;
                
				float4 v[4];
				v[0] = center - size * up;
				v[1] = center + size * right;
				v[2] = center - size * right;
				v[3] = center + size * up;
				float2 uv[4];
				uv[0] = float2(-1,-1);
				uv[1] = float2(1,-1);
				uv[2] = float2(-1,1);
				uv[3] = float2(1,1);
				
				[unroll]
				for(uint idx = 0; idx < 4; idx++){
				    o.clipPos = v[idx];
				    o.uv = uv[idx];
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[0], o);
				    triStream.Append(o);
				}
            }

            float4 frag(g2f i) : COLOR
            {
                float l = length(abs(i.uv));
                float a = (1-l) * (1-step(1, l));
//                float a = saturate(1-l);
//                a *= a;
//                return fixed4(i.color.rgb, tex2D(_MainTex, i.uv).a);
                return fixed4(i.color.rgb, a*_Active*i.color.a);
//                return fixed4(i.uv, 0, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
