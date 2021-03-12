#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D _MainTex;
sampler2D _MainTexSampler = sampler_state 
{
	Texture = <_MainTex>;
};

float4 fxaaFragProgram(float2 uv : VPOS) : COLOR {
    return tex2D(_MainTexSampler, uv);
}

technique FXAA {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL fxaaFragProgram();
    }
};