//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.6                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Previews/PreviewXATXQ2"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_SourceNewTex_1("_SourceNewTex_1(RGB)", 2D) = "white" { }
SpriteSheetFrameUV_Size_1("SpriteSheetFrameUV_Size_1", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_1("SpriteSheetFrameUV_Frame_1", Range(0, 15)) = 9
SpriteSheetFrameUV_Size_3("SpriteSheetFrameUV_Size_3", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_3("SpriteSheetFrameUV_Frame_3", Range(0, 15)) = 7
_FillColor_Color_3("_FillColor_Color_3", COLOR) = (1,1,1,1)
SpriteSheetFrameUV_Size_2("SpriteSheetFrameUV_Size_2", Range(2, 16)) = 4
SpriteSheetFrameUV_Frame_2("SpriteSheetFrameUV_Frame_2", Range(0, 15)) = 5
_FillColor_Color_1("_FillColor_Color_1", COLOR) = (0.3176471,0.8980392,0.3098039,1)
_NewTex_2("NewTex_2(RGB)", 2D) = "white" { }
_FillColor_Color_2("_FillColor_Color_2", COLOR) = (0.3176471,0.8980392,0.3098039,1)
_NewTex_3("NewTex_3(RGB)", 2D) = "white" { }
_NewTex_1("NewTex_1(RGB)", 2D) = "white" { }
_OperationBlend_Fade_3("_OperationBlend_Fade_3", Range(0, 1)) = 1
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
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Back 

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
float4 _FillColor_Color_3;
float SpriteSheetFrameUV_Size_2;
float SpriteSheetFrameUV_Frame_2;
float4 _FillColor_Color_1;
sampler2D _NewTex_2;
float4 _FillColor_Color_2;
sampler2D _NewTex_3;
sampler2D _NewTex_1;
float _OperationBlend_Fade_3;

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
float4 OperationBlend(float4 origin, float4 overlay, float blend)
{
float4 o = origin; 
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) * (o.a+0.0000001);
o.a = saturate(o.a);
o = lerp(origin, o, blend);
return o;
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
SourceRGBA_1 = lerp(SourceRGBA_1,SourceRGBA_1*SourceRGBA_1.a + SourceRGBA_2*SourceRGBA_2.a,1);
float4 FillColor_3 = UniColor(SourceRGBA_1,_FillColor_Color_3);
float2 ResizeUV_3 = ResizeUVClamp(i.texcoord,-0.3,-0.3,1.6,1.6);
float2 SpriteSheetFrameUV_2 = SpriteSheetFrame(ResizeUV_3,SpriteSheetFrameUV_Size_2,SpriteSheetFrameUV_Frame_2);
float4 SourceRGBA_3 = tex2D(_SourceNewTex_1, SpriteSheetFrameUV_2);
float4 FillColor_1 = UniColor(SourceRGBA_3,_FillColor_Color_1);
FillColor_3 = lerp(FillColor_3,FillColor_3*FillColor_3.a + FillColor_1*FillColor_1.a,1);
float4 NewTex_2 = tex2D(_NewTex_2, i.texcoord);
float4 FillColor_2 = UniColor(NewTex_2,_FillColor_Color_2);
float4 NewTex_3 = tex2D(_NewTex_3, i.texcoord);
float4 OperationBlend_1 = OperationBlend(NewTex_3, FillColor_2, 1); 
float4 OperationBlend_2 = OperationBlend(OperationBlend_1, FillColor_3, 1); 
float4 NewTex_1 = tex2D(_NewTex_1, i.texcoord);
float4 OperationBlend_3 = OperationBlend(NewTex_1, OperationBlend_2, _OperationBlend_Fade_3); 
OperationBlend_3 = lerp(OperationBlend_3,OperationBlend_3 * float4(0,0,0,1),0.823);
float4 FinalResult = OperationBlend_3;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
