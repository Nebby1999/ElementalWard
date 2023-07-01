void FragCalcs_half(float2 uv, float _Rows, float _Cols, float4 customData1, float4 customData2, out float2 Out)
{
    uint ind = floor(uv.x * _Cols);
    uint x = 0;
    uint y = 0;
    uint dataInd = ind / 3;
    uint sum = dataInd < 4 ? customData1[dataInd] : customData2[dataInd - 4];

    for (int i = 0; i < 3; ++i)
    {
        if (dataInd > 3 & i == 3)
            break;
        uint val = ceil(pow(10, 5 - i * 2));
        x = sum / val;
        sum -= x * val;
        val = ceil(pow(10, 4 - i * 2));
        y = sum / val;
        sum -= floor(y * val);
        if (dataInd * 3 + i == ind)
            i = 3;
    }

    float cols = 1.0 / _Cols;
    float rows = 1.0 / _Rows;
    uv.x += x * cols - ind * rows;
    uv.y += y * rows;
    Out = uv;
}