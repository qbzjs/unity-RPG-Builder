// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TitanForge/Sample Shader"
{
	Properties
	{
		_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		_MetallicMask("Metallic Mask", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0.6
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.6
		_Emission("Emission", Range( 0 , 1)) = 1
		_EmissionColor("Emission Color", Color) = (0.6352941,0.3764706,0.3019608,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _AlbedoTexture;
		uniform float4 _AlbedoTexture_ST;
		SamplerState sampler_AlbedoTexture;
		uniform float _Emission;
		uniform float4 _EmissionColor;
		uniform sampler2D _MetallicMask;
		SamplerState sampler_MetallicMask;
		uniform float4 _MetallicMask_ST;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_AlbedoTexture = i.uv_texcoord * _AlbedoTexture_ST.xy + _AlbedoTexture_ST.zw;
			float4 tex2DNode1 = tex2D( _AlbedoTexture, uv_AlbedoTexture );
			o.Albedo = tex2DNode1.rgb;
			o.Emission = ( ( tex2DNode1.a * _Emission ) * _EmissionColor ).rgb;
			float2 uv_MetallicMask = i.uv_texcoord * _MetallicMask_ST.xy + _MetallicMask_ST.zw;
			float4 tex2DNode4 = tex2D( _MetallicMask, uv_MetallicMask );
			o.Metallic = ( tex2DNode4.r * _Metallic );
			o.Smoothness = ( tex2DNode4.r * _Smoothness );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
0;73.6;1310;680;1041.521;24.84708;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;3;-657.8998,120.9;Inherit;False;Property;_Emission;Emission;4;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-665.8998,-79.1;Inherit;True;Property;_AlbedoTexture;Albedo Texture;0;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-320.3244,58.49833;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-672.4352,286.0707;Inherit;True;Property;_MetallicMask;Metallic Mask;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-661.4352,479.0707;Inherit;False;Property;_Metallic;Metallic;2;0;Create;True;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-658.4352,554.0707;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;False;0.6;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;10;-368.05,163.6686;Inherit;False;Property;_EmissionColor;Emission Color;5;0;Create;True;0;0;False;0;False;0.6352941,0.3764706,0.3019608,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-160.7012,70.14906;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-169.4461,270.4984;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-160.3223,373.0048;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TitanForge/Sample Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;4
WireConnection;2;1;3;0
WireConnection;9;0;2;0
WireConnection;9;1;10;0
WireConnection;5;0;4;1
WireConnection;5;1;6;0
WireConnection;8;0;4;1
WireConnection;8;1;7;0
WireConnection;0;0;1;0
WireConnection;0;2;9;0
WireConnection;0;3;5;0
WireConnection;0;4;8;0
ASEEND*/
//CHKSM=5A20D983FE455CD44220F3783CB4DA2A910E85A8