// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CardShader"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin][NoScaleOffset]_SymbolSheet("SymbolSheet", 2D) = "white" {}
		[NoScaleOffset]_Corners("Corners", 2D) = "white" {}
		[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
		_FractionTileSize("FractionTileSize", Float) = 0.25
		_Frame("Frame", Vector) = (0,0,0,0)
		_OffsetScale("OffsetScale", Vector) = (0,0,1,0)
		_Color("Color", Color) = (1,0,0,0)
		_HighlightIntensity("HighlightIntensity", Range( 0 , 1)) = 0
		[ASEEnd]_HighlightColor("HighlightColor", Color) = (0.9811321,0.9429609,0.495194,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Back
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 100500

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _HighlightColor;
			float3 _OffsetScale;
			float2 _Frame;
			float _FractionTileSize;
			float _HighlightIntensity;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainTex;
			sampler2D _Corners;
			sampler2D _SymbolSheet;


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				float temp_output_7_0_g78 = ( 1.0 - 0.5 );
				float2 uv_MainTex83 = IN.ase_texcoord3.xy;
				float temp_output_3_0_g78 = tex2D( _MainTex, uv_MainTex83 ).a;
				float temp_output_11_0_g78 = ( fwidth( temp_output_3_0_g78 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g78 = smoothstep( ( temp_output_7_0_g78 - temp_output_11_0_g78 ) , ( temp_output_11_0_g78 + temp_output_7_0_g78 ) , temp_output_3_0_g78);
				float temp_output_84_0 = smoothstepResult12_g78;
				float temp_output_7_0_g77 = ( 1.0 - 0.5 );
				float2 uv_Corners70 = IN.ase_texcoord3.xy;
				float temp_output_3_0_g77 = tex2D( _Corners, uv_Corners70 ).a;
				float temp_output_11_0_g77 = ( fwidth( temp_output_3_0_g77 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g77 = smoothstep( ( temp_output_7_0_g77 - temp_output_11_0_g77 ) , ( temp_output_11_0_g77 + temp_output_7_0_g77 ) , temp_output_3_0_g77);
				float3 OffsetScale219 = _OffsetScale;
				float temp_output_2_0_g66 = ( (OffsetScale219).z - 1.0 );
				float temp_output_203_0 = ( 1.0 - temp_output_2_0_g66 );
				float2 temp_cast_0 = (temp_output_203_0).xx;
				float3 temp_output_205_0 = ( ( temp_output_2_0_g66 * 0.5 ) + ( OffsetScale219 * float3( -1,-1,0 ) ) );
				float2 texCoord211 = IN.ase_texcoord3.xy * temp_cast_0 + temp_output_205_0.xy;
				float FractionTileSize135 = _FractionTileSize;
				float2 appendResult10_g74 = (float2(FractionTileSize135 , FractionTileSize135));
				float2 temp_output_11_0_g74 = ( abs( (texCoord211*2.0 + -1.0) ) - appendResult10_g74 );
				float2 break16_g74 = ( 1.0 - ( temp_output_11_0_g74 / fwidth( temp_output_11_0_g74 ) ) );
				float2 temp_cast_2 = (temp_output_203_0).xx;
				float temp_output_1_0_g69 = FractionTileSize135;
				float temp_output_5_0_g69 = ( temp_output_1_0_g69 + ( temp_output_1_0_g69 * 0.5 ) );
				float2 appendResult11_g69 = (float2(( temp_output_5_0_g69 * -1.0 ) , temp_output_5_0_g69));
				float2 Frame198 = _Frame;
				float2 break14_g69 = Frame198;
				float2 appendResult7_g69 = (float2(break14_g69.x , ( break14_g69.y * -1.0 )));
				float2 texCoord207 = IN.ase_texcoord3.xy * temp_cast_2 + ( temp_output_205_0 + float3( ( appendResult11_g69 + ( appendResult7_g69 * temp_output_1_0_g69 ) ) ,  0.0 ) ).xy;
				float4 tex2DNode215 = tex2D( _SymbolSheet, texCoord207 );
				float temp_output_7_0_g71 = ( 1.0 - 0.5 );
				float temp_output_3_0_g71 = tex2DNode215.a;
				float temp_output_11_0_g71 = ( fwidth( temp_output_3_0_g71 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g71 = smoothstep( ( temp_output_7_0_g71 - temp_output_11_0_g71 ) , ( temp_output_11_0_g71 + temp_output_7_0_g71 ) , temp_output_3_0_g71);
				float temp_output_2_0_g65 = ( _OffsetScale.z - 1.0 );
				float temp_output_185_0 = ( 1.0 - temp_output_2_0_g65 );
				float2 temp_cast_5 = (temp_output_185_0).xx;
				float3 temp_output_188_0 = ( ( temp_output_2_0_g65 * 0.5 ) + OffsetScale219 );
				float2 texCoord192 = IN.ase_texcoord3.xy * temp_cast_5 + temp_output_188_0.xy;
				float2 appendResult10_g72 = (float2(FractionTileSize135 , FractionTileSize135));
				float2 temp_output_11_0_g72 = ( abs( (texCoord192*2.0 + -1.0) ) - appendResult10_g72 );
				float2 break16_g72 = ( 1.0 - ( temp_output_11_0_g72 / fwidth( temp_output_11_0_g72 ) ) );
				float2 temp_cast_7 = (temp_output_185_0).xx;
				float temp_output_1_0_g67 = FractionTileSize135;
				float temp_output_5_0_g67 = ( temp_output_1_0_g67 + ( temp_output_1_0_g67 * 0.5 ) );
				float2 appendResult11_g67 = (float2(( temp_output_5_0_g67 * -1.0 ) , temp_output_5_0_g67));
				float2 break14_g67 = Frame198;
				float2 appendResult7_g67 = (float2(break14_g67.x , ( break14_g67.y * -1.0 )));
				float2 texCoord190 = IN.ase_texcoord3.xy * temp_cast_7 + ( temp_output_188_0 + float3( ( appendResult11_g67 + ( appendResult7_g67 * temp_output_1_0_g67 ) ) ,  0.0 ) ).xy;
				float4 tex2DNode193 = tex2D( _SymbolSheet, texCoord190 );
				float temp_output_7_0_g73 = ( 1.0 - 0.5 );
				float temp_output_3_0_g73 = tex2DNode193.a;
				float temp_output_11_0_g73 = ( fwidth( temp_output_3_0_g73 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g73 = smoothstep( ( temp_output_7_0_g73 - temp_output_11_0_g73 ) , ( temp_output_11_0_g73 + temp_output_7_0_g73 ) , temp_output_3_0_g73);
				float temp_output_2_0_g68 = ( 1.5 - 1.0 );
				float temp_output_175_0 = ( 1.0 - temp_output_2_0_g68 );
				float2 temp_cast_10 = (temp_output_175_0).xx;
				float temp_output_178_0 = ( ( temp_output_2_0_g68 * 0.5 ) + 0.0 );
				float2 temp_cast_11 = (temp_output_178_0).xx;
				float2 texCoord170 = IN.ase_texcoord3.xy * temp_cast_10 + temp_cast_11;
				float2 appendResult10_g75 = (float2(FractionTileSize135 , FractionTileSize135));
				float2 temp_output_11_0_g75 = ( abs( (texCoord170*2.0 + -1.0) ) - appendResult10_g75 );
				float2 break16_g75 = ( 1.0 - ( temp_output_11_0_g75 / fwidth( temp_output_11_0_g75 ) ) );
				float2 temp_cast_12 = (temp_output_175_0).xx;
				float temp_output_1_0_g70 = FractionTileSize135;
				float temp_output_5_0_g70 = ( temp_output_1_0_g70 + ( temp_output_1_0_g70 * 0.5 ) );
				float2 appendResult11_g70 = (float2(( temp_output_5_0_g70 * -1.0 ) , temp_output_5_0_g70));
				float2 break14_g70 = Frame198;
				float2 appendResult7_g70 = (float2(break14_g70.x , ( break14_g70.y * -1.0 )));
				float2 texCoord81 = IN.ase_texcoord3.xy * temp_cast_12 + ( temp_output_178_0 + ( appendResult11_g70 + ( appendResult7_g70 * temp_output_1_0_g70 ) ) );
				float4 tex2DNode1 = tex2D( _SymbolSheet, texCoord81 );
				float temp_output_7_0_g76 = ( 1.0 - 0.5 );
				float temp_output_3_0_g76 = tex2DNode1.a;
				float temp_output_11_0_g76 = ( fwidth( temp_output_3_0_g76 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g76 = smoothstep( ( temp_output_7_0_g76 - temp_output_11_0_g76 ) , ( temp_output_11_0_g76 + temp_output_7_0_g76 ) , temp_output_3_0_g76);
				float4 lerpResult244 = lerp( ( ( temp_output_84_0 * _Color ) + ( smoothstepResult12_g77 * ( 1.0 - ( ( saturate( min( break16_g74.x , break16_g74.y ) ) * tex2DNode215 * smoothstepResult12_g71 ) + float4( 0,0,0,0 ) + ( saturate( min( break16_g72.x , break16_g72.y ) ) * tex2DNode193 * smoothstepResult12_g73 ) ) ) ) + ( saturate( min( break16_g75.x , break16_g75.y ) ) * tex2DNode1 * smoothstepResult12_g76 ) ) , _HighlightColor , _HighlightIntensity);
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = lerpResult244.rgb;
				float Alpha = temp_output_84_0;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 100500

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _HighlightColor;
			float3 _OffsetScale;
			float2 _Frame;
			float _FractionTileSize;
			float _HighlightIntensity;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainTex;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = TransformWorldToHClip( positionWS );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float temp_output_7_0_g78 = ( 1.0 - 0.5 );
				float2 uv_MainTex83 = IN.ase_texcoord2.xy;
				float temp_output_3_0_g78 = tex2D( _MainTex, uv_MainTex83 ).a;
				float temp_output_11_0_g78 = ( fwidth( temp_output_3_0_g78 ) * ( 1.0 + 0.5 ) );
				float smoothstepResult12_g78 = smoothstep( ( temp_output_7_0_g78 - temp_output_11_0_g78 ) , ( temp_output_11_0_g78 + temp_output_7_0_g78 ) , temp_output_3_0_g78);
				float temp_output_84_0 = smoothstepResult12_g78;
				
				float Alpha = temp_output_84_0;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

	
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18909
157;435;1496;526;-478.5614;552.2998;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;182;-2800,352;Inherit;False;2261.395;756.8467;Comment;15;196;195;194;193;192;191;190;189;188;187;185;199;200;184;219;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;180;-2787.929,-451.6235;Inherit;False;2261.395;756.8467;Comment;13;122;175;135;145;178;169;81;176;170;119;128;198;217;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;201;-2763.792,-1237.59;Inherit;False;2261.395;756.8467;Comment;15;215;214;213;212;211;210;209;208;207;206;205;204;203;221;223;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;83;327.4291,-544.975;Inherit;True;Property;_MainTex;MainTex;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;f385a43af4916a54db9993547a110350;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-784,752;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;169;-1891.127,28.27863;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;194;-1088,544;Inherit;True;Rectangle;-1;;72;6b23e0c975270fb4084c354b2c83366a;0;3;1;FLOAT2;0,0;False;2;FLOAT;0.25;False;3;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;195;-1088,848;Inherit;True;SDF;-1;;73;dff82bfe8a0e4e746a52c3f8c3fba486;0;3;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;212;-1051.792,-1045.59;Inherit;True;Rectangle;-1;;74;6b23e0c975270fb4084c354b2c83366a;0;3;1;FLOAT2;0,0;False;2;FLOAT;0.25;False;3;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;81;-1686.029,-12.67953;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.99,0.9;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;87;1180.805,-147.3749;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;209;-747.7922,-837.5898;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;223;-2511.81,-983.7432;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;-1,-1,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1;-1432.704,-42.29276;Inherit;True;Property;_SymbolSheet;SymbolSheet;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;c67293b3d5ddfa0438fe421ce525c412;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;70;319.9218,-270.4657;Inherit;True;Property;_Corners;Corners;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;3287bdf26573c614e817d55b2068a734;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;176;-1314.979,-182.5457;Inherit;False;135;FractionTileSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;208;-1051.792,-741.5898;Inherit;True;SDF;-1;;71;dff82bfe8a0e4e746a52c3f8c3fba486;0;3;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-762.5338,-55.62898;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;227;-91.01253,79.29874;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;215;-1403.792,-821.5898;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;c67293b3d5ddfa0438fe421ce525c412;True;0;False;white;Auto;False;Instance;1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;243;1314.462,-402.7998;Inherit;False;Property;_HighlightColor;HighlightColor;8;0;Create;True;0;0;0;False;0;False;0.9811321,0.9429609,0.495194,0;0.9811321,0.9429609,0.4951938,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;170;-1326.282,-308.3707;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;237;327.2065,37.25411;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;84;647.7291,-476.3522;Inherit;True;SDF;-1;;78;dff82bfe8a0e4e746a52c3f8c3fba486;0;3;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;85;684.1282,-655.7518;Inherit;False;Property;_Color;Color;6;0;Create;True;0;0;0;False;0;False;1,0,0,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;16;-1070.847,51.22323;Inherit;True;SDF;-1;;76;dff82bfe8a0e4e746a52c3f8c3fba486;0;3;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;119;-1076.086,-251.4998;Inherit;True;Rectangle;-1;;75;6b23e0c975270fb4084c354b2c83366a;0;3;1;FLOAT2;0,0;False;2;FLOAT;0.25;False;3;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;928.9144,-91.07623;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;935.421,-586.4805;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;71;646.6756,-239.8664;Inherit;True;SDF;-1;;77;dff82bfe8a0e4e746a52c3f8c3fba486;0;3;2;FLOAT;0.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;210;-1291.792,-965.5898;Inherit;False;135;FractionTileSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;244;1631.661,-275.3995;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;211;-1307.792,-1093.59;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;184;-2769.206,643.7272;Inherit;False;Property;_OffsetScale;OffsetScale;5;0;Create;True;0;0;0;False;0;False;0,0,1;0.32,-0.5,0.68;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;188;-2064,640;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;205;-2027.792,-949.5898;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;145;-2253.807,46.28651;Inherit;False;Atlas;-1;;70;19ffdab529b47e34ab2704e71154e44f;0;2;1;FLOAT;0.25;False;13;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-2737.929,18.12354;Inherit;False;Property;_FractionTileSize;FractionTileSize;3;0;Create;True;0;0;0;False;0;False;0.25;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;222;-2887.036,-1028.178;Inherit;False;219;OffsetScale;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;221;-2672.267,-1186.17;Inherit;False;False;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;-2532.273,822.6764;Inherit;False;135;FractionTileSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;214;-2496.065,-766.9135;Inherit;False;135;FractionTileSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;135;-2494.511,16.3687;Inherit;False;FractionTileSize;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;203;-2251.792,-1189.59;Inherit;False;UVConstantScale;-1;;66;74d840f5af494f14d96fe3ac4044eda3;0;1;7;FLOAT;0;False;2;FLOAT;0;FLOAT;6
Node;AmplifyShaderEditor.SimpleAddOpNode;178;-2043.465,-159.8273;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;193;-1440,768;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;c67293b3d5ddfa0438fe421ce525c412;True;0;False;white;Auto;False;Instance;1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;198;-2488.469,119.6545;Inherit;False;Frame;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;185;-2288,400;Inherit;False;UVConstantScale;-1;;65;74d840f5af494f14d96fe3ac4044eda3;0;1;7;FLOAT;0;False;2;FLOAT;0;FLOAT;6
Node;AmplifyShaderEditor.GetLocalVarNode;213;-2480.065,-686.9135;Inherit;False;198;Frame;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;219;-2273.187,655.5986;Inherit;False;OffsetScale;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;190;-1696,784;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.99,0.9;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;189;-1904,832;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;187;-2272,848;Inherit;False;Atlas;-1;;67;19ffdab529b47e34ab2704e71154e44f;0;2;1;FLOAT;0.25;False;13;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;206;-1867.792,-757.5898;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;207;-1659.792,-805.5898;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.99,0.9;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;217;-2415.649,-350.323;Inherit;False;Constant;_Scale;Scale;8;0;Create;True;0;0;0;False;0;False;1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;-2516.273,902.6764;Inherit;False;198;Frame;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;192;-1344,496;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;191;-1328,624;Inherit;False;135;FractionTileSize;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;128;-2674.533,121.3123;Inherit;False;Property;_Frame;Frame;4;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;204;-2235.792,-741.5898;Inherit;False;Atlas;-1;;69;19ffdab529b47e34ab2704e71154e44f;0;2;1;FLOAT;0.25;False;13;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;175;-2257.18,-356.2951;Inherit;False;UVConstantScale;-1;;68;74d840f5af494f14d96fe3ac4044eda3;0;1;7;FLOAT;0;False;2;FLOAT;0;FLOAT;6
Node;AmplifyShaderEditor.RangedFloatNode;245;1454.861,-80.39971;Inherit;False;Property;_HighlightIntensity;HighlightIntensity;7;0;Create;True;0;0;0;False;0;False;0;0.566;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;240;1519.158,-193.21;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;241;1519.158,-193.21;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;242;1519.158,-193.21;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;238;1519.158,-193.21;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;239;1843.558,-220.4101;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;CardShader;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;1;5;False;-1;10;False;-1;1;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;  Blend;0;Two Sided;1;Cast Shadows;0;  Use Shadow Threshold;0;Receive Shadows;0;GPU Instancing;1;LOD CrossFade;0;Built-in Fog;0;DOTS Instancing;0;Meta Pass;0;Extra Pre Pass;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;5;False;True;False;True;False;False;;False;0
WireConnection;196;0;194;0
WireConnection;196;1;193;0
WireConnection;196;2;195;0
WireConnection;169;0;178;0
WireConnection;169;1;145;0
WireConnection;194;1;192;0
WireConnection;194;2;191;0
WireConnection;194;3;191;0
WireConnection;195;3;193;4
WireConnection;212;1;211;0
WireConnection;212;2;210;0
WireConnection;212;3;210;0
WireConnection;81;0;175;0
WireConnection;81;1;169;0
WireConnection;87;0;86;0
WireConnection;87;1;226;0
WireConnection;87;2;91;0
WireConnection;209;0;212;0
WireConnection;209;1;215;0
WireConnection;209;2;208;0
WireConnection;223;0;222;0
WireConnection;1;1;81;0
WireConnection;208;3;215;4
WireConnection;91;0;119;0
WireConnection;91;1;1;0
WireConnection;91;2;16;0
WireConnection;227;0;209;0
WireConnection;227;2;196;0
WireConnection;215;1;207;0
WireConnection;170;0;175;0
WireConnection;170;1;178;0
WireConnection;237;0;227;0
WireConnection;84;3;83;4
WireConnection;16;3;1;4
WireConnection;119;1;170;0
WireConnection;119;2;176;0
WireConnection;119;3;176;0
WireConnection;226;0;71;0
WireConnection;226;1;237;0
WireConnection;86;0;84;0
WireConnection;86;1;85;0
WireConnection;71;3;70;4
WireConnection;244;0;87;0
WireConnection;244;1;243;0
WireConnection;244;2;245;0
WireConnection;211;0;203;0
WireConnection;211;1;205;0
WireConnection;188;0;185;6
WireConnection;188;1;219;0
WireConnection;205;0;203;6
WireConnection;205;1;223;0
WireConnection;145;1;135;0
WireConnection;145;13;198;0
WireConnection;221;0;222;0
WireConnection;135;0;122;0
WireConnection;203;7;221;0
WireConnection;178;0;175;6
WireConnection;193;1;190;0
WireConnection;198;0;128;0
WireConnection;185;7;184;3
WireConnection;219;0;184;0
WireConnection;190;0;185;0
WireConnection;190;1;189;0
WireConnection;189;0;188;0
WireConnection;189;1;187;0
WireConnection;187;1;200;0
WireConnection;187;13;199;0
WireConnection;206;0;205;0
WireConnection;206;1;204;0
WireConnection;207;0;203;0
WireConnection;207;1;206;0
WireConnection;192;0;185;0
WireConnection;192;1;188;0
WireConnection;204;1;214;0
WireConnection;204;13;213;0
WireConnection;175;7;217;0
WireConnection;239;2;244;0
WireConnection;239;3;84;0
ASEEND*/
//CHKSM=C745528C6943B02585D5A48B5D94FAF55E5D2BA7