using NotSoWild.Core;
using NotSoWild.Visual;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class BuildingManager : MonoBehaviour
    {
        TownBootstrap _bootstrap;
        ResidentManager _residents;
        int _siteVisualSeed;

        public void Initialize(TownBootstrap bootstrap, ResidentManager residents)
        {
            _bootstrap = bootstrap;
            _residents = residents;
        }

        public bool CanAfford(TownState state, BuildingDefinition definition) =>
            state != null && definition != null && state.Gold >= definition.GoldCost;

        public bool CanPlaceAt(TownState state, BuildingDefinition definition, GridCoordinates center)
        {
            if (_bootstrap?.Grid == null || definition == null || state == null)
            {
                return false;
            }

            return _bootstrap.Grid.CanPlaceBuilding(center, definition.Width, definition.Height);
        }

        public bool TryStartConstruction(
            TownState state,
            BuildingDefinition definition,
            GridCoordinates center,
            ResidentRecord builder)
        {
            if (state == null || definition == null || builder == null)
            {
                return false;
            }

            if (!WorkforceHelper.IsIdle(builder) || !WorkforceHelper.CanBuild(builder))
            {
                return false;
            }

            if (!WorkforceHelper.CanStartConstruction(state))
            {
                return false;
            }

            if (!CanAfford(state, definition) || !CanPlaceAt(state, definition, center))
            {
                return false;
            }

            var grid = _bootstrap.Grid;
            var origin = grid.GetFootprintOriginFromCenter(center, definition.Width, definition.Height);
            if (!grid.TryOccupyFootprint(origin, definition.Width, definition.Height))
            {
                return false;
            }

            state.Gold -= definition.GoldCost;

            var site = new ConstructionSite
            {
                Id = state.AllocateConstructionId(),
                Definition = definition,
                Center = center,
                Origin = origin,
                Builder = builder,
                Progress = 0f
            };

            builder.WorkState = ResidentWorkState.Building;
            builder.ConstructionSite = site;
            builder.AssignedBuilding = null;

            site.View = CreateBuildingView(definition, grid, origin, center, scaffold: true);
            state.ConstructionSites.Add(site);
            _residents?.SyncAgentWorkState(builder);

            string builderName = builder.Definition != null ? builder.Definition.DisplayName : "Worker";
            state.AddLog($"{builderName} started building {definition.DisplayName}.");
            return true;
        }

        public void AdvanceConstruction(TownState state, float deltaTime)
        {
            if (state == null || deltaTime <= 0f)
            {
                return;
            }

            for (int i = state.ConstructionSites.Count - 1; i >= 0; i--)
            {
                var site = state.ConstructionSites[i];
                if (site?.Builder == null || site.Definition == null)
                {
                    continue;
                }

                site.Progress += WorkforceHelper.GetBuildSpeedMultiplier(site.Builder) * deltaTime;
                site.View?.ShowConstructionProgress(site.Progress / site.RequiredSeconds);
                if (!site.IsComplete)
                {
                    continue;
                }

                CompleteConstruction(state, site);
            }
        }

        void CompleteConstruction(TownState state, ConstructionSite site)
        {
            if (site.View != null)
            {
                site.View.HideConstructionProgress();
                site.View.Setup(site.Definition, _bootstrap.Grid, site.Origin, site.Center, EnsureSprite(site.Definition));
                site.View.SetStaffed(false);
            }

            var building = new PlacedBuildingRecord
            {
                Definition = site.Definition,
                Center = site.Center,
                Origin = site.Origin,
                View = site.View,
                Worker = null
            };

            state.Buildings.Add(building);
            state.ConstructionSites.Remove(site);

            if (site.Builder != null)
            {
                site.Builder.WorkState = ResidentWorkState.Idle;
                site.Builder.ConstructionSite = null;
                _residents?.SyncAgentWorkState(site.Builder);
            }

            state.AddLog($"{site.Definition.DisplayName} is ready — assign a worker to operate it.");
        }

        public bool TryAssignBuilder(TownState state, ConstructionSite site, ResidentRecord builder)
        {
            if (state == null || site == null || builder == null)
            {
                return false;
            }

            if (!WorkforceHelper.IsIdle(builder) || !WorkforceHelper.CanBuild(builder))
            {
                return false;
            }

            if (site.Builder != null)
            {
                site.Builder.WorkState = ResidentWorkState.Idle;
                site.Builder.ConstructionSite = null;
                _residents?.SyncAgentWorkState(site.Builder);
            }

            site.Builder = builder;
            builder.WorkState = ResidentWorkState.Building;
            builder.ConstructionSite = site;
            builder.AssignedBuilding = null;
            _residents?.SyncAgentWorkState(builder);

            string builderName = builder.Definition != null ? builder.Definition.DisplayName : "Worker";
            string buildingName = site.Definition != null ? site.Definition.DisplayName : "construction";
            state.AddLog($"{builderName} continues building {buildingName}.");
            return true;
        }

        public bool CancelConstruction(TownState state, ConstructionSite site)
        {
            if (state == null || site == null || site.Definition == null || _bootstrap?.Grid == null)
            {
                return false;
            }

            if (site.Builder != null)
            {
                site.Builder.WorkState = ResidentWorkState.Idle;
                site.Builder.ConstructionSite = null;
                _residents?.SyncAgentWorkState(site.Builder);
            }

            _bootstrap.Grid.ReleaseFootprint(site.Origin, site.Definition.Width, site.Definition.Height);
            if (site.View != null)
            {
                Destroy(site.View.gameObject);
            }

            int refund = Mathf.FloorToInt(site.Definition.GoldCost * 0.5f);
            state.Gold += refund;
            state.ConstructionSites.Remove(site);
            state.AddLog($"{site.Definition.DisplayName} construction cancelled. Refunded {refund} gold.");
            return true;
        }

        BuildingView CreateBuildingView(
            BuildingDefinition definition,
            TownGrid grid,
            GridCoordinates origin,
            GridCoordinates center,
            bool scaffold)
        {
            var buildingObject = new GameObject(definition.DisplayName);
            buildingObject.transform.SetParent(transform, false);
            var view = buildingObject.AddComponent<BuildingView>();
            Sprite sprite = scaffold ? GetScaffoldSprite(definition) : EnsureSprite(definition);
            view.Setup(definition, grid, origin, center, sprite);
            if (scaffold)
            {
                view.SetScaffold(true);
                view.ShowConstructionProgress(0f);
            }
            else
            {
                view.SetStaffed(false);
            }
            return view;
        }

        public Sprite GetSpriteForDefinition(BuildingDefinition definition) => EnsureSprite(definition);

        public BuildingView CreatePreview(
            BuildingDefinition definition,
            TownGrid grid,
            GridCoordinates center,
            Transform parent)
        {
            var origin = grid.GetFootprintOriginFromCenter(center, definition.Width, definition.Height);
            var buildingObject = new GameObject($"{definition.DisplayName} Preview");
            buildingObject.transform.SetParent(parent, false);
            var view = buildingObject.AddComponent<BuildingView>();
            view.Setup(definition, grid, origin, center, EnsureSprite(definition));
            view.SetPreview(true);
            return view;
        }

        Sprite EnsureSprite(BuildingDefinition definition)
        {
            if (definition?.Sprite != null)
            {
                return definition.Sprite;
            }

            var fallback = TryLoadSpriteFallback(definition);
            if (fallback != null)
            {
                return fallback;
            }

            _siteVisualSeed++;
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var color = PreviewColor(_siteVisualSeed);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        }

        static System.Collections.Generic.Dictionary<string, Sprite> _fallbackSprites;
        static System.Collections.Generic.Dictionary<string, Sprite> _scaffoldSprites;

        static Sprite TryLoadSpriteFallback(BuildingDefinition definition)
        {
            EnsureFallbackCatalog();
            if (_fallbackSprites == null)
            {
                return null;
            }

            string key = MapResourceSpriteName(definition);
            return key != null && _fallbackSprites.TryGetValue(key, out var sprite) ? sprite : null;
        }

        static void EnsureFallbackCatalog()
        {
            if (_fallbackSprites != null)
            {
                return;
            }

            _fallbackSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
            foreach (var sprite in Resources.LoadAll<Sprite>("NotSoWild/BuildingSprites"))
            {
                if (sprite != null)
                {
                    _fallbackSprites[sprite.name] = sprite;
                }
            }
        }

        static string MapResourceSpriteName(BuildingDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            return definition.name switch
            {
                "Building_Saloon" => "saloon",
                "Building_GeneralStore" => "general_store",
                "Building_SheriffOffice" => "sheriff_office",
                "Building_Armory" => "armory",
                "Building_Hospital" => "hospital",
                "Building_Church" => "church",
                "Building_ResidentialHouse" => "residential_house",
                "Building_ProspectorHut" => "prospector_hut",
                _ => null
            };
        }

        static Sprite GetScaffoldSprite(BuildingDefinition definition)
        {
            EnsureScaffoldCatalog();
            if (_scaffoldSprites == null || definition == null)
            {
                return EnsureSpriteFallback(definition);
            }

            string key = $"scaffold_{definition.Width}x{definition.Height}";
            return _scaffoldSprites.TryGetValue(key, out var sprite)
                ? sprite
                : EnsureSpriteFallback(definition);
        }

        static void EnsureScaffoldCatalog()
        {
            if (_scaffoldSprites != null)
            {
                return;
            }

            _scaffoldSprites = new System.Collections.Generic.Dictionary<string, Sprite>();
            foreach (var sprite in Resources.LoadAll<Sprite>("NotSoWild/Scaffolds"))
            {
                if (sprite != null)
                {
                    _scaffoldSprites[sprite.name] = sprite;
                }
            }
        }

        static Sprite EnsureSpriteFallback(BuildingDefinition definition)
        {
            if (definition?.Sprite != null)
            {
                return definition.Sprite;
            }

            return TryLoadSpriteFallback(definition);
        }

        static Color PreviewColor(int seed)
        {
            Random.InitState(seed * 92821);
            return new Color(0.45f + Random.value * 0.2f, 0.35f + Random.value * 0.2f, 0.25f, 1f);
        }
    }
}
