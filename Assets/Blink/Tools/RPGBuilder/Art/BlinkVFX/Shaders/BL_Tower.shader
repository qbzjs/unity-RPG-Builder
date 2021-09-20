// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BLINK/Tower"
{
	Properties
	{
		_noise("noise", 2D) = "white" {}
		_Lerp_distort("Lerp_distort", Range( 0 , 1)) = 0.2505414
		_time("time", Float) = 1
		_Color_ramp("Color_ramp", 2D) = "white" {}
		_Scale_distort("Scale_distort", Float) = 7.72
		_PowerColor("PowerColor", Float) = 0.54
		_Power_Alpha("Power_Alpha", Float) = 0.69
		_Color("Color", Color) = (0,0,0,0)
		[HDR]_Colorv2("Colorv2", Color) = (0,0,0,0)
		_Float0("Float 0", Float) = 0
		_speed("speed", Vector) = (0,1,0,0.5)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float4 _Color;
		uniform sampler2D _Color_ramp;
		uniform sampler2D _noise;
		uniform float _time;
		uniform float4 _speed;
		uniform float4 _noise_ST;
		uniform float _Scale_distort;
		uniform float _Lerp_distort;
		uniform float _PowerColor;
		uniform float4 _Colorv2;
		uniform float _Power_Alpha;
		uniform float _Float0;


		float2 UnityGradientNoiseDir( float2 p )
		{
			p = fmod(p , 289);
			float x = fmod((34 * p.x + 1) * p.x , 289) + p.y;
			x = fmod( (34 * x + 1) * x , 289);
			x = frac( x / 41 ) * 2 - 1;
			return normalize( float2(x - floor(x + 0.5 ), abs( x ) - 0.5 ) );
		}
		
		float UnityGradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 ip = floor( p );
			float2 fp = frac( p );
			float d00 = dot( UnityGradientNoiseDir( ip ), fp );
			float d01 = dot( UnityGradientNoiseDir( ip + float2( 0, 1 ) ), fp - float2( 0, 1 ) );
			float d10 = dot( UnityGradientNoiseDir( ip + float2( 1, 0 ) ), fp - float2( 1, 0 ) );
			float d11 = dot( UnityGradientNoiseDir( ip + float2( 1, 1 ) ), fp - float2( 1, 1 ) );
			fp = fp * fp * fp * ( fp * ( fp * 6 - 15 ) + 10 );
			return lerp( lerp( d00, d01, fp.y ), lerp( d10, d11, fp.y ), fp.x ) + 0.5;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float mulTime17 = _Time.y * _time;
			float2 appendResult46 = (float2(_speed.z , _speed.w));
			float2 uv_noise = i.uv_texcoord * _noise_ST.xy + _noise_ST.zw;
			float2 panner12 = ( mulTime17 * appendResult46 + uv_noise);
			float2 panner16 = ( mulTime17 * _speed.xy + i.uv_texcoord);
			float gradientNoise14 = UnityGradientNoise(panner16,_Scale_distort);
			gradientNoise14 = gradientNoise14*0.5 + 0.5;
			float2 temp_cast_1 = (gradientNoise14).xx;
			float2 lerpResult19 = lerp( panner12 , temp_cast_1 , _Lerp_distort);
			float temp_output_29_0 = ( tex2D( _noise, lerpResult19 ).r * (1.0 + (abs( ( i.uv_texcoord.x + -0.5 ) ) - 0.0) * (0.0 - 1.0) / (0.5 - 0.0)) );
			float2 temp_cast_2 = (pow( saturate( temp_output_29_0 ) , _PowerColor )).xx;
			o.Emission = ( _Color * tex2D( _Color_ramp, temp_cast_2 ) * _Colorv2 ).rgb;
			float smoothstepResult25 = smoothstep( 0.0 , _Power_Alpha , temp_output_29_0);
			float2 uv_TexCoord39 = i.uv_texcoord + float2( 0,0.4 );
			float smoothstepResult38 = smoothstep( 0.36 , 0.59 , uv_TexCoord39.y);
			float smoothstepResult32 = smoothstep( 0.0 , _Float0 , ( saturate( ( 1.0 - i.uv_texcoord.y ) ) * smoothstepResult38 ));
			o.Alpha = ( smoothstepResult25 * ( smoothstepResult32 * i.vertexColor.a ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;387;1920;632;2062.857;717.0798;2.816587;True;False
Node;AmplifyShaderEditor.RangedFloatNode;18;-1258.851,-576.7759;Inherit;False;Property;_time;time;2;0;Create;True;0;0;0;False;0;False;1;0.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;17;-1049.851,-561.7759;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-1094.851,-772.7759;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;44;-1266.756,-415.8133;Inherit;False;Property;_speed;speed;10;0;Create;True;0;0;0;False;0;False;0,1,0,0.5;-0.5,0,0,0.5;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;16;-807.8516,-703.7759;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1075.756,-309.8133;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1090,-156;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-919.4803,-329.508;Inherit;False;0;9;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-722.6979,-479.2953;Inherit;False;Property;_Scale_distort;Scale_distort;4;0;Create;True;0;0;0;False;0;False;7.72;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;14;-505.4803,-569.508;Inherit;True;Gradient;True;True;2;0;FLOAT2;0,0;False;1;FLOAT;7.72;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-509.4803,-351.508;Inherit;False;Property;_Lerp_distort;Lerp_distort;1;0;Create;True;0;0;0;False;0;False;0.2505414;0.2505414;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;6;-819,-91;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;12;-556.4039,-263.508;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;7;-607,-59;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;-155.4803,-536.508;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-527.9706,716.4155;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;35;-191.8827,850.5888;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-615.8546,1088.273;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0.4;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;8;-332,12;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-113.4803,-278.508;Inherit;True;Property;_noise;noise;0;0;Create;True;0;0;0;False;0;False;-1;None;2333f54d6fd1cb844b505a320f0dcfcb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-0.8542862,223.2983;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;38;-178.6134,1154.871;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.36;False;2;FLOAT;0.59;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;40;29.77869,948.4692;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;48;216.5946,89.56735;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;367.5801,803.9155;Inherit;False;Property;_Float0;Float 0;9;0;Create;True;0;0;0;False;0;False;0;1.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;242.9507,-101.1144;Inherit;False;Property;_PowerColor;PowerColor;5;0;Create;True;0;0;0;False;0;False;0.54;0.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;230.6677,1045.352;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;43;475.5964,912.1765;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;398.6139,307.4435;Inherit;False;Property;_Power_Alpha;Power_Alpha;6;0;Create;True;0;0;0;False;0;False;0.69;3.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;23;431.9507,-84.11438;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;32;530.5614,680.891;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;752.5964,879.1765;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;25;570.6139,189.4435;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;27;648.8955,-482.0891;Inherit;False;Property;_Color;Color;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5801886,0.9517458,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;47;644.0434,-656.0336;Inherit;False;Property;_Colorv2;Colorv2;8;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;39.39663,39.39663,39.39663,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;565.9507,-312.1144;Inherit;True;Property;_Color_ramp;Color_ramp;3;0;Create;True;0;0;0;False;0;False;-1;None;8104cb3b15ee8c444b7270a7b8983d0a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;780.0387,415.016;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;922.8955,-285.0891;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;45;-1069.756,-452.8133;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;49;1109,-62;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;BLINK/Tower;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;18;0
WireConnection;16;0;15;0
WireConnection;16;2;44;0
WireConnection;16;1;17;0
WireConnection;46;0;44;3
WireConnection;46;1;44;4
WireConnection;14;0;16;0
WireConnection;14;1;21;0
WireConnection;6;0;5;1
WireConnection;12;0;13;0
WireConnection;12;2;46;0
WireConnection;12;1;17;0
WireConnection;7;0;6;0
WireConnection;19;0;12;0
WireConnection;19;1;14;0
WireConnection;19;2;20;0
WireConnection;35;0;34;2
WireConnection;8;0;7;0
WireConnection;9;1;19;0
WireConnection;29;0;9;1
WireConnection;29;1;8;0
WireConnection;38;0;39;2
WireConnection;40;0;35;0
WireConnection;48;0;29;0
WireConnection;41;0;40;0
WireConnection;41;1;38;0
WireConnection;23;0;48;0
WireConnection;23;1;24;0
WireConnection;32;0;41;0
WireConnection;32;2;33;0
WireConnection;42;0;32;0
WireConnection;42;1;43;4
WireConnection;25;0;29;0
WireConnection;25;2;26;0
WireConnection;22;1;23;0
WireConnection;30;0;25;0
WireConnection;30;1;42;0
WireConnection;28;0;27;0
WireConnection;28;1;22;0
WireConnection;28;2;47;0
WireConnection;45;0;44;1
WireConnection;45;1;44;2
WireConnection;49;2;28;0
WireConnection;49;9;30;0
ASEEND*/
//CHKSM=582310E0F9AC95EBFCE88609F7D0E54BD35AB679