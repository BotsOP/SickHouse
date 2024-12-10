#ifndef SHADER_GRAPH_SUPPORT_H
#define SHADER_GRAPH_SUPPORT_H

StructuredBuffer<int> _GridFloorBuffer;
float gridWidth;
float gridHeight;
float tileSize;

void GetFloorIndex_float(in float3 In, out float2 Out)
{
    int2 gridPos = int2(round((In.x / tileSize) + (gridWidth * tileSize / 2)), round((In.z / tileSize) + (gridHeight * tileSize / 2)));
    int index = gridPos.x * gridWidth + gridPos.y % gridHeight;
    Out = float2(_GridFloorBuffer[index], _GridFloorBuffer[index]);
}

#endif