#ifndef SHADER_GRAPH_SUPPORT_H
#define SHADER_GRAPH_SUPPORT_H

StructuredBuffer<float4> gridBuffer;
float gridWidth;
float gridHeight;
float tileSize;

void GetSelectionColor_float(in float3 In, out float4 Out)
{
    int2 gridPos = int2(round(((In.x - tileSize / 2) / tileSize) + (gridWidth * tileSize / 2)), round((In.z / tileSize) + (gridHeight * tileSize / 2)));
    int index = gridPos.x * gridWidth + gridPos.y % gridHeight;
    Out = gridBuffer[index];
    // Out = float3(index, index, index);
}

#endif