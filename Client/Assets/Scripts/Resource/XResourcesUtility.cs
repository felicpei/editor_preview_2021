

using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;

public enum ResourceType
{
    unknow,
    first,
    text = first,
    script,
    folder,
    scene,
    prefab,
    model,
    audio,
    material,
    shader,
    spriteatlas,
    font,
    video,
    texture,
    asset,
    bytes,
    last = texture
}

public static class XResourcesUtility
{
    
    
    public static ResourceType GetResourceTypeByPath(string resource)
    {
        var extension = Path.GetExtension(resource);
        if (extension != null)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".unity":
                    return ResourceType.scene;
                case ".prefab":
                    return ResourceType.prefab;
                case ".mat":
                case ".physicmaterial":
                    return ResourceType.material;
                case ".shader":
                case ".cginc":
                case ".shadervariants":
                case ".shadergraph":
                    return ResourceType.shader;
                case ".otf":
                case ".ttf":
                case ".fontsettings":
                    return ResourceType.font;
                case ".spriteatlas":
                    return ResourceType.spriteatlas;
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".bmp":
                case ".png":
                case ".psd":
                case ".tiff":
                case ".iff":
                case ".gif":
                case ".pict":
                case ".exr":
                case ".cubemap":
                case ".tif":
                case ".guiskin":
                case ".tsv":
                case ".mask":
                case ".flare":
                case ".lighting":
                case ".terrainlayer":
                case ".rendertexture":
                    return ResourceType.texture;
                case ".controller":
                case ".anim":
                case ".fbx":
                case ".obj":
                    return ResourceType.model;
                case ".aif":
                case ".wav":
                case ".mp3":
                case ".and":
                case ".ogg":
                    return ResourceType.audio;
                case ".tab":
                case ".cfg":
                case ".lua":
                    return ResourceType.text;
                case ".cs":
                case ".js":
                case ".boo":
                case ".dll":
                    return ResourceType.script;
                case ".mp4":
                    return ResourceType.video;
                case ".bytes":
                    return ResourceType.bytes;
                case ".asset":
                case ".spm":
                case ".sbsar":
                    return ResourceType.asset;
                case "":
                    return ResourceType.folder;
                default:
                    return ResourceType.unknow;
            }
        }
        return ResourceType.unknow;
    }
}
