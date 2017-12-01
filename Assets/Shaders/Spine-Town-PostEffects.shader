// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Spine/Town Post Effects" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_MainTex ("Texture to blend", 2D) = "black" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15

		_EffectAmount ("Effect Amount", Range (0, 1)) = 1.0
		_BrightnessAmount ("Brightness Amount", Range(0.0, 1.5)) = 1.0
		_TransitionTime ("Transition Time", Range(0.0, 1)) = 0
		_DesaturateTransitionTime ("Desaturate Transition Time", Range(0.0, 1)) = 0

		_FirstColourGradeLUT ("First Color Grade", 3D) = "" {}
		_SecondColourGradeLUT ("Second Color Grade", 3D) = "" {}
		_DesatColourGradeLUT ("Desaturate Color Grade", 3D) = "" {}
	}
	// 2 texture stage GPUs
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
	
			bool _UseClipRect;
			float4 _ClipRect;

			bool _UseAlphaClip;			

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
				
				OUT.color = IN.color;//* _Color;
				return OUT;
			}

			uniform float _EffectAmount;
			uniform float _BrightnessAmount;
			uniform float _Cutoff;

			uniform sampler3D _FirstColourGradeLUT;
			uniform sampler3D _SecondColourGradeLUT;
			uniform sampler3D _DesatColourGradeLUT;

			uniform float _TransitionTime;
			uniform float _DesaturateTransitionTime;

			static const float lut_size = 16.0;
			static const float3 scale = ( ( lut_size - 1.0 ) / lut_size ).xxx;
			static const float3 offset = ( 1.0 / ( 2.0 * lut_size )).xxx;

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				if (_UseClipRect)
					color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				clip (color.a - _Cutoff);

				float3 brtColor = color.rgb * _BrightnessAmount;
				color.rgb = lerp(brtColor, dot(brtColor, float3(0.3, 0.59, 0.11)), _EffectAmount);

				float3 source_colour = scale * color.xyz + offset;
				float3 start_colour = tex3D( _FirstColourGradeLUT, source_colour ).rgb;
				float3 end_colour = tex3D( _SecondColourGradeLUT, source_colour ).rgb;
				float3 full_colour = lerp( start_colour, end_colour, _TransitionTime );

				float3 desat_colour = tex3D( _DesatColourGradeLUT, source_colour ).rgb;
				color.rgb = lerp( full_colour, desat_colour, _DesaturateTransitionTime );

				return color;
			}
		ENDCG
		}
	}
	FallBack "UI/Default"
}