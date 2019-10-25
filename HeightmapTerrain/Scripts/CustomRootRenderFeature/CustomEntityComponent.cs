using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core;
using Xenko.Core.Annotations;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Engine.Design;
using Xenko.Graphics;
using Xenko.Input;
using Xenko.Rendering;

namespace HeightmapTerrain.CustomRootRenderFeature
{
    [DataContract("CustomEntityComponent")]
    [Display("CustomEntityComponent", Expand = ExpandRule.Once)]
    [DefaultEntityComponentRenderer(typeof(CustomEntityProcessor))]
    [ComponentOrder(100)]
    public sealed class CustomEntityComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Create an empty component.
        /// </summary>
        public CustomEntityComponent()
        {
        }

        /// <summary>
        /// Gets or sets the texture
        /// </summary>
        /// <userdoc>The reference to the texture</userdoc>
        [DataMember(10)]
        [Display("Texture")]
        public Texture Texture { get; set; }

        [DataMember(20)]
        [Display("Material")]
        public Material Material;

        /// <summary>
        /// Gets or sets the color
        /// </summary>
        /// <userdoc>The color</userdoc>
        [DataMember(30)]
        [Display("Base Color")]
        public Color4 Color { get; set; } = Color4.White;

        /// <summary>
        /// Gets or sets the scaling of the texture.
        /// </summary>
        /// <value>The texture scale.</value>
        /// <userdoc>The scaling of the texture</userdoc>
        [DataMember(40)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 5.0, 0.01f, 1.0f, 2)]
        public float TextureScale { get; set; } = 1f;

        /// <summary>
        /// The render group for this component.
        /// </summary>
        [DataMember(50)]
        [Display("Render group")]
        [DefaultValue(RenderGroup.Group0)]
        public RenderGroup RenderGroup { get; set; }
    }
}
