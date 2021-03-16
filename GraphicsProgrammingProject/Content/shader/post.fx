#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 ScreenSize;

Texture2D _MainTex;
sampler2D _MainTexSampler = sampler_state 
{
	Texture = <_MainTex>;
};

float2 convertUV(float2 inputUV) {
	return inputUV * float2(1.0 / ScreenSize.x, 1.0 / ScreenSize.y);
}

float4 InvertPS(float2 uv : VPOS) : COLOR
{
	uv = convertUV(uv);
	float4 color = tex2D(_MainTexSampler, uv);

	return 1 - color;
}

technique Invert
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL InvertPS();
	}
};

float4 ChromaticAberrationPS(float2 uv : VPOS) : COLOR
{
	uv = convertUV(uv);

	float strength = 1;
	float3 rgbOffset = 1 + float3(0.01, 0.005, 0) * strength;
	float dist = distance(uv, float2(0.5, 0.5));
	float2 dir = uv - float2(0.5, 0.5);

	// scale rgb offset & renormalize
	rgbOffset = normalize(rgbOffset * dist);

	// Calculate uvs for each color channel
	float2 uvR = float2(0.5, 0.5) + rgbOffset.r * dir;
	float2 uvG = float2(0.5, 0.5) + rgbOffset.g * dir;
	float2 uvB = float2(0.5, 0.5) + rgbOffset.b * dir;

	float4 colorR = tex2D(_MainTexSampler, uvR);
	float4 colorG = tex2D(_MainTexSampler, uvG);
	float4 colorB = tex2D(_MainTexSampler, uvB);

	return float4(colorR.r, colorG.g, colorB.b, 1);
}

technique CA
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL ChromaticAberrationPS();
	}
};

float4 BlurPS(float2 uv : VPOS) : COLOR
{
    float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    float Quality = 10.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
    float Size = 40.0; // BLUR SIZE (Radius)
    // GAUSSIAN BLUR SETTINGS }}}

	float2 Radius = Size / float2(1280.0, 720.0);

    // Pixel colour
	uv = convertUV(uv);
    float4 Color = tex2D(_MainTexSampler, uv);
    
    // Blur calculations
    for( float d = 0.0; d < Pi; d += Pi/Directions)
    {
		for(float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
        {
			Color += tex2D( _MainTexSampler, uv + float2(cos(d), sin(d)) * Radius * i);		
        }
    }
    
    // Output to screen
    Color /= Quality * Directions - 16.0;
    return Color;
}

technique GaussianBlur 
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL BlurPS();
	}
};