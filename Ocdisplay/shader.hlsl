cbuffer MatrixBuffer : register(b0)
{
    matrix transform;
};

struct VS_Input
{
    float3 pos : POSITION;
    float4 col : COLOR;
};

struct PS_Input
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
};

PS_Input VS(VS_Input input)
{
    PS_Input output;
    output.pos = mul(float4(input.pos, 1.0), transform); // Apply transformation matrix
    output.col = input.col;
    return output;
}

float4 PS(PS_Input input) : SV_Target
{
    return input.col; // Output color directly
}
