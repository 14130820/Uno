//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.6                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/Background_AutoMove"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_EmbossFull_Angle_1("_EmbossFull_Angle_1", Range(-1, 1)) = 0.514
_EmbossFull_Distance_1("_EmbossFull_Distance_1", Range(0, 16)) = -2.529
_EmbossFull_Intensity_1("_EmbossFull_Intensity_1", Range(-2, 2)) = 1
_EmbossFull_grayfade_1("_EmbossFull_grayfade_1", Range(-2, 2)) = 1
_EmbossFull_original_1("_EmbossFull_original_1", Range(-2, 2)) = 1
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
float _EmbossFull_Angle_1;
float _EmbossFull_Distance_1;
float _EmbossFull_Intensity_1;
float _EmbossFull_grayfade_1;
float _EmbossFull_original_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float4 PageCurl(sampler2D txt, float2 uv, float m1, float m2, float radius, float Shadow)
{
float4 text = float4(0,0,0,1);
float2 mouse = float2(m1, m1);
float2 mdir = float2(m2, -0.2);
float2 mouseDir = normalize(abs(mdir)*mdir);
float2 origin = mouse-mdir;
float mouseDist = origin;
mouseDist = distance(mouse, origin);
float proj = dot(uv - origin, mouseDir);
float dist = proj - mouseDist;
float2 linePoint = uv - dist * mouseDir;
if (dist > radius)
{
text.a = 1 - pow(saturate(dist - radius) * 8.5, Shadow * 0.05);
text.a *= tex2D(txt, uv).a;
}
else if (dist >= 0.)
{
float theta = asin(dist / radius);
float2 p2 = linePoint + mouseDir * (3.1415 - theta) * radius;
float2 p1 = linePoint + mouseDir * theta * radius;
float px = 1;
if ((p2.x < 0) || (p2.x > 1) || (p2.y < 0) || (p2.y > 1)) px = 0;
float4 t1 = tex2D(txt, p1);
float4 t2 = tex2D(txt, p2);
t1 = lerp(float4(0, 0, 0, tex2D(txt, uv).a*Shadow * 0.125), t1, t1.a);
text = lerp(float4(t1.rgb,t1.a), float4(t2.rgb,1), t2.a*px);
text.rgb *= pow(saturate((radius - dist) / radius), .2);
}
else
{
float2 p = linePoint + mouseDir * (abs(dist) + 3.1415 * radius);
float px = 1;
if ((p.x < 0) || (p.x > 1) || (p.y < 0) || (p.y > 1)) px = 0;
float4 t1 = tex2D(txt, uv);
float4 t2 = tex2D(txt, p);
text = lerp(float4(t1.rgb, t1.a), float4(t2.rgb, px), t2.a*px);
text=saturate(text);
}
return saturate(text);
}
float4 EmbossFull(sampler2D txt, float2 uv, float angle, float dist, float intensity, float g, float o)
{
angle = angle *3.1415926;
intensity = intensity *0.25;
#define rot(n) mul(n, float2x2(cos(angle), -sin(angle), sin(angle), cos(angle)))
float m1 = 0; float m2 = 0; float m3 = 0;
float m4 = 0; float m5 = 1; float m6 = 0;
float m7 = 0; float m8 = 0; float m9 = -1;
float Offset = 0.5;
float Scale = 1;
float4 r = float4(0, 0, 0, 0);
dist = dist * 0.005;
float4 rgb = tex2D(txt, uv);
r += tex2D(txt, uv + rot(float2(-dist, -dist))) * m1*intensity;
r += tex2D(txt, uv + rot(float2(0, -dist))) * m2*intensity;
r += tex2D(txt, uv + rot(float2(dist, -dist))) * m3*intensity;
r += tex2D(txt, uv + rot(float2(-dist, 0))) * m4*intensity;
r += tex2D(txt, uv + rot(float2(0, 0)))* m5*intensity;
r += tex2D(txt, uv + rot(float2(dist, 0))) * m6*intensity;
r += tex2D(txt, uv + rot(float2(-dist, dist))) * m7*intensity;
r += tex2D(txt, uv + rot(float2(0, dist))) * m8*intensity;
r += tex2D(txt, uv + rot(float2(dist, dist))) * m9*intensity;
r = lerp(r,dot(r.rgb,3),g);
r = lerp(r+0.5,rgb+r,o);
r = saturate(r);
r.a = rgb.a;
return r;
}
float4 frag (v2f i) : COLOR
{
float4 _EmbossFull_1 = EmbossFull(_MainTex,i.texcoord,_EmbossFull_Angle_1,_EmbossFull_Distance_1,_EmbossFull_Intensity_1,_EmbossFull_grayfade_1,_EmbossFull_original_1);
float4 FinalResult = _EmbossFull_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
