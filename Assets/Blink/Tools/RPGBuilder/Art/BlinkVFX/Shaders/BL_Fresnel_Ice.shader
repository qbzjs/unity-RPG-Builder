// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BLINK/Ice_1"
{
	Properties
	{
		_Scale("Scale", Float) = 1
		_Power("Power", Float) = 5
		_Color("Color", Color) = (1,1,1,0)
		[HDR]_Color2("Color2", Color) = (1,1,1,0)
		_Vector0("Vector 0", Vector) = (0,0,0,0)
		_HSV_aux("HSV_aux", Float) = 30
		_Smoothstep("Smoothstep", Vector) = (0,1,0,0)
		_aux_smooth("aux_smooth", Float) = 0
		_Vector1("Vector 1", Vector) = (6,6,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Unlit keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
			float4 uv_tex4coord;
		};

		uniform float _HSV_aux;
		uniform float4 _Color;
		uniform float4 _Color2;
		uniform float _Scale;
		uniform float _Power;
		uniform float2 _Vector1;
		uniform float _aux_smooth;
		uniform float2 _Smoothstep;
		uniform float2 _Vector0;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		float2 voronoihash24( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi24( float2 v, float time, inout float2 id, inout float2 mr, float smoothness, inout float2 smoothId )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash24( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			
			 		}
			 	}
			}
			return F1;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 hsvTorgb39 = RGBToHSV( _Color.rgb );
			float3 hsvTorgb44 = HSVToRGB( float3(( _HSV_aux + hsvTorgb39.x ),1.0,1.0) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV5 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode5 = ( 0.0 + _Scale * pow( 1.0 - fresnelNdotV5, _Power ) );
			float time24 = 0.0;
			float2 voronoiSmoothId0 = 0;
			float2 uv_TexCoord25 = i.uv_texcoord * float2( 1.5,1.5 );
			float2 coords24 = ( ( uv_TexCoord25 + i.uv_tex4coord.z ) * _Vector1 ) * 2.21;
			float2 id24 = 0;
			float2 uv24 = 0;
			float fade24 = 0.5;
			float voroi24 = 0;
			float rest24 = 0;
			for( int it24 = 0; it24 <2; it24++ ){
			voroi24 += fade24 * voronoi24( coords24, time24, id24, uv24, 0,voronoiSmoothId0 );
			rest24 += fade24;
			coords24 *= 2;
			fade24 *= 0.5;
			}//Voronoi24
			voroi24 /= rest24;
			float smoothstepResult48 = smoothstep( _Smoothstep.x , _Smoothstep.y , i.uv_texcoord.y);
			float4 lerpResult30 = lerp( float4( hsvTorgb44 , 0.0 ) , ( _Color * _Color2 * ( 0.1 + max( ( saturate( fresnelNode5 ) * i.uv_texcoord.y ) , voroi24 ) ) ) , saturate( ( _aux_smooth + smoothstepResult48 ) ));
			float smoothstepResult29 = smoothstep( _Vector0.x , _Vector0.y , voroi24);
			float4 lerpResult53 = lerp( lerpResult30 , _Color2 , smoothstepResult29);
			o.Emission = lerpResult53.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;363;1920;656;-952.3819;87.23463;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;8;-887,185;Inherit;False;Property;_Scale;Scale;0;0;Create;True;0;0;0;False;0;False;1;1.55;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-1323.5,597.8803;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;10;-884,278;Inherit;False;Property;_Power;Power;1;0;Create;True;0;0;0;False;0;False;5;0.47;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-1321,301.1043;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1.5,1.5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-1053.905,517.7307;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FresnelNode;5;-657,86;Inherit;True;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;54;-1018.259,652.2684;Inherit;False;Property;_Vector1;Vector 1;9;0;Create;True;0;0;0;False;0;False;6,6;4,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;13;-313,-266;Inherit;False;Property;_Color;Color;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5801886,0.9517458,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-824.8854,448.5495;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-839.4972,719.8057;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;12;-336,168;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-338.6395,463.0636;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;49;411.2336,343.5382;Inherit;False;Property;_Smoothstep;Smoothstep;7;0;Create;True;0;0;0;False;0;False;0,1;0.18,0.77;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;47;592.2705,259.6752;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VoronoiNode;24;-639.3838,400.6371;Inherit;True;0;0;1;0;2;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;2.21;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.RGBToHSVNode;39;179.4453,-480.251;Inherit;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SmoothstepOpNode;48;951.5123,308.922;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;844.6616,138.6818;Inherit;False;Property;_aux_smooth;aux_smooth;8;0;Create;True;0;0;0;False;0;False;0;0.14;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;632.3209,-552.6526;Inherit;False;Property;_HSV_aux;HSV_aux;6;0;Create;True;0;0;0;False;0;False;30;24.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;41;562.3585,-404.2325;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;23;-130.0377,254.2505;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;26;-46.87134,488.113;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;726.2797,-450.9032;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-318,-90.24101;Inherit;False;Property;_Color2;Color2;3;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;4.541205,4.541205,4.541205,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;50;1075.076,111.9331;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;184.9068,461.2668;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;52;1230.099,204.8295;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-57,-117;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;34;-250.3406,899.7596;Inherit;False;Property;_Vector0;Vector 0;5;0;Create;True;0;0;0;False;0;False;0,0;-0.05,1.12;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.HSVToRGBNode;44;1077.538,-451.1305;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;30;1343.222,-32.99678;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;29;-27.67848,829.6647;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.41;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-890.3307,552.9515;Inherit;False;Property;_scale;scale;4;0;Create;True;0;0;0;False;0;False;2.21;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;53;1789.119,479.1541;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;56;2501.236,145.7821;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;BLINK/Ice_1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;33;0;25;0
WireConnection;33;1;32;3
WireConnection;5;2;8;0
WireConnection;5;3;10;0
WireConnection;55;0;33;0
WireConnection;55;1;54;0
WireConnection;12;0;5;0
WireConnection;20;0;12;0
WireConnection;20;1;18;2
WireConnection;24;0;55;0
WireConnection;39;0;13;0
WireConnection;48;0;47;2
WireConnection;48;1;49;1
WireConnection;48;2;49;2
WireConnection;41;0;39;0
WireConnection;26;0;20;0
WireConnection;26;1;24;0
WireConnection;43;0;42;0
WireConnection;43;1;41;0
WireConnection;50;0;51;0
WireConnection;50;1;48;0
WireConnection;22;0;23;0
WireConnection;22;1;26;0
WireConnection;52;0;50;0
WireConnection;17;0;13;0
WireConnection;17;1;16;0
WireConnection;17;2;22;0
WireConnection;44;0;43;0
WireConnection;30;0;44;0
WireConnection;30;1;17;0
WireConnection;30;2;52;0
WireConnection;29;0;24;0
WireConnection;29;1;34;1
WireConnection;29;2;34;2
WireConnection;53;0;30;0
WireConnection;53;1;16;0
WireConnection;53;2;29;0
WireConnection;56;2;53;0
ASEEND*/
//CHKSM=A3ECD8AFE9754212C0B24CD59E6F14263747791C