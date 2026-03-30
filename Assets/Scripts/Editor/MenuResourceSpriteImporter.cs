#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MenuResourceSpriteImporter : AssetPostprocessor
{
    private const string ResourceSpriteFolder = "Assets/Resources/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ResourceSpriteFolder))
        {
            return;
        }

        if (assetImporter is not TextureImporter textureImporter)
        {
            return;
        }

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        textureImporter.alphaIsTransparency = true;
        textureImporter.mipmapEnabled = false;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.maxTextureSize = 2048;
    }
}
#endif
