using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Input;
using Xenko.Rendering;
using Xenko.Rendering.Compositing;

namespace HeightmapTerrain.Scripts
{
    // TODO: Change to ScriptComponent?
    // Then we can have a constructor that takes in eg. the heightmap then call a load method when ready
    public class Terrain : StartupScript
    {
        // Size of the terrian (in meters)
        private const int SIZE = 64;

        // Vertices per side
        private const int VERTEX_COUNT = 128;

        // Mesh Data
        private VertexPositionNormalColor[] vertices;
        private int[] indices;

        private VertexBufferBinding _vertexBufferBinding;
        private IndexBufferBinding _indexBufferBinding;

        // Things for the model
        private Material _material;
        private Mesh _mesh;
        private ModelComponent _modelComponent;

        /// <summary>
        /// Generates the vertex positions for the terrain
        /// </summary>
        private void GenerateTerrainData()
        {
            // Setup our arrays
            vertices = new VertexPositionNormalColor[VERTEX_COUNT * VERTEX_COUNT];
            indices = new int[6 * (VERTEX_COUNT - 1) * (VERTEX_COUNT - 1)];

            // tmp
            Random random = new Random();

            // Generate the vertex positions
            int vertexPointer = 0;
            for (int x = 0; x < VERTEX_COUNT; x++)
            {
                for (int z = 0; z < VERTEX_COUNT; z++)
                {
                    vertices[vertexPointer].Position = new Vector3(x / ((float)VERTEX_COUNT - 1) * SIZE, 0, z / ((float)VERTEX_COUNT - 1) * SIZE);
                    vertices[vertexPointer].Color = new Color(random.Next(2), random.Next(2), random.Next(2));

                    vertexPointer++;
                }
            }

            // Generate the indices
            int indexPointer = 0;
            for (int x = 0; x < VERTEX_COUNT - 1; x++)
            {
                for (int z = 0; z < VERTEX_COUNT - 1; z++)
                {
                    int topLeft = x + (z + 1) * VERTEX_COUNT;
                    int topRight = x + (z + 1) * VERTEX_COUNT + 1;
                    int bottomLeft = x + z * VERTEX_COUNT;
                    int bottomRight = x + z * VERTEX_COUNT + 1;

                    indices[indexPointer++] = topLeft;
                    indices[indexPointer++] = bottomRight;
                    indices[indexPointer++] = bottomLeft;
                    indices[indexPointer++] = topLeft;
                    indices[indexPointer++] = topRight;
                    indices[indexPointer++] = bottomRight;
                }
            }
        }

        /// <summary>
        /// Calculates the Height of each vertex from the supplied heightmap
        /// </summary>
        private void CalculateHeights()
        {
            // TODO: Calculate from heightmap

            // tmp
            Random random = new Random();
            for (int i = 0; i < vertices.Length; i++)
            {
                // tmp set height values
                float height = random.Next(4, 10); ;

                vertices[i].Position.Y = height;
            }
        }

        /// <summary>
        /// Calculates the normals of each vertex
        /// </summary>
        private void CalculateNormals()
        {
            for (int i = 0; i < indices.Length / 3; i++)
            {
                // Get the indices of the 3 vertices that make up the triangle
                int vertA = indices[i * 3];
                int vertB = indices[i * 3 + 1];
                int vertC = indices[i * 3 + 2];

                // Cross them together to produce the normal
                Vector3 normal = Vector3.Cross(vertices[vertC].Position - vertices[vertA].Position, vertices[vertB].Position - vertices[vertA].Position);
                normal.Normalize();

                // Add the computed normal to the vertices
                vertices[vertA].Normal += normal;
                vertices[vertB].Normal += normal;
                vertices[vertC].Normal += normal;
            }

            // Normalize incase any vector is > 1
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }
        }

        private void CreateMesh()
        {
            var vbo = Xenko.Graphics.Buffer.Vertex.New(
                GraphicsDevice,
                vertices,
                GraphicsResourceUsage.Default
            );

            var ibo = Xenko.Graphics.Buffer.Index.New(
                GraphicsDevice,
                indices,
                GraphicsResourceUsage.Default
            );

            _vertexBufferBinding = new VertexBufferBinding(vbo, VertexPositionNormalColor.Layout, vertices.Length);
            _indexBufferBinding = new IndexBufferBinding(ibo, is32Bit: true, count: indices.Length);

            _mesh = new Mesh()
            {
                Draw = new MeshDraw()
                {
                    PrimitiveType = PrimitiveType.TriangleList,
                    VertexBuffers = new[]
                    {
                        _vertexBufferBinding
                    },
                    IndexBuffer = _indexBufferBinding,
                    DrawCount = indices.Length
                }
            };

            _modelComponent = new ModelComponent()
            {
                Model = new Model()
                {
                    _mesh,
                    _material
                }
            };

            Entity.Add(_modelComponent);
        }

        public override void Start()
        {
            this._material = Content.Load<Material>("TerrainMat");

            GenerateTerrainData();
            CalculateHeights();
            CalculateNormals();
            CreateMesh();

            // tmp
            Game.Window.AllowUserResizing = true;
        }
    }
}
