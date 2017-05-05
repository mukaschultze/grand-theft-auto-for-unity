// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "GTA/Standard" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
		 _Layer ("Layer", int) = 0

		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {
            }
		[NoScaleOffset] _MaskTex ("Mask (A)", 2D) = "white" {
            }

		_Ambient ("Ambient", Range(0,1)) = 1.0
		_Diffuse ("Diffuse", Range(0,1)) = 1.0
		_Specular ("Specular", Range(0,1)) = 0.0
		_Cutout ("Cutout", Range(0,1)) = 0.01
		
		[ToggleOff] _ZWrite ("ZWrite", int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blend Source", int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blend Destination", int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", int) = 0
	}
	SubShader {
        Tags {
            "RenderType"="Opaque" }
		LOD 200
        Cull [_Cull]
		Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        ZTest LEqual

		CGPROGRAM
		//#pragma multi_compile _ _ALPHA_FADE
		//#pragma multi_compile _ _LAYER_DEBUG
		//#pragma multi_compile _ _SPECULAR_ON
		//#pragma multi_compile _ _NIGHT_ILLUMINATION
		#pragma surface surf Standard fullforwardshadows alphatest:_Cutout
		#pragma target 3.0

		struct Input {
            float3 worldPos;
            float2 uv_MainTex;
            float4 MainTex_TexelSize;
            float4 color : COLOR;
	        float4 texcoord1 : TEXCOORD1;
        };

        sampler2D _MainTex;
        sampler2D _MaskTex;
        int _Layer;
        half _Ambient;
        half _Diffuse;
        half _Specular;
        fixed4 _Color;
        #ifdef _LAYER_DEBUG
	    fixed4 _DebugColors[32];
        #endif

        half4 blur (Input IN) {
            half4 sum = half4(0,0,0,0);
            float dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld,float4(IN.worldPos.x,IN.worldPos.y,IN.worldPos.z,0)));
            float depth = saturate((dist - 70) / 10000);
            float2 uv = IN.uv_MainTex;

            sum += tex2D(_MainTex, float2(uv.x - 5.0 * depth, uv.y - 5.0 * depth)) * 0.025;
            sum += tex2D(_MainTex, float2(uv.x - 4.0 * depth, uv.y - 4.0 * depth)) * 0.05;
            sum += tex2D(_MainTex, float2(uv.x - 3.0 * depth, uv.y - 3.0 * depth)) * 0.09;
            sum += tex2D(_MainTex, float2(uv.x - 2.0 * depth, uv.y - 2.0 * depth)) * 0.12;
            sum += tex2D(_MainTex, float2(uv.x - 1.0 * depth, uv.y - 1.0 * depth)) * 0.15;
            sum += tex2D(_MainTex, float2(uv.x, uv.y)) * 0.16;
            sum += tex2D(_MainTex, float2(uv.x + 1.0 * depth, uv.y + 1.0 * depth)) * 0.15;
            sum += tex2D(_MainTex, float2(uv.x + 2.0 * depth, uv.y + 2.0 * depth)) * 0.12;
            sum += tex2D(_MainTex, float2(uv.x + 3.0 * depth, uv.y + 3.0 * depth)) * 0.09;
            sum += tex2D(_MainTex, float2(uv.x + 4.0 * depth, uv.y + 4.0 * depth)) * 0.05;
            sum += tex2D(_MainTex, float2(uv.x + 5.0 * depth, uv.y + 5.0 * depth)) * 0.025;

            return sum;
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;
            o.Alpha = tex2D(_MaskTex, IN.uv_MainTex).a;
            o.Occlusion = _Ambient;
            o.Smoothness = 1 - _Diffuse;
            #ifdef _SPECULAR_ON
			o.Metallic = _Specular;
            #endif
            #ifdef _LAYER_DEBUG
            o.Albedo = length(o.Albedo) * 0.2 + _DebugColors[_Layer].rgb * 0.8;
            #endif
            #ifdef _NIGHT_ILLUMINATION
            o.Emission = IN.color * o.Albedo;
            #endif
		}

        float mip_map_level(in float2 texture_coordinate) {
            float2 dx_vtc = ddx(texture_coordinate);
            float2 dy_vtc = ddy(texture_coordinate);
            float delta_max_sqr = max(dot(dx_vtc, dx_vtc), dot(dy_vtc, dy_vtc));
            return 0.5 * log2(delta_max_sqr);
        }
		ENDCG
	}
    FallBack "GTA/Diffuse"
}
