using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xenko.Core;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Graphics.GeometricPrimitives;

namespace Xenko.Rendering.ProceduralModels
{
    [DataContract("TerrainProceduralModel")]
    [Display("Terrain")]
    public class TerrainProceduralModel : PrimitiveProceduralModelBase
    {
        public TerrainProceduralModel()
        {
        }

        /// <summary>
        /// Gets or sets the size of the terrain.
        /// </summary>
        /// <value>The size x.</value>
        /// <userdoc>The size of terrain along the X/Z axis</userdoc>
        [DataMember(10)]
        [Display("Size")]
        public Vector2 Size { get; set; } = new Vector2(1.0f);

        /// <summary>
        /// Gets or Sets the heightmap texture
        /// </summary>
        /// <userdoc>The heightmap of the terrain</userdoc>
        [DataMember(20)]
        [Display("Heightmap")]
        public Texture Heightmap { get; set; }

        /// <summary>
        /// Gets or sets the value indicating if a back face should be added.
        /// </summary>
        /// <userdoc>Should a back face be generated</userdoc>
        [DataMember(30)]
        [DefaultValue(false)]
        [Display("Back Face")]
        public bool GenerateBackFace { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.Terrain.New(Size.X, Size.Y, Heightmap, GenerateBackFace, UvScale.X, UvScale.Y, false);
        }
    }
}
