using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xenko.Core.Mathematics;
using Xenko.Graphics;

namespace Xenko.Graphics.GeometricPrimitives
{
    public partial class GeometricPrimitive
    {
        /// <summary>
        /// A Terrain Primitive
        /// </summary>
        public static class Terrain
        {
            public static GeometricMeshData<VertexPositionNormalTexture> New(float sizeX = 1.0f, float sizeZ = 1.0f, Texture heightmap = null, bool generateBackFace = false, float uFactor = 1f, float vFactor = 1f, bool toLeftHanded = false)
            {
                bool useRandomHeight = false;
                int vertexCount = 100;
                Color[] heightValues = new Color[vertexCount * vertexCount];
                heightmap.Recreate();

                if (heightmap != null)
                {
                    //throw new Exception();
                    int width = heightmap.CalculateWidth<int>();
                    if (width > 0)
                    {
                        if (heightmap.GraphicsDevice != null)
                        {
                            vertexCount = width;
                            var context = new GraphicsContext(heightmap.GraphicsDevice);
                            heightmap.GetData(context.CommandList, heightValues);
                        }
                    }
                }
                else
                    useRandomHeight = true;

                var vertices = new VertexPositionNormalTexture[vertexCount * vertexCount];
                var indices = new int[6 * (vertexCount - 1) * (vertexCount - 1)];

                // TODO Calculate
                Vector3 normal = Vector3.UnitY;

                var uv = new Vector2(uFactor, vFactor);

                Random rng = new Random();

                // Create vertices
                int vertexPointer = 0;
                for (int x = 0; x < vertexCount; x++)
                {
                    for (int z = 0; z < vertexCount; z++)
                    {
                        float height;
                        if (useRandomHeight)
                            height = rng.Next(2) / 3.1f;
                        else
                            height = heightValues[x + z * vertexCount].R / 3.1f;

                        Vector3 position = new Vector3(x / ((float)vertexCount - 1) * sizeX, height, z / ((float)vertexCount - 1) * sizeZ);
                        var texCoord = new Vector2(uv.X * x / vertexCount, uv.Y * z / vertexCount);

                        vertices[vertexPointer++] = new VertexPositionNormalTexture(position, normal, texCoord);
                    }
                }

                // Create indices
                int indexPointer = 0;
                for (int x = 0; x < vertexCount - 1; x++)
                {
                    for (int z = 0; z < vertexCount - 1; z++)
                    {
                        int topLeft = x + (z + 1) * vertexCount;
                        int topRight = x + (z + 1) * vertexCount + 1;
                        int bottomLeft = x + z * vertexCount;
                        int bottomRight = x + z * vertexCount + 1;

                        indices[indexPointer++] = topLeft;
                        indices[indexPointer++] = bottomRight;
                        indices[indexPointer++] = bottomLeft;

                        indices[indexPointer++] = topLeft;
                        indices[indexPointer++] = topRight;
                        indices[indexPointer++] = bottomRight;
                    }
                }

                // TODO: Generate back faces if needed

                // Create the primitive object.
                return new GeometricMeshData<VertexPositionNormalTexture>(vertices, indices, toLeftHanded) { Name = "Terrain" };
            }
        }
    }
}
