// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BLINK/Alpha_mask"
{
	Properties
	{
		[HDR]_Colorv2("Colorv2", Color) = (1,1,1,0)
		_Color("Color", Color) = (1,1,1,0)
		_Power_Grad("Power_Grad", Float) = 0.57
		_Grad("Grad", 2D) = "white" {}
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
		uniform sampler2D _Grad;
		uniform float _Power_Grad;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float smoothstepResult16 = smoothstep( -0.11 , 0.34 , i.uv_texcoord.y);
			float temp_output_22_0 = ( (1.0 + (abs( ( i.uv_texcoord.x + -0.5 ) ) - 0.0) * (0.0 - 1.0) / (0.5 - 0.0)) * ( ( 1.0 - i.uv_texcoord.y ) * smoothstepResult16 ) );
			float2 temp_cast_0 = (pow( temp_output_22_0 , _Power_Grad )).xx;
			o.Emission = ( _Colorv2 * _Color * tex2D( _Grad, temp_cast_0 ) ).rgb;
			o.Alpha = ( temp_output_22_0 * i.vertexColor.a );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18912
0;381;1920;638;1936.011;482.8003;2.40934;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-2832.667,1.420532;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-2480.667,-211.5795;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;20;-2166.667,-230.5795;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;-2136.667,58.42053;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;16;-2036.667,364.4205;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;-0.11;False;2;FLOAT;0.34;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;21;-1919.667,-194.5795;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1754.667,228.4205;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1084.281,-187.1145;Inherit;False;Property;_Power_Grad;Power_Grad;2;0;Create;True;0;0;0;False;0;False;0.57;0.61;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1439.667,-1.579468;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;11;-880.2801,-163.1145;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-525.0786,-26.57587;Inherit;True;Property;_Grad;Grad;3;0;Create;True;0;0;0;False;0;False;-1;8104cb3b15ee8c444b7270a7b8983d0a;8104cb3b15ee8c444b7270a7b8983d0a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-548.3147,-492.8026;Inherit;False;Property;_Color;Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5801886,0.9517458,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-588.3147,-328.8026;Inherit;False;Property;_Colorv2;Colorv2;0;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;3.441591,3.441591,3.441591,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;10;-1008.975,357.889;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-774.975,266.889;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-237,-175;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;23;420,12;Float;False;True;-1;3;ASEMaterialInspector;0;0;Unlit;BLINK/Alpha_mask;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;14;1
WireConnection;20;0;15;0
WireConnection;17;0;14;2
WireConnection;16;0;14;2
WireConnection;21;0;20;0
WireConnection;18;0;17;0
WireConnection;18;1;16;0
WireConnection;22;0;21;0
WireConnection;22;1;18;0
WireConnection;11;0;22;0
WireConnection;11;1;12;0
WireConnection;13;1;11;0
WireConnection;9;0;22;0
WireConnection;9;1;10;4
WireConnection;6;0;8;0
WireConnection;6;1;7;0
WireConnection;6;2;13;0
WireConnection;23;2;6;0
WireConnection;23;9;9;0
ASEEND*/
//CHKSM=778CA56CECE760E0638C7E422C67CF6D7BFFB6B8