// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BLINK/SPLASHv2"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_SctexturexyScdistortionzw("Sc texture xy Sc distortion zw", Vector) = (1,1,1,1)
		_SptexturexySpdistortionzw("Sp texture xy Sp distortion zw", Vector) = (-0.2,0,-0.1,0)
		_lerp("lerp", Range( 0 , 1)) = 0.1408898
		_Power_noise("Power_noise", Float) = 1
		_Color("Color", Color) = (1,1,1,0)
		[HDR]_Colorv2("Colorv2", Color) = (1,1,1,0)
		_Grad("Grad", 2D) = "white" {}
		_Power_color("Power_color", Float) = 1
		[Toggle]_textureorient("texture orient", Float) = 1
		_pivotnoise("pivot noise", Vector) = (0,0,0,0)
		_mask("mask", 2D) = "white" {}
		_test_mask("test_mask", Float) = 0
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.5
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_tex4coord;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float4 _Color;
		uniform sampler2D _Grad;
		uniform float _Power_noise;
		uniform sampler2D _MainTex;
		uniform float4 _SptexturexySpdistortionzw;
		uniform float _textureorient;
		uniform float4 _SctexturexyScdistortionzw;
		uniform float4 _pivotnoise;
		uniform float _lerp;
		uniform sampler2D _mask;
		uniform float _test_mask;
		uniform float _Power_color;
		uniform float4 _Colorv2;


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 appendResult63 = (float2(_SptexturexySpdistortionzw.x , _SptexturexySpdistortionzw.y));
			float2 speedtexture65 = appendResult63;
			float2 CenteredUV15_g19 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float sctexturex58 = _SctexturexyScdistortionzw.x;
			float2 break17_g19 = CenteredUV15_g19;
			float sctexturey57 = _SctexturexyScdistortionzw.y;
			float2 appendResult23_g19 = (float2(( length( CenteredUV15_g19 ) * sctexturex58 * 2.0 ) , ( atan2( break17_g19.x , break17_g19.y ) * ( 1.0 / 6.28318548202515 ) * sctexturey57 )));
			float2 temp_output_73_0 = appendResult23_g19;
			float2 break77 = temp_output_73_0;
			float2 appendResult80 = (float2(break77.y , break77.x));
			float2 ifLocalVar86 = 0;
			if( (( _textureorient )?( 1.0 ):( 0.0 )) == 0.0 )
				ifLocalVar86 = temp_output_73_0;
			else
				ifLocalVar86 = appendResult80;
			float2 panner100 = ( 1.0 * _Time.y * speedtexture65 + ifLocalVar86);
			float2 appendResult95 = (float2(_pivotnoise.x , _pivotnoise.y));
			float2 appendResult60 = (float2(_SptexturexySpdistortionzw.z , _SptexturexySpdistortionzw.w));
			float2 speedistort64 = appendResult60;
			float2 CenteredUV15_g21 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float scdistortionx61 = _SctexturexyScdistortionzw.z;
			float2 break17_g21 = CenteredUV15_g21;
			float scdistortiony62 = _SctexturexyScdistortionzw.w;
			float2 appendResult23_g21 = (float2(( length( CenteredUV15_g21 ) * scdistortionx61 * 2.0 ) , ( atan2( break17_g21.x , break17_g21.y ) * ( 1.0 / 6.28318548202515 ) * scdistortiony62 )));
			float2 panner91 = ( 1.0 * _Time.y * speedistort64 + appendResult23_g21);
			float simpleNoise94 = SimpleNoise( panner91*24.0 );
			float2 appendResult93 = (float2(_pivotnoise.z , _pivotnoise.w));
			float2 lerpResult108 = lerp( ( panner100 + appendResult95 ) , ( simpleNoise94 + appendResult93 ) , _lerp);
			float4 appendResult72 = (float4(i.uv_texcoord.x , ( (0.0 + (i.uv_texcoord.y - 0.0) * (-1.0 - 0.0) / (1.0 - 0.0)) + 1.0 ) , 0.0 , 0.0));
			float2 CenteredUV15_g20 = ( appendResult72.xy - float2( 0.5,0.5 ) );
			float2 break17_g20 = CenteredUV15_g20;
			float2 appendResult23_g20 = (float2(( length( CenteredUV15_g20 ) * sctexturex58 * 2.0 ) , ( atan2( break17_g20.x , break17_g20.y ) * ( 1.0 / 6.28318548202515 ) * sctexturey57 )));
			float2 temp_output_75_0 = appendResult23_g20;
			float2 break84 = temp_output_75_0;
			float2 appendResult90 = (float2(break84.y , break84.x));
			float2 ifLocalVar98 = 0;
			if( (( _textureorient )?( 1.0 ):( 0.0 )) == 0.0 )
				ifLocalVar98 = temp_output_75_0;
			else
				ifLocalVar98 = appendResult90;
			float2 panner101 = ( 1.0 * _Time.y * speedtexture65 + ifLocalVar98);
			float2 CenteredUV15_g22 = ( appendResult72.xy - float2( 0.5,0.5 ) );
			float2 break17_g22 = CenteredUV15_g22;
			float2 appendResult23_g22 = (float2(( length( CenteredUV15_g22 ) * scdistortionx61 * 2.0 ) , ( atan2( break17_g22.x , break17_g22.y ) * ( 1.0 / 6.28318548202515 ) * scdistortiony62 )));
			float2 panner97 = ( 1.0 * _Time.y * speedistort64 + appendResult23_g22);
			float simpleNoise106 = SimpleNoise( panner97*24.0 );
			float2 temp_cast_2 = (simpleNoise106).xx;
			float2 lerpResult110 = lerp( panner101 , temp_cast_2 , _lerp);
			float2 CenteredUV15_g23 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float2 break17_g23 = CenteredUV15_g23;
			float2 appendResult23_g23 = (float2(( length( CenteredUV15_g23 ) * 1.0 * 2.0 ) , ( atan2( break17_g23.x , break17_g23.y ) * ( 1.0 / 6.28318548202515 ) * 1.0 )));
			float2 break103 = appendResult23_g23;
			float2 appendResult109 = (float2(break103.y , break103.x));
			float lerpResult114 = lerp( tex2D( _MainTex, lerpResult108 ).r , tex2D( _MainTex, lerpResult110 ).r , tex2D( _mask, appendResult109 ).r);
			float Noise_test39 = lerpResult114;
			float2 CenteredUV15_g24 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			float2 break17_g24 = CenteredUV15_g24;
			float2 appendResult23_g24 = (float2(( length( CenteredUV15_g24 ) * 1.0 * 2.0 ) , ( atan2( break17_g24.x , break17_g24.y ) * ( 1.0 / 6.28318548202515 ) * 1.0 )));
			float smoothstepResult12 = smoothstep( 0.0 , ( i.uv_tex4coord.z + _Power_noise ) , ( Noise_test39 * ( ( 1.0 - appendResult23_g24.x ) * saturate( ( appendResult23_g24.x + _test_mask ) ) ) ));
			float2 temp_cast_3 = (pow( smoothstepResult12 , _Power_color )).xx;
			o.Emission = ( _Color * tex2D( _Grad, temp_cast_3 ) * _Colorv2 ).rgb;
			o.Alpha = saturate( ( smoothstepResult12 * i.vertexColor.a ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;399;1920;620;1875.755;329.7436;2.012803;True;False
Node;AmplifyShaderEditor.Vector4Node;56;-6441.713,-4219.767;Inherit;False;Property;_SctexturexyScdistortionzw;Sc texture xy Sc distortion zw;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.66,1.41,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;66;-8171.545,-3116.126;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;67;-7916.597,-2992.854;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;57;-6098.503,-4207.687;Inherit;False;sctexturey;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;58;-6102.503,-4287.687;Inherit;False;sctexturex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-7684.523,-2207.983;Inherit;False;58;sctexturex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;59;-6625.354,-3920.709;Inherit;False;Property;_SptexturexySpdistortionzw;Sp texture xy Sp distortion zw;2;0;Create;True;0;0;0;False;0;False;-0.2,0,-0.1,0;0,-0.35,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-7610.159,-2874.386;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-7677.03,-2129.244;Inherit;False;57;sctexturey;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;60;-6324.373,-3718.38;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-7201.457,-2971.527;Inherit;False;58;sctexturex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;73;-7476.865,-2132.278;Inherit;True;Polar Coordinates;-1;;19;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;-7380.654,-3015.659;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;-7189.357,-2891.969;Inherit;False;57;sctexturey;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;62;-6083.108,-4026.132;Inherit;False;scdistortiony;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-6097.108,-4106.131;Inherit;False;scdistortionx;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-6189.83,-3801.909;Inherit;False;speedistort;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;75;-6960.733,-3024.893;Inherit;True;Polar Coordinates;-1;;20;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-7831.479,-1400.163;Inherit;False;62;scdistortiony;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;63;-6347.048,-3909.065;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-7838.479,-1501.163;Inherit;False;61;scdistortionx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;77;-7062.616,-1988.965;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;81;-7596.679,-2476.789;Inherit;False;61;scdistortionx;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;82;-6949.375,-3382.013;Inherit;False;Property;_textureorient;texture orient;9;0;Create;True;0;0;0;False;0;False;1;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;84;-6636.498,-3107.843;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;83;-7322.572,-1225.875;Inherit;False;64;speedistort;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;85;-7603.214,-1465.925;Inherit;True;Polar Coordinates;-1;;21;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;80;-6916.561,-1973.572;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;65;-6188.988,-3904.444;Inherit;False;speedtexture;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-7595.704,-2390.653;Inherit;False;62;scdistortiony;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;86;-6830.434,-2205.22;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;87;-7358.314,-2517.484;Inherit;True;Polar Coordinates;-1;;22;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;88;-6802.291,-1796.757;Inherit;False;Property;_pivotnoise;pivot noise;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;4.21,1.19,-0.44,4.86;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;90;-6501.442,-3100.449;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-6733.979,-1990.46;Inherit;False;65;speedtexture;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;91;-7102.736,-1355.693;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-6872.004,-2320.472;Inherit;False;64;speedistort;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;94;-6813.666,-1479.851;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;24;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;95;-6576.329,-1771.401;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;96;-6443.732,-2902.72;Inherit;False;65;speedtexture;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;97;-6678.899,-2492.443;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;99;-6579.327,-1205.035;Inherit;True;Polar Coordinates;-1;;23;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;100;-6548.892,-2066.234;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;93;-6567.329,-1674.401;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ConditionalIfNode;98;-6445.315,-3324.097;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;104;-6354.561,-1994.755;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-6259.879,-2124.285;Inherit;False;Property;_lerp;lerp;3;0;Create;True;0;0;0;False;0;False;0.1408898;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;103;-6281.63,-1166.415;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.PannerNode;101;-6233.634,-3021.362;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;105;-6149.305,-1552.431;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;106;-6358.436,-2565.525;Inherit;True;Simple;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;24;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;108;-5900.141,-1705.662;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;107;-6101.407,-3497.024;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;None;67188aec381c46f44ae873b31d651847;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;52;-3395.636,265.8326;Inherit;True;Polar Coordinates;-1;;24;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;109;-6156.575,-1168.022;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;110;-6040.742,-2459.039;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;112;-5724.818,-2959.927;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;125;-2946.696,880.141;Inherit;False;Property;_test_mask;test_mask;14;0;Create;True;0;0;0;False;0;False;0;4.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;113;-5772.837,-2058.876;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;111;-6012.391,-1200.653;Inherit;True;Property;_mask;mask;13;0;Create;True;0;0;0;False;0;False;-1;None;fa5c011cb0cc3ca43b883fdd880f2fc3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;53;-3058.19,305.7032;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.LerpOp;114;-5043.431,-2323.334;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;-2753.033,780.4867;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;54;-2716.578,407.2643;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;123;-2452.716,807.6144;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-4348.001,-1994.076;Inherit;False;Noise_test;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;119;-2140.083,502.6687;Inherit;True;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-2012.486,330.1625;Inherit;False;Property;_Power_noise;Power_noise;4;0;Create;True;0;0;0;False;0;False;1;0.52;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-2460.757,515.7123;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2541.402,134.2867;Inherit;False;39;Noise_test;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2220.453,292.8253;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;120;-1821.083,519.6687;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;12;-1744.58,193.7274;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-1032.033,77.36584;Inherit;False;Property;_Power_color;Power_color;8;0;Create;True;0;0;0;False;0;False;1;0.74;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;115;-696.7867,365.3128;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;116;-768.8749,90.06489;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;126;-243.7704,-189.8091;Inherit;False;Property;_Colorv2;Colorv2;6;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;16.94838,16.94838,16.94838,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-490.967,32.6918;Inherit;True;Property;_Grad;Grad;7;0;Create;True;0;0;0;False;0;False;-1;None;8104cb3b15ee8c444b7270a7b8983d0a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-239.2584,-379.5928;Inherit;False;Property;_Color;Color;5;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5801886,0.9517458,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-464.5099,482.2625;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;23;-114.9869,451.4616;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;15;-1052.214,303.4863;Inherit;True;Property;_toon;toon;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosterizeNode;16;-1387.797,506.1287;Inherit;True;1;2;1;COLOR;0,0,0,0;False;0;INT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;144.7417,-65.55861;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1640.797,629.1287;Inherit;False;Property;_Toon_index;Toon_index;11;0;Create;True;0;0;0;False;0;False;0;55.43;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;127;411.8168,107.7053;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;BLINK/SPLASHv2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;67;0;66;2
WireConnection;57;0;56;2
WireConnection;58;0;56;1
WireConnection;69;0;67;0
WireConnection;60;0;59;3
WireConnection;60;1;59;4
WireConnection;73;3;70;0
WireConnection;73;4;68;0
WireConnection;72;0;66;1
WireConnection;72;1;69;0
WireConnection;62;0;56;4
WireConnection;61;0;56;3
WireConnection;64;0;60;0
WireConnection;75;1;72;0
WireConnection;75;3;74;0
WireConnection;75;4;71;0
WireConnection;63;0;59;1
WireConnection;63;1;59;2
WireConnection;77;0;73;0
WireConnection;84;0;75;0
WireConnection;85;3;76;0
WireConnection;85;4;78;0
WireConnection;80;0;77;1
WireConnection;80;1;77;0
WireConnection;65;0;63;0
WireConnection;86;0;82;0
WireConnection;86;2;80;0
WireConnection;86;3;73;0
WireConnection;86;4;80;0
WireConnection;87;1;72;0
WireConnection;87;3;81;0
WireConnection;87;4;79;0
WireConnection;90;0;84;1
WireConnection;90;1;84;0
WireConnection;91;0;85;0
WireConnection;91;2;83;0
WireConnection;94;0;91;0
WireConnection;95;0;88;1
WireConnection;95;1;88;2
WireConnection;97;0;87;0
WireConnection;97;2;89;0
WireConnection;100;0;86;0
WireConnection;100;2;92;0
WireConnection;93;0;88;3
WireConnection;93;1;88;4
WireConnection;98;0;82;0
WireConnection;98;2;90;0
WireConnection;98;3;75;0
WireConnection;98;4;90;0
WireConnection;104;0;100;0
WireConnection;104;1;95;0
WireConnection;103;0;99;0
WireConnection;101;0;98;0
WireConnection;101;2;96;0
WireConnection;105;0;94;0
WireConnection;105;1;93;0
WireConnection;106;0;97;0
WireConnection;108;0;104;0
WireConnection;108;1;105;0
WireConnection;108;2;102;0
WireConnection;109;0;103;1
WireConnection;109;1;103;0
WireConnection;110;0;101;0
WireConnection;110;1;106;0
WireConnection;110;2;102;0
WireConnection;112;0;107;0
WireConnection;112;1;110;0
WireConnection;113;0;107;0
WireConnection;113;1;108;0
WireConnection;111;1;109;0
WireConnection;53;0;52;0
WireConnection;114;0;113;1
WireConnection;114;1;112;1
WireConnection;114;2;111;1
WireConnection;121;0;53;0
WireConnection;121;1;125;0
WireConnection;54;0;53;0
WireConnection;123;0;121;0
WireConnection;39;0;114;0
WireConnection;124;0;54;0
WireConnection;124;1;123;0
WireConnection;55;0;40;0
WireConnection;55;1;124;0
WireConnection;120;0;119;3
WireConnection;120;1;8;0
WireConnection;12;0;55;0
WireConnection;12;2;120;0
WireConnection;116;0;12;0
WireConnection;116;1;117;0
WireConnection;14;1;116;0
WireConnection;20;0;12;0
WireConnection;20;1;115;4
WireConnection;23;0;20;0
WireConnection;15;1;12;0
WireConnection;15;0;16;0
WireConnection;16;1;12;0
WireConnection;16;0;17;0
WireConnection;10;0;9;0
WireConnection;10;1;14;0
WireConnection;10;2;126;0
WireConnection;127;2;10;0
WireConnection;127;9;23;0
ASEEND*/
//CHKSM=15B78055B9B62EC8E739A562396F99E9CEB8FA04