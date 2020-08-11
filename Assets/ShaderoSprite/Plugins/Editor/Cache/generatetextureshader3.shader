//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.6                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Previews/GenerateXATXQ3"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_ColorGradients_Color1_1("_ColorGradients_Color1_1", COLOR) = (0,0,0,1)
_ColorGradients_Color2_1("_ColorGradients_Color2_1", COLOR) = (0.3396226,0.3396226,0.3396226,1)
_ColorGradients_Color3_1("_ColorGradients_Color3_1", COLOR) = (0.4622642,0.4622642,0.4622642,1)
_ColorGradients_Color4_1("_ColorGradients_Color4_1", COLOR) = (0.6415094,0.6415094,0.6415094,1)
_SourceNewTex_1("_SourceNewTex_1(RGB)", 2D) = "white" { }
_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

// required for UI.Mask
[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
[HideInInspector]_Stencil("Stencil ID", Float) = 0
[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
[HideInInspector]_ColorMask("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off 

// required for UI.Mask
Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp]
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}

Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
float4 color    : COLOR;
};

sampler2D _MainTex;
float _SpriteFade;
float4 _ColorGradients_Color1_1;
float4 _ColorGradients_Color2_1;
float4 _ColorGradients_Color3_1;
float4 _ColorGradients_Color4_1;
sampler2D _SourceNewTex_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float2 RotationUV(float2 uv, float rot, float posx, float posy, float speed)
{
rot=rot+(_Time*speed*360);
uv = uv - float2(posx, posy);
float angle = rot * 0.01744444;
float sinX = sin(angle);
float cosX = cos(angle);
float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
uv = mul(uv, rotationMatrix) + float2(posx, posy);
return uv;
}
float4 UniColor(float4 txt, float4 color)
{
txt.rgb = lerp(txt.rgb,color.rgb,color.a);
return txt;
}
float2 PolarCoordinatesUV(float2 uv, float size)
{
float2 r = uv - float2(0.5, 0.5);
uv.y = sqrt(r.x * r.x + r.y * r.y);
uv.y /= 0.318471;
uv.y = 1.0 - uv.y;
uv.x = atan2(r.y, r.x);
uv.x -= 1.57079632679;
if (uv.x < 0.0) { uv.x += 6.28318530718; }
uv.x /= 6.28318530718;
uv.x = 1.0 - uv.x;
return uv;
}
float4 Color_Gradients(float4 txt, float2 uv, float4 col1, float4 col2, float4 col3, float4 col4)
{
float4 c1 = lerp(col1, col2, smoothstep(0., 0.33, uv.x));
c1 = lerp(c1, col3, smoothstep(0.33, 0.66, uv.x));
c1 = lerp(c1, col4, smoothstep(0.66, 1, uv.x));
c1.a = txt.a;
return c1;
}

float4 TurnBlackToAlpha(float4 txt, float force, float fade)
{
float3 gs = dot(txt.rgb, float3(1., 1., 1.));
gs=saturate(gs);
return lerp(txt,float4(force*txt.rgb, gs.r), fade);
}

float4 frag (v2f i) : COLOR
{
float2 RotationUV_1 = RotationUV(i.texcoord,360,0.5,0.5,-3);
float2 RotationUV_2 = RotationUV(RotationUV_1,-90,0.5,0.5,0);
float2 PolarCoordinatesUV_1 = PolarCoordinatesUV(RotationUV_2,0.2);
float4 _ColorGradients_1 = Color_Gradients(float4(0,0,0,1),PolarCoordinatesUV_1,_ColorGradients_Color1_1,_ColorGradients_Color2_1,_ColorGradients_Color3_1,_ColorGradients_Color4_1);
float4 TurnBlackToAlpha_1 = TurnBlackToAlpha(_ColorGradients_1,1,1);
float4 SourceRGBA_1 = tex2D(_SourceNewTex_1, RotationUV_1);
float4 MaskAlpha_1=TurnBlackToAlpha_1;
MaskAlpha_1.a = lerp(SourceRGBA_1.a * TurnBlackToAlpha_1.a, (1 - SourceRGBA_1.a) * TurnBlackToAlpha_1.a,0);
float4 FinalResult = MaskAlpha_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
