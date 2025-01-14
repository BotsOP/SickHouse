using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileRenderSettings
{
    public Material material;
    public TextureWtihReference[] textures;
    public Mesh mesh;
}

[Serializable]
public struct TextureWtihReference
{
    public string textureName;
    public Texture2D texture;
}