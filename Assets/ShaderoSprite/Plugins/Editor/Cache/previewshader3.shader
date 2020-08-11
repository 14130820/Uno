//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.6                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Previews/PreviewXATXQ3"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_SourceNewTex_1("_SourceNewTex_1(RGB)", 2D) = "white" { }
SpriteSheetFrameUV_Size_1("SpriteSheetFrameUV_Size_1", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_1("SpriteSheetFrameUV_Frame_1", Range(0, 15)) = 9
SpriteSheetFrameUV_Size_3("SpriteSheetFrameUV_Size_3", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_3("SpriteSheetFrameUV_Frame_3", Range(0, 15)) = 7
_FillColor_Color_4("_FillColor_Color_4", COLOR) = (1,1,1,1)
SpriteSheetFrameUV_Size_2("SpriteSheetFrameUV_Size_2", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_2("SpriteSheetFrameUV_Frame_2", Range(0, 15)) = 5
_FillColor_Color_1("_FillColor_Color_1", COLOR) = (0.3176471,0.8980392,0.3098039,1)
_SourceNewTex_2("_SourceNewTex_2(RGB)", 2D) = "white" { }
_FillColor_Color_3("_FillColor_Color_3", COLOR) = (0.3176471,0.8980392,0.3098039,1)
_SourceNewTex_3("_SourceNewTex_3(RGB)", 2D) = "white" { }
_NewTex_1("NewTex_1(RGB)", 2D) = "white" { }
_OperationBlend_Fade_7("_OperationBlend_Fade_7", Range(0, 1)) = 1
_OperationBlend_Fade_8("_OperationBlend_Fade_8", Range(0, 1)) = 0
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
sampler2D _SourceNewTex_1;
float SpriteSheetFrameUV_Size_1;
float SpriteSheetFrameUV_Frame_1;
float SpriteSheetFrameUV_Size_3;
float SpriteSheetFrameUV_Frame_3;
float4 _FillColor_Color_4;
float SpriteSheetFrameUV_Size_2;
float SpriteSheetFrameUV_Frame_2;
float4 _FillColor_Color_1;
sampler2D _SourceNewTex_2;
float4 _FillColor_Color_3;
sampler2D _SourceNewTex_3;
sampler2D _NewTex_1;
float _OperationBlend_Fade_7;
float _OperationBlend_Fade_8;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}



float4 grayscale(float4 txt,float fade)
{
float3 gs = dot(txt.rgb, float3(0.3, 0.59, 0.11));
return lerp(txt,float4(gs, txt.a), fade);
}

float4 UniColor(float4 txt, float4 color)
{
txt.rgb = lerp(txt.rgb,color.rgb,color.a);
return txt;
}
inline float RBFXmod(float x,float modu)
{
return x - floor(x * (1.0 / modu)) * modu;
}

float3 RBFXrainbow(float t)
{
t= RBFXmod(t,1.0);
float tx = t * 8;
float r = clamp(tx - 4.0, 0.0, 1.0) + clamp(2.0 - tx, 0.0, 1.0);
float g = tx < 2.0 ? clamp(tx, 0.0, 1.0) : clamp(4.0 - tx, 0.0, 1.0);
float b = tx < 4.0 ? clamp(tx - 2.0, 0.0, 1.0) : clamp(6.0 - tx, 0.0, 1.0);
return float3(r, g, b);
}

float4 Plasma(float4 txt, float2 uv, float _Fade, float speed)
{
float _TimeX=_Time.y * speed;
float a = 1.1 + _TimeX * 2.25;
float b = 0.5 + _TimeX * 1.77;
float c = 8.4 + _TimeX * 1.58;
float d = 610 + _TimeX * 2.03;
float x1 = 2.0 * uv.x;
float n = sin(a + x1) + sin(b - x1) + sin(c + 2.0 * uv.y) + sin(d + 5.0 * uv.y);
n = RBFXmod(((5.0 + n) / 5.0), 1.0);
float4 nx=txt;
n += nx.r * 0.2 + nx.g * 0.4 + nx.b * 0.2;
float4 ret=float4(RBFXrainbow(n),txt.a);
return lerp(txt,ret,_Fade);
}
float4 OperationBlend(float4 origin, float4 overlay, float blend)
{
float4 o = origin; 
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) * (o.a+0.0000001);
o.a = saturate(o.a);
o = lerp(origin, o, blend);
return o;
}
float Generate_Fire_hash2D(float2 x)
{
return frac(sin(dot(x, float2(13.454, 7.405)))*12.3043);
}

float Generate_Fire_voronoi2D(float2 uv, float precision)
{
float2 fl = floor(uv);
float2 fr = frac(uv);
float res = 1.0;
for (int j = -1; j <= 1; j++)
{
for (int i = -1; i <= 1; i++)
{
float2 p = float2(i, j);
float h = Generate_Fire_hash2D(fl + p);
float2 vp = p - fr + h;
float d = dot(vp, vp);
res += 1.0 / pow(d, 8.0);
}
}
return pow(1.0 / res, precision);
}

float4 Generate_Fire(float2 uv, float posX, float posY, float precision, float smooth, float speed, float black)
{
uv += float2(posX, posY);
float t = _Time*60*speed;
float up0 = Generate_Fire_voronoi2D(uv * float2(6.0, 4.0) + float2(0, -t), precision);
float up1 = 0.5 + Generate_Fire_voronoi2D(uv * float2(6.0, 4.0) + float2(42, -t ) + 30.0, precision);
float finalMask = up0 * up1  + (1.0 - uv.y);
finalMask += (1.0 - uv.y)* 0.5;
finalMask *= 0.7 - abs(uv.x - 0.5);
float4 result = smoothstep(smooth, 0.95, finalMask);
result.a = saturate(result.a + black);
return result;
}

float4 TurnBlackToAlpha(float4 txt, float force, float fade)
{
float3 gs = dot(txt.rgb, float3(1., 1., 1.));
gs=saturate(gs);
return lerp(txt,float4(force*txt.rgb, gs.r), fade);
}

float2 ResizeUV(float2 uv, float offsetx, float offsety, float zoomx, float zoomy)
{
uv += float2(offsetx, offsety);
uv = fmod(uv * float2(zoomx*zoomx, zoomy*zoomy), 1);
return uv;
}

float2 ResizeUVClamp(float2 uv, float offsetx, float offsety, float zoomx, float zoomy)
{
uv += float2(offsetx, offsety);
uv = fmod(clamp(uv * float2(zoomx*zoomx, zoomy*zoomy), 0.0001, 0.9999), 1);
return uv;
}
float2 SpriteSheetFrame(float2 uv, float size, float frame)
{
frame = int(frame);
uv /= size;
uv.x += fmod(frame,size) / size;
uv.y -=1/size;
uv.y += 1-floor(frame / size) / size;
return uv;
}
float4 frag (v2f i) : COLOR
{
float2 ResizeUV_1 = ResizeUVClamp(i.texcoord,-0.7,0,2.5,2.5);
float2 SpriteSheetFrameUV_1 = SpriteSheetFrame(ResizeUV_1,SpriteSheetFrameUV_Size_1,SpriteSheetFrameUV_Frame_1);
float4 SourceRGBA_1 = tex2D(_SourceNewTex_1, SpriteSheetFrameUV_1);
float2 ResizeUV_2 = ResizeUVClamp(i.texcoord,-0.145,-0.84,2.5,2.5);
float2 SpriteSheetFrameUV_3 = SpriteSheetFrame(ResizeUV_2,SpriteSheetFrameUV_Size_3,SpriteSheetFrameUV_Frame_3);
float4 SourceRGBA_2 = tex2D(_SourceNewTex_1, SpriteSheetFrameUV_3);
float4 OperationBlend_1 = OperationBlend(SourceRGBA_1, SourceRGBA_2, 1); 
float4 FillColor_4 = UniColor(OperationBlend_1,_FillColor_Color_4);
float2 ResizeUV_3 = ResizeUVClamp(i.texcoord,-0.3,-0.3,1.6,1.6);
float2 SpriteSheetFrameUV_2 = SpriteSheetFrame(ResizeUV_3,SpriteSheetFrameUV_Size_2,SpriteSheetFrameUV_Frame_2);
float4 SourceRGBA_3 = tex2D(_SourceNewTex_1, SpriteSheetFrameUV_2);
float4 FillColor_1 = UniColor(SourceRGBA_3,_FillColor_Color_1);
float4 OperationBlend_2 = OperationBlend(FillColor_4, FillColor_1, 1); 
float4 SourceRGBA_4 = tex2D(_SourceNewTex_2, i.texcoord);
float4 FillColor_3 = UniColor(SourceRGBA_4,_FillColor_Color_3);
float4 SourceRGBA_5 = tex2D(_SourceNewTex_3, i.texcoord);
float4 OperationBlend_3 = OperationBlend(SourceRGBA_5, FillColor_3, 1); 
float4 OperationBlend_4 = OperationBlend(OperationBlend_3, OperationBlend_2, 1); 
float4 FillColor_2 = UniColor(SourceRGBA_5, float4(0,0,0,1));
float4 NewTex_1 = tex2D(_NewTex_1, i.texcoord);
float4 TurnBlackToAlpha_1 = TurnBlackToAlpha(NewTex_1,1,1);
float4 OperationBlend_5 = OperationBlend(FillColor_2, TurnBlackToAlpha_1, 1); 
float4 OperationBlend_7 = OperationBlend(OperationBlend_5, OperationBlend_4, _OperationBlend_Fade_7); 
float4 _PlasmaFX_1 = Plasma(SourceRGBA_5,i.texcoord,1,0.564);
float4 GrayScale_1 = grayscale(_PlasmaFX_1,1);
float4 OperationBlend_6 = OperationBlend(GrayScale_1, OperationBlend_7, 0.695); 
float4 OperationBlend_8 = OperationBlend(OperationBlend_7, OperationBlend_6, _OperationBlend_Fade_8); 
float4 FinalResult = OperationBlend_8;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
