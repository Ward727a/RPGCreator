#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif
// RoundedRect.fx
 float2 Size;
 float Radius;
 float Thickness; 
 float4 OutlineColor; 
 
 sampler TextureSampler : register(s0);
 
 float sdRoundedBox(float2 p, float2 b, float r) {
     float2 q = abs(p) - b + r;
     return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
 }
 
 float4 MainPS(float4 pos : SV_Position, float4 color : COLOR0, float2 uv : TEXCOORD0) : SV_Target {
     float2 p = (uv - 0.5) * Size;
     float2 halfSize = Size * 0.5;
     
     // Calcul de la distance SDF
     float dist = sdRoundedBox(p, halfSize, Radius);
     
     float smoothing = 1.5;
     
     float fillAlpha = 1.0 - smoothstep(-smoothing, 0.0, dist + Thickness);
     
     float outlineMask = 1.0 - smoothstep(-smoothing, 0.0, dist);
     float innerMask = 1.0 - smoothstep(-smoothing, 0.0, dist + Thickness);
     float finalOutlineAlpha = outlineMask - innerMask;
 
     float4 finalColor = lerp(OutlineColor, color, fillAlpha);
     
     finalColor.a = outlineMask * color.a;
 
     return finalColor;
 }
 
 technique RoundedRect {
     pass P0 {
         PixelShader = compile PS_SHADERMODEL MainPS();
     }
 };