using HeightmapTerrain.Scripts;
using HeightmapTerrain.Scripts.CustomRootRenderFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Annotations;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Input;
using Xenko.Rendering;

namespace HeightmapTerrain.CustomRootRenderFeature
{
    public class CustomEntityProcessor : EntityProcessor<CustomEntityComponent, TerrainWrapper>, IEntityComponentRenderProcessor
    {
        public VisibilityGroup VisibilityGroup { get; set; }

        public TerrainWrapper Terrain { get; private set; }

        protected override void OnSystemRemove()
        {
            if (Terrain != null)
            {
                VisibilityGroup.RenderObjects.Remove(Terrain);
                Terrain.VertexBuffer?.Dispose();
                Terrain.IndexBuffer?.Dispose();
                Terrain = null;
            }

            base.OnSystemRemove();
        }

        protected override TerrainWrapper GenerateComponentData([NotNull] Entity entity, [NotNull] CustomEntityComponent component)
        {
            return new TerrainWrapper { RenderGroup = component.RenderGroup };
        }

        protected override bool IsAssociatedDataValid([NotNull] Entity entity, [NotNull] CustomEntityComponent component, [NotNull] TerrainWrapper associatedData)
        {
            return component.RenderGroup == associatedData.RenderGroup;
        }

        public override void Draw(RenderContext context)
        {
            var previousRenderObject = Terrain;
            Terrain = null;

            foreach (var entityKeyPair in ComponentDatas)
            {
                var customEntityComponent = entityKeyPair.Key;
                var terrain = entityKeyPair.Value;

                if (customEntityComponent.Enabled)
                {
                    terrain.Color = customEntityComponent.Color;
                    terrain.Texture = customEntityComponent.Texture;
                    terrain.Material = customEntityComponent.Material;
                    terrain.TextureScale = customEntityComponent.TextureScale;
                    terrain.RenderGroup = customEntityComponent.RenderGroup;
                    terrain.WorldMatrix = customEntityComponent.Entity.Transform.WorldMatrix;
                    Terrain = terrain;
                    break;
                }
            }

            if (Terrain != previousRenderObject)
            {
                if (previousRenderObject != null)
                    VisibilityGroup.RenderObjects.Remove(previousRenderObject);
                if (Terrain != null)
                    VisibilityGroup.RenderObjects.Add(Terrain);
            }
        }
    }
}
