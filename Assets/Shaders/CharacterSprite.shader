Shader "Spine/DungeonSprite" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_MainTex ("Texture to blend", 2D) = "black" {}

		_EffectAmount ("Effect Amount", Range (0, 1)) = 1.0
		_BrightnessAmount ("Brightness Amount", Range(0.0, 1)) = 1.0
		_TransitionTime ("Transition Time", Range(0.0, 1)) = 0
		_DesaturateTransitionTime ("Desaturate Transition Time", Range(0.0, 1)) = 0

		_FirstColourGradeLUT ("First Color Grade", 3D) = "" {}
		_SecondColourGradeLUT ("Second Color Grade", 3D) = "" {}
		_DesatColourGradeLUT ("Desaturate Color Grade", 3D) = "" {}
	}
	// 2 texture stage GPUs
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass {
			ColorMaterial AmbientAndDiffuse
			SetTexture [_MainTex] {
				Combine texture * primary
			}
		}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			
			Fog { Mode Off }
			ZWrite On
			ZTest LEqual
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			struct v2f { 
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
			};

			uniform float4 _MainTex_ST;

			v2f vert (appdata_base v) {
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _Cutoff;

			uniform float _EffectAmount;
			uniform float _BrightnessAmount;

			uniform sampler3D _FirstColourGradeLUT;
			uniform sampler3D _SecondColourGradeLUT;
			uniform sampler3D _DesatColourGradeLUT;

			uniform float _TransitionTime;
			uniform float _DesaturateTransitionTime;

			static const float lut_size = 16.0;
			static const float3 scale = ( ( lut_size - 1.0 ) / lut_size ).xxx;
			static const float3 offset = ( 1.0 / ( 2.0 * lut_size )).xxx;

			float4 frag (v2f i) : COLOR {
				fixed4 texcol = tex2D(_MainTex, i.uv);
				clip(texcol.a - _Cutoff);

				float3 brtColor = texcol.rgb * _BrightnessAmount;
				texcol.rgb = lerp(brtColor, dot(brtColor, float3(0.3, 0.59, 0.11)), _EffectAmount);

				float3 source_colour = scale * texcol.xyz + offset;
				float3 start_colour = tex3D( _FirstColourGradeLUT, source_colour ).rgb;
				float3 end_colour = tex3D( _SecondColourGradeLUT, source_colour ).rgb;
				float3 full_colour = lerp( start_colour, end_colour, _TransitionTime );

				float3 desat_colour = tex3D( _DesatColourGradeLUT, source_colour ).rgb;
				texcol.rgb = lerp( full_colour, desat_colour, _DesaturateTransitionTime );

				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	// 1 texture stage GPUs
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass {
			ColorMaterial AmbientAndDiffuse
			SetTexture [_MainTex] {
				Combine texture * primary DOUBLE, texture * primary
			}
		}
	}
}