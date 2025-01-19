using System.Collections.Generic;
using UnityEngine;

public static class DVPerfomanceConfigs
{
    public static long MemoryLimit
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return 1 * DVConfigs.GB;
#else
            return 500 * DVConfigs.MB;
#endif
        }
    }

    public static int AnimationFrame
    {
        get
        {
            // 기기의 Frame에 맞게 조정
            // 2의 배수로
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return 30 * 2;
#else
            return 7 * 2;
#endif
        }
    }

    public static long EstimateGameObjectMemory(GameObject go, bool instMat)
    {
        long memorySize = 0;

        if(instMat) 
            memorySize += GetMaterialMemory(go);

        foreach (var child in go.GetComponentsInChildren<Transform>(true)) {
            
            memorySize += GetGameObjectMemory(child.gameObject);
        }

        return memorySize;
    }

    private static long GetMaterialMemory(GameObject go) {    
        HashSet<Material> sharedMaterials = new HashSet<Material>();

        foreach (var child in go.GetComponentsInChildren<Transform>(true)) {
            if (go.TryGetComponent(out MeshRenderer renderer))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    sharedMaterials.Add(material);
                }
            }
        }

        // TODO: Material 별 용량 정의 하거나 평균값으로
        const long estMaterialMemory = 3 * DVConfigs.KB; // 추정치

        return sharedMaterials.Count * estMaterialMemory;
    }

    private static long GetGameObjectMemory(GameObject go) {
        long memorySize = 0;

        // Base GameObject Memory
        {
            memorySize += string.IsNullOrEmpty(go.name) ? 0 : go.name.Length * sizeof(char); // Name
            memorySize += string.IsNullOrEmpty(go.tag) ? 0 : go.tag.Length * sizeof(char); // Tag
            memorySize += sizeof(int); // Layer
            memorySize += 64; // Base Memory
        }

        foreach (var component in go.GetComponents<Component>()) {
            if (component is Transform)
                memorySize += 32; // 추정치
            else if (component is MonoBehaviour script)
            {
                memorySize += 64; // 추정치
            }
            else if (component is Collider || component is Renderer)
                memorySize += 128; // 추정치
            else
                memorySize += 16;
        }

        return memorySize;
    }

}
