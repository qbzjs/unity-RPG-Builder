// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BLINK/Wave"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		[HDR]_Colorv2("Colorv2", Color) = (1,1,1,0)
		_PowerAlpha("PowerAlpha", Float) = 1
		_Alpha("Alpha", Float) = 1
		_Mask("Mask", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float4 _Colorv2;
		uniform float4 _Color;
		uniform float _PowerAlpha;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float _Alpha;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = ( _Colorv2 * _Color ).rgb;
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float smoothstepResult9 = smoothstep( 0.0 , _PowerAlpha , tex2D( _Mask, uv_Mask ).r);
			o.Alpha = ( smoothstepResult9 * i.vertexColor.a * _Alpha );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;357;1920;662;2231.555;407.4589;2.030088;True;False
Node;AmplifyShaderEditor.RangedFloatNode;10;-666.786,417.96;Inherit;False;Property;_PowerAlpha;PowerAlpha;2;0;Create;True;0;0;0;False;0;False;1;4.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-851.9436,203.1255;Inherit;True;Property;_Mask;Mask;4;0;Create;True;0;0;0;False;0;False;-1;None;b15f33783c3cdf741a6af366fabf85c9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;9;-447.786,254.96;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.31;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;12;-413.786,480.96;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-355.786,704.96;Inherit;False;Property;_Alpha;Alpha;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-391,-211.5;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5801886,0.9517458,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-412.7539,-35.62567;Inherit;False;Property;_Colorv2;Colorv2;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;220.2618,220.2618,220.2618,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-71.78601,386.96;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-83.89795,25.81427;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;17;132,10;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;BLINK/Wave;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;14;1
WireConnection;9;2;10;0
WireConnection;11;0;9;0
WireConnection;11;1;12;4
WireConnection;11;2;13;0
WireConnection;16;0;15;0
WireConnection;16;1;6;0
WireConnection;17;2;16;0
WireConnection;17;9;11;0
ASEEND*/
//CHKSM=82F81A9BA299903C7C1021AE94DAD2394F1D4D1E