using NotSoWild.Core;
using UnityEditor;
using UnityEngine;

namespace NotSoWild.EditorTools
{
    static class BuildingSpriteSetup
    {
        static readonly (string assetPath, string spritePath)[] Bindings =
        {
            ("Assets/Settings/Buildings/Building_Saloon.asset", "Assets/Sprites/Buildings/saloon.png"),
            ("Assets/Settings/Buildings/Building_GeneralStore.asset", "Assets/Sprites/Buildings/general_store.png"),
            ("Assets/Settings/Buildings/Building_SheriffOffice.asset", "Assets/Sprites/Buildings/sheriff_office.png"),
        };

        [InitializeOnLoadMethod]
        static void AutoFixOnLoad()
        {
            EditorApplication.delayCall += TryFixMissingSprites;
        }

        [MenuItem("Not So Wild/Fix Building Sprites")]
        static void FixFromMenu()
        {
            TryFixMissingSprites();
        }

        static void TryFixMissingSprites()
        {
            if (Application.isPlaying)
            {
                return;
            }

            bool changed = false;
            foreach (var (assetPath, spritePath) in Bindings)
            {
                var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(assetPath);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (definition == null || sprite == null)
                {
                    if (definition == null)
                    {
                        Debug.LogWarning($"BuildingSpriteSetup: missing definition at {assetPath}");
                    }

                    if (sprite == null)
                    {
                        Debug.LogWarning($"BuildingSpriteSetup: missing sprite at {spritePath}");
                    }

                    continue;
                }

                if (definition.Sprite == sprite)
                {
                    continue;
                }

                definition.Sprite = sprite;
                EditorUtility.SetDirty(definition);
                changed = true;
                Debug.Log($"BuildingSpriteSetup: assigned {sprite.name} to {definition.DisplayName}");
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}
