// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polytope Studio/PT_Fruit_Trees_Shader"
{
	Properties
	{
		[NoScaleOffset]_BASETEXTURE("BASE TEXTURE", 2D) = "black" {}
		[Toggle]_CUSTOMCOLORSTINTING("CUSTOM COLORS  TINTING", Float) = 1
		[HDR]_TOPCOLOR("TOP COLOR", Color) = (0.3505436,0.5754717,0.3338822,1)
		[HDR]_MIDDLECOLOR("MIDDLE COLOR", Color) = (0.1891242,0.4716981,0.2807905,1)
		[HDR]_GROUNDCOLOR("GROUND COLOR", Color) = (0.1879673,0.3113208,0.1776878,1)
		[HDR]_COLORGRADIENTRATIO("COLOR GRADIENT RATIO", Range( 0 , 1)) = 1
		_LEAVESTHICKNESS("LEAVES THICKNESS", Range( 0.1 , 0.95)) = 0.5
		[Toggle]_CUSTOMFLOWERSCOLOR("CUSTOM FLOWERS COLOR", Float) = 0
		[HideInInspector]_MaskClipValue("Mask Clip Value", Range( 0 , 1)) = 1
		[HDR]_GrapesColors("Grapes Colors", Color) = (0.5566038,0.3176842,0.3176842,0)
		[Toggle(_CUSTOMWIND_ON)] _CUSTOMWIND("CUSTOM WIND", Float) = 1
		_WINDMOVEMENT("WIND MOVEMENT", Range( 0 , 1)) = 0.5
		_WINDDENSITY("WIND DENSITY", Range( 0 , 5)) = 0.2
		_WINDSTRENGHT("WIND STRENGHT", Range( 0 , 1)) = 0.3
		_VertexAOIntensity("Vertex AO Intensity", Range( 0 , 1)) = 1
		_Float6("Float 6", Range( 0 , 1)) = 1
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _CUSTOMWIND_ON
		#pragma surface surf StandardSpecularCustom keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float4 vertexColor : COLOR;
		};

		struct SurfaceOutputStandardSpecularCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Specular;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		uniform float _WINDMOVEMENT;
		uniform float _WINDDENSITY;
		uniform float _WINDSTRENGHT;
		uniform float _CUSTOMCOLORSTINTING;
		uniform float _CUSTOMFLOWERSCOLOR;
		uniform sampler2D _BASETEXTURE;
		uniform float4 _GrapesColors;
		uniform float4 _GROUNDCOLOR;
		uniform float4 _MIDDLECOLOR;
		uniform float _COLORGRADIENTRATIO;
		uniform float4 _TOPCOLOR;
		uniform float _VertexAOIntensity;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _Float6;
		uniform float _LEAVESTHICKNESS;
		uniform float _MaskClipValue;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 ase_vertex4Pos = v.vertex;
			float simplePerlin2D321 = snoise( (ase_vertex4Pos*1.0 + ( _Time.y * _WINDMOVEMENT )).xy*_WINDDENSITY );
			simplePerlin2D321 = simplePerlin2D321*0.5 + 0.5;
			float4 appendResult329 = (float4(( ( ( simplePerlin2D321 - 0.5 ) * _WINDSTRENGHT ) + ase_vertex4Pos.x ) , ase_vertex4Pos.y , ase_vertex4Pos.z , 1.0));
			float4 lerpResult330 = lerp( ase_vertex4Pos , appendResult329 , ( ase_vertex4Pos.y * 2.0 ));
			float4 transform331 = mul(unity_WorldToObject,float4( _WorldSpaceCameraPos , 0.0 ));
			float4 temp_cast_2 = (transform331.w).xxxx;
			float2 temp_cast_3 = (0.5).xx;
			float2 uv_TexCoord204 = v.texcoord.xy + temp_cast_3;
			float temp_output_207_0 = step( uv_TexCoord204.x , 1.0 );
			float clampResult281 = clamp( temp_output_207_0 , 0.0 , 1.0 );
			float4 lerpResult385 = lerp( ( lerpResult330 - temp_cast_2 ) , ase_vertex4Pos , ( 1.0 - clampResult281 ));
			#ifdef _CUSTOMWIND_ON
				float4 staticSwitch333 = lerpResult385;
			#else
				float4 staticSwitch333 = ase_vertex4Pos;
			#endif
			v.vertex.xyz = staticSwitch333.xyz;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardSpecularCustom(SurfaceOutputStandardSpecularCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandardSpecular r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Specular = s.Specular;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandardSpecular (r, viewDir, gi) + c;
		}

		inline void LightingStandardSpecularCustom_GI(SurfaceOutputStandardSpecularCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardSpecularCustom o )
		{
			float2 uv_BASETEXTURE2 = i.uv_texcoord;
			float4 tex2DNode2 = tex2D( _BASETEXTURE, uv_BASETEXTURE2 );
			float grayscale313 = dot(tex2DNode2.rgb, float3(0.299,0.587,0.114));
			float2 temp_cast_1 = (0.5).xx;
			float2 uv_TexCoord204 = i.uv_texcoord + temp_cast_1;
			float temp_output_207_0 = step( uv_TexCoord204.x , 1.0 );
			float2 temp_cast_2 = (0.0).xx;
			float2 uv_TexCoord235 = i.uv_texcoord + temp_cast_2;
			float temp_output_236_0 = step( uv_TexCoord235.y , 0.5 );
			float temp_output_238_0 = ( temp_output_207_0 + temp_output_236_0 );
			float clampResult248 = clamp( ( 1.0 - temp_output_238_0 ) , 0.0 , 1.0 );
			float4 temp_cast_3 = (( grayscale313 * clampResult248 )).xxxx;
			float4 blendOpSrc310 = _GrapesColors;
			float4 blendOpDest310 = temp_cast_3;
			float4 lerpBlendMode310 = lerp(blendOpDest310,( blendOpSrc310 * blendOpDest310 ),clampResult248);
			float4 lerpResult341 = lerp( tex2DNode2 , ( saturate( lerpBlendMode310 )) , clampResult248);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_345_0 = ( ase_vertex3Pos.y * _COLORGRADIENTRATIO );
			float temp_output_346_0 = (0.0 + (temp_output_345_0 - 0.3) * (1.0 - 0.0) / (0.5 - 0.3));
			float clampResult351 = clamp( ( ( 1.0 - temp_output_346_0 ) * (0.0 + (temp_output_345_0 - 0.0) * (1.0 - 0.0) / (0.4 - 0.0)) ) , 0.0 , 1.0 );
			float4 lerpResult353 = lerp( _GROUNDCOLOR , _MIDDLECOLOR , clampResult351);
			float clampResult354 = clamp( temp_output_346_0 , 0.0 , 1.0 );
			float4 lerpResult356 = lerp( lerpResult353 , _TOPCOLOR , clampResult354);
			float4 temp_cast_4 = (temp_output_238_0).xxxx;
			float4 lerpResult205 = lerp( lerpResult356 , temp_cast_4 , clampResult248);
			float4 temp_cast_5 = (grayscale313).xxxx;
			float clampResult281 = clamp( temp_output_207_0 , 0.0 , 1.0 );
			float4 lerpResult362 = lerp( temp_cast_5 , (( _CUSTOMFLOWERSCOLOR )?( lerpResult341 ):( tex2DNode2 )) , ( 1.0 - clampResult281 ));
			float4 blendOpSrc232 = lerpResult205;
			float4 blendOpDest232 = lerpResult362;
			float4 lerpBlendMode232 = lerp(blendOpDest232,(( blendOpDest232 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest232 ) * ( 1.0 - blendOpSrc232 ) ) : ( 2.0 * blendOpDest232 * blendOpSrc232 ) ),clampResult281);
			float4 temp_output_232_0 = lerpBlendMode232;
			o.Albedo = (( _CUSTOMCOLORSTINTING )?( temp_output_232_0 ):( (( _CUSTOMFLOWERSCOLOR )?( lerpResult341 ):( tex2DNode2 )) )).rgb;
			float clampResult378 = clamp( ( i.vertexColor.r * _VertexAOIntensity ) , 0.0 , 0.5 );
			float4 temp_cast_7 = (clampResult378).xxxx;
			float4 transform368 = mul(unity_ObjectToWorld,temp_cast_7);
			float grayscale369 = Luminance(transform368.xyz);
			float clampResult379 = clamp( grayscale369 , 0.0 , 1.0 );
			o.Occlusion = clampResult379;
			o.Translucency = ( temp_output_232_0 * _Float6 * ( temp_output_207_0 * ( 1.0 - temp_output_236_0 ) ) ).rgb;
			o.Alpha = 1;
			clip( ( 1.0 - step( tex2DNode2.a , ( 1.0 - _LEAVESTHICKNESS ) ) ) - _MaskClipValue );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;33;1697;996;3581.9;1987.007;1.238989;True;True
Node;AmplifyShaderEditor.CommentaryNode;266;-2906.208,-606.8044;Inherit;False;1584.073;616.926;mask;15;281;248;209;391;390;236;238;208;392;234;207;204;235;206;233;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;206;-2527.499,-548.541;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;233;-2544.515,-206.3119;Inherit;False;Constant;_Float2;Float 2;11;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;314;-4261.057,888.3673;Inherit;False;3755.488;634.5623;WIND;20;332;331;330;329;328;327;326;325;324;323;322;321;320;319;318;317;316;315;333;385;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;204;-2592.52,-561.3225;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;234;-2583.672,-93.77672;Inherit;False;Constant;_Float3;Float 3;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;342;-3085.827,-1986.271;Inherit;False;1766.19;1335;GRADIENT;14;356;355;354;353;352;351;350;349;348;347;346;345;344;343;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;208;-2624.249,-449.4805;Float;False;Constant;_Float1;Float 1;11;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;235;-2589.443,-272.0148;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;315;-4197.537,1357.597;Inherit;False;Property;_WINDMOVEMENT;WIND MOVEMENT;11;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;316;-4188.459,1274.563;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;207;-2298.277,-564.6639;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;343;-3027.305,-1197.69;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;344;-3071.892,-1019.562;Float;False;Property;_COLORGRADIENTRATIO;COLOR GRADIENT RATIO;5;1;[HDR];Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;39;-3105.782,85.33564;Inherit;False;2608.031;400.1461;COLOR;8;335;341;310;308;336;313;127;2;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;317;-3971.471,1220.502;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;236;-2296.121,-357.1232;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;318;-4130.978,948.3261;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;320;-3846.511,1353.231;Inherit;False;Property;_WINDDENSITY;WIND DENSITY;12;0;Create;True;0;0;0;False;0;False;0.2;1.91;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;319;-3814.034,1112.694;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;345;-2779.043,-1143.358;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;127;-3017.699,191.4318;Inherit;True;Property;_BASETEXTURE;BASE TEXTURE;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;e8be4d2172dae3b48bdb136744d8646e;0861a2144c1c12a4992d767cb6f463b4;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;238;-2104.818,-565.272;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;321;-3530.195,1105.588;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;346;-2610.92,-1005.266;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.3;False;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;209;-1907.291,-571.1132;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-2566.659,210.1853;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;248;-1751.968,-352.5235;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;313;-2131.611,133.8381;Inherit;True;1;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;347;-2398.791,-1010.27;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;348;-2604.184,-1264.042;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;322;-3140.813,1112.18;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;323;-3209.617,1336.884;Inherit;False;Property;_WINDSTRENGHT;WIND STRENGHT;13;0;Create;True;0;0;0;False;0;False;0.3;0.203;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;363;-1824.097,1788.62;Inherit;False;1754.971;371.4906;  ;7;369;368;365;364;377;378;379;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;324;-2914.047,1113.466;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;308;-1721.31,272.6496;Inherit;False;Property;_GrapesColors;Grapes Colors;9;1;[HDR];Create;True;0;0;0;False;0;False;0.5566038,0.3176842,0.3176842,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;349;-2276.75,-1299.898;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;336;-1660.359,121.9478;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;364;-1811.878,1842.537;Inherit;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;325;-2220.044,1312.439;Inherit;False;Constant;_Float4;Float 4;7;0;Create;True;0;0;0;False;0;False;2;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;350;-3005.328,-1702.342;Float;False;Property;_MIDDLECOLOR;MIDDLE COLOR;3;1;[HDR];Create;True;0;0;0;False;0;False;0.1891242,0.4716981,0.2807905,1;0,0.5943396,0.0169811,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;352;-2995.342,-1927.746;Float;False;Property;_GROUNDCOLOR;GROUND COLOR;4;1;[HDR];Create;True;0;0;0;False;0;False;0.1879673,0.3113208,0.1776878,1;0.05298166,0.3490566,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;326;-2588.611,968.8419;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;351;-2213.687,-1613.623;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;310;-1468.777,182.6426;Inherit;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;365;-1777.264,2044.109;Inherit;False;Property;_VertexAOIntensity;Vertex AO Intensity;14;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;341;-1182.208,190.2492;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;329;-2299.171,988.7838;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;355;-3003.211,-1438.972;Float;False;Property;_TOPCOLOR;TOP COLOR;2;1;[HDR];Create;True;0;0;0;False;0;False;0.3505436,0.5754717,0.3338822,1;0.01743852,0.5754717,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;337;-1356.467,503.804;Inherit;False;852.152;316.5043;LEAVES CUTOFF;4;289;290;287;288;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;281;-1739.507,-580.2343;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;327;-1939.741,1317.856;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;328;-2060.268,1009.169;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;353;-2104.375,-1908.263;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;354;-2094.266,-1006.486;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;377;-1436.481,1840.474;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;331;-1608.296,1320.026;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;330;-1779.816,982.6949;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ClampOpNode;378;-1197.763,1839.876;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;392;-1530.307,-577.2631;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;335;-993.7451,194.1185;Inherit;False;Property;_CUSTOMFLOWERSCOLOR;CUSTOM FLOWERS COLOR;7;0;Create;True;0;0;0;False;0;False;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;356;-1684.263,-1469.351;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;287;-1306.468,705.3088;Inherit;False;Property;_LEAVESTHICKNESS;LEAVES THICKNESS;6;0;Create;True;0;0;0;False;0;False;0.5;0.95;0.1;0.95;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;390;-2102.996,-356.147;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;368;-895.1022,1865.024;Inherit;True;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;205;-614.5958,-547.8208;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;386;-1488.273,814.1622;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;288;-1184.1,596.2936;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;362;-585.8334,109.2359;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;332;-1437.335,988.5349;Inherit;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;385;-1138.498,979.0674;Inherit;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StepOpNode;289;-1014.878,555.267;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;369;-632.3733,1856.658;Inherit;True;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;372;65.40105,282.2135;Inherit;False;Property;_Float6;Float 6;15;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;232;-266.3238,32.14053;Inherit;True;Overlay;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;391;-1944.086,-361.5232;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;333;-793.7081,978.0486;Inherit;False;Property;_CUSTOMWIND;CUSTOM WIND;10;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;371;418.2303,42.45147;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;290;-702.3157,553.804;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;394;-2066.058,347.1769;Inherit;False;Constant;_Color0;Color 0;17;1;[HDR];Create;True;0;0;0;False;0;False;0.5566038,0.5566038,0.5566038,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;393;-1868.283,63.00656;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;357;146.5512,166.6256;Inherit;False;Property;_CUSTOMCOLORSTINTING;CUSTOM COLORS  TINTING;1;0;Create;True;0;0;0;False;0;False;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;130;981.2258,-196.5941;Inherit;False;Property;_MaskClipValue;Mask Clip Value;8;1;[HideInInspector];Fetch;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;379;-371.9919,1879.043;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;62;718.5422,192.8389;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Polytope Studio/PT_Fruit_Trees_Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;-1;16;-1;-1;0;False;0;0;False;-1;-1;0;True;130;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;204;1;206;0
WireConnection;235;1;233;0
WireConnection;207;0;204;1
WireConnection;207;1;208;0
WireConnection;317;0;316;0
WireConnection;317;1;315;0
WireConnection;236;0;235;2
WireConnection;236;1;234;0
WireConnection;319;0;318;0
WireConnection;319;2;317;0
WireConnection;345;0;343;2
WireConnection;345;1;344;0
WireConnection;238;0;207;0
WireConnection;238;1;236;0
WireConnection;321;0;319;0
WireConnection;321;1;320;0
WireConnection;346;0;345;0
WireConnection;209;0;238;0
WireConnection;2;0;127;0
WireConnection;248;0;209;0
WireConnection;313;0;2;0
WireConnection;347;0;346;0
WireConnection;348;0;345;0
WireConnection;322;0;321;0
WireConnection;324;0;322;0
WireConnection;324;1;323;0
WireConnection;349;0;347;0
WireConnection;349;1;348;0
WireConnection;336;0;313;0
WireConnection;336;1;248;0
WireConnection;326;0;324;0
WireConnection;326;1;318;1
WireConnection;351;0;349;0
WireConnection;310;0;308;0
WireConnection;310;1;336;0
WireConnection;310;2;248;0
WireConnection;341;0;2;0
WireConnection;341;1;310;0
WireConnection;341;2;248;0
WireConnection;329;0;326;0
WireConnection;329;1;318;2
WireConnection;329;2;318;3
WireConnection;281;0;207;0
WireConnection;328;0;318;2
WireConnection;328;1;325;0
WireConnection;353;0;352;0
WireConnection;353;1;350;0
WireConnection;353;2;351;0
WireConnection;354;0;346;0
WireConnection;377;0;364;1
WireConnection;377;1;365;0
WireConnection;331;0;327;0
WireConnection;330;0;318;0
WireConnection;330;1;329;0
WireConnection;330;2;328;0
WireConnection;378;0;377;0
WireConnection;392;0;281;0
WireConnection;335;0;2;0
WireConnection;335;1;341;0
WireConnection;356;0;353;0
WireConnection;356;1;355;0
WireConnection;356;2;354;0
WireConnection;390;0;236;0
WireConnection;368;0;378;0
WireConnection;205;0;356;0
WireConnection;205;1;238;0
WireConnection;205;2;248;0
WireConnection;386;0;281;0
WireConnection;288;0;287;0
WireConnection;362;0;313;0
WireConnection;362;1;335;0
WireConnection;362;2;392;0
WireConnection;332;0;330;0
WireConnection;332;1;331;4
WireConnection;385;0;332;0
WireConnection;385;1;318;0
WireConnection;385;2;386;0
WireConnection;289;0;2;4
WireConnection;289;1;288;0
WireConnection;369;0;368;0
WireConnection;232;0;205;0
WireConnection;232;1;362;0
WireConnection;232;2;281;0
WireConnection;391;0;207;0
WireConnection;391;1;390;0
WireConnection;333;1;318;0
WireConnection;333;0;385;0
WireConnection;371;0;232;0
WireConnection;371;1;372;0
WireConnection;371;2;391;0
WireConnection;290;0;289;0
WireConnection;393;0;313;0
WireConnection;393;1;394;0
WireConnection;357;0;335;0
WireConnection;357;1;232;0
WireConnection;379;0;369;0
WireConnection;62;0;357;0
WireConnection;62;5;379;0
WireConnection;62;7;371;0
WireConnection;62;10;290;0
WireConnection;62;11;333;0
ASEEND*/
//CHKSM=1826F4C4CE8B417CBC3E09B03242EFE439515E85