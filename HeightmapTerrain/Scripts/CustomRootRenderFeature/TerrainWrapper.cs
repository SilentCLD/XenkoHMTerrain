using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xenko.Core.Mathematics;
using Xenko.Graphics;
using Xenko.Rendering;
//using Buffer = Xenko.Graphics.Buffer;

namespace HeightmapTerrain.Scripts.CustomRootRenderFeature
{
    public class TerrainWrapper : RenderObject
    {
        // Shader properties
        public Color4 Color = Color4.White;
        public Texture Texture;
        public Material Material;
        public float TextureScale = 1;
        public Matrix WorldMatrix = Matrix.Identity;

        // Vertex buffer setup
        public int VertexCount;
        public int IndexCount;
        public static VertexDeclaration VertexDeclaration = VertexPositionNormalColor.Layout;
        public static PrimitiveType PrimitiveType = PrimitiveType.TriangleList;
        public Xenko.Graphics.Buffer VertexBuffer;
        public Xenko.Graphics.Buffer IndexBuffer;

        public void Prepare(RenderDrawContext context)
        {
            if (VertexBuffer != null)
                return;

            //Thread.Sleep(3000);

            Terrain terrain = new Terrain();

            terrain.load(Texture, Material, context);

            Vector2[] texPos = new Vector2[3]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1)
            };

            VertexBuffer = Xenko.Graphics.Buffer.Vertex.New(context.GraphicsDevice, terrain.vertices, GraphicsResourceUsage.Immutable);
            IndexBuffer = Xenko.Graphics.Buffer.Index.New(context.GraphicsDevice, terrain.indices, GraphicsResourceUsage.Immutable);

            //VertexBuffer = terrain._vertexBufferBinding;
            //IndexBuffer = terrain._indexBufferBinding;
            VertexCount = VertexBuffer.ElementCount;
            //IndexCount = IndexBuffer.ElementCount;
        }
    }
}
