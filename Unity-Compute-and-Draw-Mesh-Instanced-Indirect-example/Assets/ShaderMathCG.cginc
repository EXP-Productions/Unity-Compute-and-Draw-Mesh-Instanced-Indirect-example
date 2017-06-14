struct Transform
{
	float4 position;
	float4 rotation;
	float4 scale;
};

float4x4 GetScaleMatrix(float3 scale)
{
    return float4x4(scale.x, 0, 0, 0,
        0, scale.y, 0, 0,
        0, 0, scale.z, 0,
        0, 0, 0, 1);
}

float4x4 GetTranslateMatrix(float3 pos)
{
    return float4x4(1, 0, 0, pos.x,
       0, 1, 0, pos.y,
       0, 0, 1, pos.z,
       0, 0, 0, 1);
}


float4x4 GetXRotationMatrix(float rot)
{
    float c = cos(rot);
    float s = sin(rot);
    return float4x4(1, 0, 0, 0,
        0, c, -s, 0,
        0, s, c, 0,
        0, 0, 0, 1);
}

float4x4 GetYRotationMatrix(float rot)
{
    float c = cos(rot);
    float s = sin(rot);
    return float4x4(c, 0, s, 0,
        0, 1, 0, 0,
        -s, 0, c, 0,
        0, 0, 0, 1);
}

float4x4 GetZRotationMatrix(float rot)
{
    float c = cos(rot);
    float s = sin(rot);
    return float4x4(c, -s, 0, 0,
        s, c, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);
}

float4x4 GetRotationMatrix(float3 rot)
{
    float4x4 rotMatrix = GetXRotationMatrix(rot.x);
    rotMatrix = mul(rotMatrix, GetYRotationMatrix(rot.y));
    rotMatrix = mul(rotMatrix, GetZRotationMatrix(rot.z));

    return rotMatrix;
}