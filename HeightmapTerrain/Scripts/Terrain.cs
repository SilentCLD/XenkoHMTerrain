using HeightmapTerrain.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Input;
using Xenko.Physics;
using Xenko.Rendering;
using Xenko.Rendering.Compositing;

namespace HeightmapTerrain.Scripts
{
    // TODO: Change to ScriptComponent?
    // Then we can have a constructor that takes in eg. the heightmap then call a load method when ready
    public class Terrain : StartupScript
    {
        // Size of the terrian (in meters)
        public const int TERRAIN_SIZE = 514;

        // Min / Max height of the terrain
        public const int MAX_HEIGHT = 100;
        public const int MIN_HEIGHT = 0;

        // How many vertices are in a strip (automatically set during initialize)
        private int _vertexCount = 100;

        // Mesh Data
        private VertexPositionNormalColor[] vertices;
        private int[] indices;

        private VertexBufferBinding _vertexBufferBinding;
        private IndexBufferBinding _indexBufferBinding;

        // Things for the model
        private Material _material;
        private Mesh _mesh;
        private ModelComponent _modelComponent;

        // Things for the collider
        private StaticColliderComponent _colliderComponent;
        private Vector3[] _colVertices;

        /// <summary>
        /// Initializes all global arrays
        /// </summary>
        private void InitializeGlobalArrays()
        {
            // Mesh
            vertices = new VertexPositionNormalColor[_vertexCount * _vertexCount];
            indices = new int[6 * (_vertexCount - 1) * (_vertexCount - 1)];

            // Collider
            _colVertices = new Vector3[_vertexCount * _vertexCount];
        }

        /// <summary>
        /// Generates the vertex positions for the terrain
        /// </summary>
        private void GenerateTerrainData()
        {
            // Generate the vertex positions
            int vertexPointer = 0;
            for (int x = 0; x < _vertexCount; x++)
            {
                for (int z = 0; z < _vertexCount; z++)
                {
                    Vector3 pos = new Vector3(x / ((float)_vertexCount - 1) * TERRAIN_SIZE, vertices[vertexPointer].Position.Y, z / ((float)_vertexCount - 1) * TERRAIN_SIZE);
                    vertices[vertexPointer].Position = pos;

                    // We only need the position of the vertices for the collider so we'll just put it in a new array
                    _colVertices[vertexPointer] = pos;

                    vertexPointer++;
                }
            }

            // Generate the indices
            int indexPointer = 0;
            for (int x = 0; x < _vertexCount - 1; x++)
            {
                for (int z = 0; z < _vertexCount - 1; z++)
                {
                    int topLeft = x + (z + 1) * _vertexCount;
                    int topRight = x + (z + 1) * _vertexCount + 1;
                    int bottomLeft = x + z * _vertexCount;
                    int bottomRight = x + z * _vertexCount + 1;

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
        private void CalculateHeights(Texture heightmap)
        {
            // Setup the array for the height information
            Color[] heightValues = new Color[_vertexCount * _vertexCount];

            // Get the height information and put it in the array
            if (heightmap != null)
                heightmap.GetData(Game.GraphicsContext.CommandList, heightValues);

            // Loop through each vertex and set its height
            int vertexPointer = 0;
            for (int x = 0; x < _vertexCount; x++)
            {
                for (int z = 0; z < _vertexCount; z++)
                {
                    float height = heightValues[x + z * _vertexCount].R / 3.1f;

                    // TODO: Squash the values to be in range instead of just slicing the top off? ¯\_(ツ)_/¯
                    if (height > MAX_HEIGHT)
                        height = MAX_HEIGHT;
                    else if (height < MIN_HEIGHT)
                        height = MIN_HEIGHT;

                    vertices[vertexPointer++].Position.Y = height;
                }
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

        /// <summary>
        /// Calculate the mesh colours based on height.
        /// </summary>
        private void CalculateColours()
        {
            // This is probably a temporary method, will most likely pull in a terrain texture instead of this in the future
            // Or use this as a backup?

            int vertexPointer = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[vertexPointer].Position.Y < 10)
                    vertices[vertexPointer].Color = Color.Blue;
                else if (vertices[vertexPointer].Position.Y < 20)
                    vertices[vertexPointer].Color = Color.Green;
                else if (vertices[vertexPointer].Position.Y < 40)
                    vertices[vertexPointer].Color = Color.SaddleBrown;
                else
                    vertices[vertexPointer].Color = Color.White;

                vertexPointer++;
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

            // Bounding box for culling
            _mesh.BoundingBox = Helpers.FromPoints(vertices);
            _mesh.BoundingSphere = BoundingSphere.FromBox(_mesh.BoundingBox);
        }

        /// <summary>
        /// Creates the physics collider
        /// </summary>
        private void CreateCollider()
        {
            _colliderComponent = new StaticColliderComponent();
            var shape = new StaticMeshColliderShape(_colVertices, indices, Vector3.One);

            _colliderComponent.ColliderShape = shape;

            // Not sure if this is strictly necessary but can save cpu time according to the docs
            _colliderComponent.CanSleep = true;

            Entity.Add(_colliderComponent);
        }

        public override void Start()
        {
            this._material = Content.Load<Material>("Materials/TerrainMat");
            Texture heightMap = Content.Load<Texture>("Textures/Heightmap");

            if (heightMap == null)
                Log.Warning("No heightmap loaded! Using default values");
            else
                _vertexCount = heightMap.Width;

            InitializeGlobalArrays();

            CalculateHeights(heightMap);
            GenerateTerrainData();
            CalculateNormals();
            CalculateColours();

            CreateMesh();
            CreateCollider();

            // tmp
            Game.Window.AllowUserResizing = true;
        }

        /// <summary>
        /// Cleanup on application close or scene change
        /// </summary>
        public override void Cancel()
        {
            _vertexBufferBinding.Buffer?.Dispose();
            _indexBufferBinding.Buffer?.Dispose();
        }
    }
}
