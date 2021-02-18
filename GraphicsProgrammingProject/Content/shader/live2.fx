#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External Properties
float4x4 World, View, Projection;

float3 CameraPosition;
float3 LightPosition;
float Ambient;

Texture2D MainTex;
sampler2D MainTextureSampler = sampler_state
{
    Texture = <MainTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D NormalTex;
sampler2D NormalTextureSampler = sampler_state
{
    Texture = <NormalTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

Texture2D SpecularTex;
sampler2D SpecularTextureSampler = sampler_state
{
    Texture = <SpecularTex>;
    MipFilter = POINT;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

// Getting out vertex data from vertex shader to pixel shader
struct VertexShaderOutput 
{
    float4 position     : SV_POSITION;
    float4 color        : COLOR0;
    float2 uv           : TEXCOORD0;
    float3 worldPos     : TEXCOORD1;
    float3 worldNormal  : TEXCOORD2;
};

// Vertex shader, receives values directly from semantic channels
VertexShaderOutput MainVS(float4 position : POSITION, float4 color : COLOR0, float2 uv : TEXCOORD, float3 normal : NORMAL)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.color = color;
    output.uv = uv;
    output.position = mul(mul(mul(position, World), View), Projection);
    output.worldPos = mul(position, World);
    output.worldNormal = mul(normal, World);

    return output;
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texColor = tex2D(MainTextureSampler, input.uv);
    float4 normalColor = tex2D(NormalTextureSampler, input.uv);

    float3 perturbedNormal = input.worldNormal;
    perturbedNormal.rg += (normalColor.rg * 2 - 1);
    perturbedNormal = normalize(perturbedNormal);
    
    float3 lightDirection = normalize(input.worldPos - LightPosition);

    float light = max(dot(perturbedNormal, -lightDirection), Ambient);

    return float4(light * texColor.rgb, 1);
}

technique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};