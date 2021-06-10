// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GUI/GradientImage"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_ScrollSpeed("ScrollSpeed", Float) = 0
		_Direction("Direction", Vector) = (0,0,0,0)

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			CompFront [_StencilComp]
			PassFront [_StencilOp]
			FailFront Keep
			ZFailFront Keep
			CompBack Always
			PassBack Keep
			FailBack Keep
			ZFailBack Keep
		}


		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		
		Pass
		{
			Name "Default"
		CGPROGRAM
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			#include "UnityShaderVariables.cginc"

			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform float4 Yellow;
			uniform float4 Red;
			uniform float4 Blue;
			uniform float4 Green;
			uniform float2 _Direction;
			uniform float _ScrollSpeed;

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 texCoord15 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 lerpResult23 = lerp( Yellow , Red , texCoord15.x);
				float2 texCoord30 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 lerpResult31 = lerp( Blue , Green , texCoord30.y);
				float2 temp_cast_0 = (3.0).xx;
				float2 normalizeResult67 = normalize( _Direction );
				float mulTime60 = _Time.y * _ScrollSpeed;
				float2 FinalUV13_g5 = ( temp_cast_0 * ( 0.5 + (texCoord15*1.0 + ( normalizeResult67 * mulTime60 )) ) );
				float2 temp_cast_1 = (0.5).xx;
				float2 temp_cast_2 = (1.0).xx;
				float4 appendResult16_g5 = (float4(ddx( FinalUV13_g5 ) , ddy( FinalUV13_g5 )));
				float4 UVDerivatives17_g5 = appendResult16_g5;
				float4 break28_g5 = UVDerivatives17_g5;
				float2 appendResult19_g5 = (float2(break28_g5.x , break28_g5.z));
				float2 appendResult20_g5 = (float2(break28_g5.x , break28_g5.z));
				float dotResult24_g5 = dot( appendResult19_g5 , appendResult20_g5 );
				float2 appendResult21_g5 = (float2(break28_g5.y , break28_g5.w));
				float2 appendResult22_g5 = (float2(break28_g5.y , break28_g5.w));
				float dotResult23_g5 = dot( appendResult21_g5 , appendResult22_g5 );
				float2 appendResult25_g5 = (float2(dotResult24_g5 , dotResult23_g5));
				float2 derivativesLength29_g5 = sqrt( appendResult25_g5 );
				float2 temp_cast_3 = (-1.0).xx;
				float2 temp_cast_4 = (1.0).xx;
				float2 clampResult57_g5 = clamp( ( ( ( abs( ( frac( ( FinalUV13_g5 + 0.25 ) ) - temp_cast_1 ) ) * 4.0 ) - temp_cast_2 ) * ( 0.35 / derivativesLength29_g5 ) ) , temp_cast_3 , temp_cast_4 );
				float2 break71_g5 = clampResult57_g5;
				float2 break55_g5 = derivativesLength29_g5;
				float4 lerpResult73_g5 = lerp( lerpResult23 , lerpResult31 , saturate( ( 0.5 + ( 0.5 * break71_g5.x * break71_g5.y * sqrt( saturate( ( 1.1 - max( break55_g5.x , break55_g5.y ) ) ) ) ) ) ));
				
				half4 color = lerpResult73_g5;
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18909
0;494;1454;505;745.1726;812.798;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;61;-470.5909,-641.7867;Inherit;False;Property;_ScrollSpeed;ScrollSpeed;0;0;Create;True;0;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;65;-343.1726,-782.798;Inherit;False;Property;_Direction;Direction;1;0;Create;True;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.NormalizeNode;67;-125.1726,-746.798;Inherit;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;60;-256.0688,-644.9375;Inherit;False;1;0;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-560,-32;Inherit;False;Global;Green;Green;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.3686275,0.7333333,0.007843138,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;30;-308.9598,-321.2929;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-340.9885,-548.7247;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-560,-208;Inherit;False;Global;Blue;Blue;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0.4117647,0.6784314,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-560,-560;Inherit;False;Global;Yellow;Yellow;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.9058824,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;86.82739,-686.798;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;3;-560,-384;Inherit;False;Global;Red;Red;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.8705883,0.03137255,0.1294118,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;49;-6.837757,-306.8296;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;-84.1326,-531.5345;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;59;231.9312,-636.9375;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT2;1.16,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;31;-79.0702,-219.0552;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;48;470.5766,-502.8721;Inherit;True;Checkerboard;-1;;5;43dad715d66e03a4c8ad5f9564018081;0;4;1;FLOAT2;0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;8;819.5406,-502.8336;Float;False;True;-1;2;ASEMaterialInspector;0;6;GUI/GradientImage;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;True;0;True;-9;False;False;False;False;False;False;False;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;True;-11;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;67;0;65;0
WireConnection;60;0;61;0
WireConnection;66;0;67;0
WireConnection;66;1;60;0
WireConnection;23;0;14;0
WireConnection;23;1;3;0
WireConnection;23;2;15;1
WireConnection;59;0;15;0
WireConnection;59;2;66;0
WireConnection;31;0;2;0
WireConnection;31;1;13;0
WireConnection;31;2;30;2
WireConnection;48;1;59;0
WireConnection;48;2;23;0
WireConnection;48;3;31;0
WireConnection;48;4;49;0
WireConnection;8;0;48;0
ASEEND*/
//CHKSM=1466B6460A203B9BED45EFC51E7BA3DEFDE6C615