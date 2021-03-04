#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// External Properties
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightDirection;
float3 ViewDirection;
float3 Ambient;

float3 CameraPosition;
float Time;

Texture2D DirtTex;
sampler2D DirtTextureSampler = sampler_state
{
    Texture = <DirtTex>;
    MipFilter = LINEAR;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
};

TextureCube SkyTex;
samplerCUBE SkyTextureSampler = sampler_state
{
    Texture = <SkyTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};


// Getting out vertex data from vertex shader to pixel shader
struct VertexShaderOutput {
    float4 position : SV_POSITION;
    float4 color : COLOR0;
    float2 tex : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;
    float3 texCube : TEXCOORD3;
    float3 binormal : TEXCOORD4;
    float3 tangent : TEXCOORD5;
};

// Vertex shader, receives values directly from semantic channels
VertexShaderOutput MainVS( float4 position : POSITION, float4 color : COLOR0, float3 normal : NORMAL, float3 binormal : BINORMAL, float3 tangent : TANGENT, float2 tex : TEXCOORD0 )
{
    VertexShaderOutput o = (VertexShaderOutput)0;

    o.position = mul( mul( mul(position, World), View ), Projection );
    o.color = color;
    o.normal = mul(normal, (float3x3)World);
    o.tex = tex;
    o.worldPos = mul(position, World);

    return o;
}

// Pixel Shader, receives input from vertex shader, and outputs to COLOR semantic
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Sample texure
    float3 dirtFar = tex2D(DirtTextureSampler, input.tex / 2).rgb;
    float3 dirt = tex2D(DirtTextureSampler, input.tex * 2).rgb;
    float3 dirtClose = tex2D(DirtTextureSampler, input.tex * 50).rgb;

    float d = distance(input.worldPos, CameraPosition);

    float lerp1 = lerp (dirtClose, dirt, clamp(d / 250, 0, 1));

    float3 texColor = lerp(lerp1, dirtFar, clamp((d - 500) / 500, 0, 1));

    // Lighting calculation
    float3 lighting = max( dot(input.normal, LightDirection), 0.0) + Ambient;

    // Output
    return float4(texColor * lighting, 1);
}

VertexShaderOutput SkyVS(float4 position : POSITION, float3 normal : NORMAL, float2 tex : TEXCOORD0)
{
    VertexShaderOutput o = (VertexShaderOutput)0;

    o.position = mul(mul(mul(position, World), View), Projection);
    o.normal = mul(normal, (float3x3)World);
    o.worldPos = mul(position, World);

    float4 VertexPosition = mul(position, World);
    o.texCube = normalize(VertexPosition - CameraPosition);

    return o;
}

float4 SkyPS(VertexShaderOutput input) : COLOR
{
    float3 topColor = float3(68, 118, 189) / 255.0;
    float3 bottomColor = float3(188, 214, 231) / 255.0;

    float3 viewDirection = normalize(input.worldPos - CameraPosition);

    float sun = pow(max(dot(-viewDirection, LightDirection), 0.0), 512);
    float3 sunColor = float3(255, 200, 50) / 255.0;

    return float4(lerp(bottomColor, topColor, viewDirection.y) + sun * sunColor, 1);
}

technique Terrain
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique SkyBox
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL SkyVS();
        PixelShader = compile PS_SHADERMODEL SkyPS();
    }
};