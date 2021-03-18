#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define EDGE_THRESHOLD_MIN 0.0312
#define EDGE_THRESHOLD_MAX 0.125
#define QUALITY(q) ((q) < 5 ? 1.0 : ((q) > 5 ? ((q) < 10 ? 2.0 : ((q) < 11 ? 4.0 : 8.0)) : 1.5))
#define ITERATIONS 12
#define SUBPIXEL_QUALITY 0.75

float2 ScreenSize;

Texture2D _MainTex;
sampler2D _MainTexSampler = sampler_state 
{
	Texture = <_MainTex>;
    Filter = LINEAR;
    AddressU = MIRROR;
    AddressV = MIRROR;
};

float2 getInverseScreenSize() {
    return float2(1.0 / ScreenSize.x, 1.0 / ScreenSize.y);
}

float2 normalizeUV(float2 inputUV) {
	return inputUV * getInverseScreenSize();
}

float rgb2luma(float3 rgb) {
    return sqrt(dot(rgb, float3(0.299, 0.587, 0.114)));
}

float4 fxaaFragProgram(float2 uv : VPOS) : COLOR {
    // Source: http://blog.simonrodriguez.fr/articles/30-07-2016_implementing_fxaa.html

    // Store original uv and convert/normalize input between [0,1]
    float2 uvInput = uv;
    uv = normalizeUV(uv);
    float2 inverseScreenSize = getInverseScreenSize();

    // Grab the color and luma for the current pixel
    float3 colorCenter = tex2D(_MainTexSampler, uv).rgb;
    float lumaCenter = rgb2luma(colorCenter);

    // Grab the luma for the neighbours
    float lumaDown = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(0, -1))).rgb);
    float lumaUp = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(0, 1))).rgb);
    float lumaLeft = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(-1, 0))).rgb);
    float lumaRight = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(1, 0))).rgb);

    // Calculate the luma range
    float lumaMin = min(lumaCenter, min(min(lumaDown, lumaUp), min(lumaLeft, lumaRight)));
    float lumaMax = max(lumaCenter, max(max(lumaDown, lumaUp), max(lumaLeft, lumaRight)));
    float lumaRange = lumaMax - lumaMin;

    // Check if the difference in luma is within the threshold, if not do not apply the algorithm
    if (lumaRange < max(EDGE_THRESHOLD_MIN, lumaMax * EDGE_THRESHOLD_MAX)) {
        return float4(colorCenter.rgb, 1);
    }

    // Grab the luma for the diagonal neighbours
    float lumaDownLeft = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(-1, -1))).rgb);
    float lumaUpRight = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(1, 1))).rgb);
    float lumaUpLeft = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(-1, 1))).rgb);
    float lumaDownRight = rgb2luma(tex2D(_MainTexSampler, normalizeUV(uvInput + float2(1, -1))).rgb);

    float lumaDownUp = lumaDown + lumaUp;
    float lumaLeftRight = lumaLeft + lumaRight;

    float lumaLeftCorners = lumaDownLeft + lumaUpLeft;
    float lumaDownCorners = lumaDownLeft + lumaDownRight;
    float lumaRightCorners = lumaDownRight + lumaUpRight;
    float lumaUpCorners = lumaUpLeft + lumaUpRight;

    float edgeHorizontal = abs(-2.0 * lumaLeft + lumaLeftCorners) + abs(-2.0  * lumaCenter + lumaDownUp) + abs(-2.0 * lumaRight + lumaRightCorners);
    float edgeVertical = abs(-2.0 * lumaUp + lumaUpCorners) + abs(-2.0  * lumaCenter + lumaLeftRight) + abs(-2.0 * lumaDown + lumaDownCorners);

    bool isHorizontal = edgeHorizontal >= edgeVertical;

    float luma1 = isHorizontal ? lumaDown : lumaLeft;
    float luma2 = isHorizontal ? lumaUp : lumaRight;
    float gradient1 = luma1 - lumaCenter;
    float gradient2 = luma2 - lumaCenter;

    bool is1Steepest = abs(gradient1) >= abs(gradient2);

    float gradientScaled = 0.25 * max(abs(gradient1), abs(gradient2));

    float stepLength = isHorizontal ? inverseScreenSize.y : inverseScreenSize.x;

    float lumaLocalAverage = 0.0;

    if (is1Steepest) {
        stepLength = -stepLength;
        lumaLocalAverage = .5 * (luma1 + lumaCenter);
    }
    else {
        lumaLocalAverage = .5 * (luma2 + lumaCenter);
    }

    float2 currentUv = uv;
    if (isHorizontal) {
        currentUv.y += stepLength * 0.5;
    }
    else {
        currentUv.x += stepLength * 0.5;
    }

    float2 offset = isHorizontal ? float2(inverseScreenSize.x, 0.0) : float2(0.0, inverseScreenSize.y);

    float2 uv1 = currentUv - offset * QUALITY(0);
    float2 uv2 = currentUv + offset * QUALITY(0);

    float lumaEnd1 = rgb2luma(tex2D(_MainTexSampler, uv1).rgb);
    float lumaEnd2 = rgb2luma(tex2D(_MainTexSampler, uv2).rgb);
    lumaEnd1 -= lumaLocalAverage;
    lumaEnd2 -= lumaLocalAverage;

    bool reached1 = abs(lumaEnd1) >= gradientScaled;
    bool reached2 = abs(lumaEnd2) >= gradientScaled;
    bool reachedBoth = reached1 && reached2;

    if (!reached1) {
        uv1 -= offset * QUALITY(1);
    }
    if (!reached2) {
        uv2 += offset * QUALITY(1);
    }

    if (!reachedBoth) {
        for (int i = 2; i < ITERATIONS; i++) {
            if (!reached1) {
                lumaEnd1 = rgb2luma(tex2Dlod(_MainTexSampler, float4(uv1, 0.0, 0.0)).rgb);
                lumaEnd1 = lumaEnd1 - lumaLocalAverage;
            }
            if (!reached2) {
                lumaEnd2 = rgb2luma(tex2Dlod(_MainTexSampler, float4(uv2, 0.0, 0.0)).rgb);
                lumaEnd2 = lumaEnd2 - lumaLocalAverage;
            }

            reached1 = abs(lumaEnd1) >= gradientScaled;
            reached2 = abs(lumaEnd2) >= gradientScaled;
            reachedBoth = reached1 && reached2;

            if (!reached1) {
                uv1 -= offset * QUALITY(i);
            }
            if (!reached2) {
                uv2 += offset * QUALITY(i);
            }

            if (reachedBoth) {
                break;
            }
        }
    }

    float distance1 = isHorizontal ? (uv.x - uv1.x) : (uv.y - uv1.y);
    float distance2 = isHorizontal ? (uv2.x - uv.x) : (uv2.y - uv.y);

    bool isDirection1 = distance1 < distance2;
    float distanceFinal = min(distance1, distance2);

    float edgeThickness = (distance1 + distance2);

    float pixelOffset = -distanceFinal / edgeThickness + 0.5;

    bool isLumaCenterSmaller = lumaCenter < lumaLocalAverage;

	bool correctVariation1 = (lumaEnd1 < 0.0) != isLumaCenterSmaller;
	bool correctVariation2 = (lumaEnd2 < 0.0) != isLumaCenterSmaller;
	
	bool correctVariation = isDirection1 ? correctVariation1 : correctVariation2;

    float finalOffset = correctVariation ? pixelOffset : 0.0;

    // Subpixel AA
    float lumaAverage = (1.0/12.0) * (2.0 * (lumaDownUp + lumaLeftRight) + lumaLeftCorners + lumaRightCorners);
	float subpixelOffset1 = clamp(abs(lumaAverage - lumaCenter)/lumaRange,0.0,1.0);
	float subpixelOffset2 = (-2.0 * subpixelOffset1 + 3.0) * subpixelOffset1 * subpixelOffset1;
	float subpixelOffsetFinal = subpixelOffset2 * subpixelOffset2 * SUBPIXEL_QUALITY;

    finalOffset = max(finalOffset, subpixelOffsetFinal);

    float2 finalUv = uv;
    if (isHorizontal) {
        finalUv.y += finalOffset * stepLength;
    }
    else {
        finalUv.x += finalOffset * stepLength;
    }

    return tex2D(_MainTexSampler, finalUv);
}

technique FXAA {
    pass P0 {
        PixelShader = compile PS_SHADERMODEL fxaaFragProgram();
    }
};