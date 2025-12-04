using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Models;

namespace SlimeGame.GameAsset
{
    internal class WorldDraw
    {

        public BasicEffect basicEffect;
        VertexPositionTexture[] TextureVertices;
        static readonly short[] TwoSidedQuadIndices = new short[] { 0, 1, 2, 2, 3, 0 };
        static readonly Vector2 TL = new Vector2(0, 0); // TL
        static readonly Vector2 TR = new Vector2(1, 0); // TR
        static readonly Vector2 BR = new Vector2(1, 1); // BR
        static readonly Vector2 BL = new Vector2(0, 1); // BL

        public Matrix World { get => basicEffect.World; set => basicEffect.World = value; }
        public Matrix View { get => basicEffect.View; set => basicEffect.View = value; }
        public Matrix Projection { get => basicEffect.Projection; set => basicEffect.Projection = value; }
        public Texture2D Texture { get => basicEffect.Texture; set => basicEffect.Texture = value; }
        public WorldDraw(GraphicsDevice graphicsDevice, bool doubleSided, Texture2D? texture = null, Matrix? world = null, Matrix? view = null, Matrix? projection = null)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = texture != null,
                Texture = texture != null ? texture : null,
                LightingEnabled = false,
            };

            TextureVertices = new VertexPositionTexture[4];
            TextureVertices[0].TextureCoordinate = new Vector2(0, 0); // TL
            TextureVertices[1].TextureCoordinate = new Vector2(1, 0); // TR
            TextureVertices[2].TextureCoordinate = new Vector2(1, 1); // BR
            TextureVertices[3].TextureCoordinate = new Vector2(0, 1); // BL

            World =
                world != null ? (Matrix)world :
                Matrix.CreateRotationY(0f) * Matrix.CreateRotationX(0f);

            View =
                view != null ? (Matrix)view :
                Matrix.CreateLookAt(
                new Vector3(0, 0, 5),  // camera position (looking toward origin)
                Vector3.Zero,          // target
                Vector3.Down             // up vector
            );

            Projection =
                projection != null ? (Matrix)projection :
                Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.PiOver4,
                            graphicsDevice.Viewport.AspectRatio,
                            0.1f,
                            1000f
                            );

            if (doubleSided)
            {
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
            }
            else
            {
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            basicEffect.VertexColorEnabled = true;

            // Depth test on
            // If you want to show both sides:
        }
        public void SetWorld(Matrix world)
        {
            basicEffect.World = world;
        }
        public void SetView(Matrix view)
        {
            basicEffect.View = view;
        }
        public void SetProjection(Matrix projection)
        {
            basicEffect.Projection = projection;
        }
        public void SetTexture(Texture2D texture)
        {
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;
        }
        public void DisableTexture()
        {
            basicEffect.TextureEnabled = false;
        }
        public void EnableTexture()
        {
            basicEffect.TextureEnabled = true;
        }
        public void DrawQuad(GraphicsDevice graphicsDevice, Vector3[] Vertices, Texture2D texture)
        {
            SetTexture(texture);

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleList,
                    new VertexPositionTexture[]
                    {
                    new VertexPositionTexture(Vertices[0], TL), // TL
                    new VertexPositionTexture(Vertices[1], TR), // TR
                    new VertexPositionTexture(Vertices[2], BR), // BR
                    new VertexPositionTexture(Vertices[3], BL), // BL
                    },
                    0,
                    4,
                    TwoSidedQuadIndices,
                    0,
                    2
                );
            }
        }
        public void DrawQuad(GraphicsDevice graphicsDevice, Vector3[] Vertices, Color color)
        {
            DisableTexture();

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    new VertexPositionColor[]
                    {
                    new VertexPositionColor(Vertices[0], color), // TL
                    new VertexPositionColor(Vertices[1], color), // TR
                    new VertexPositionColor(Vertices[2], color), // BR
                    new VertexPositionColor(Vertices[3], color), // BL
                    },
                    0,
                    4,
                    TwoSidedQuadIndices,
                    0,
                    2
                );
            }
        }
        public void DrawTriangle(GraphicsDevice graphicsDevice, Vector3[] Vertices, Color color)
        {
            DisableTexture();

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    new VertexPositionColor[]
                    {
                    new VertexPositionColor(Vertices[0], color), // TL
                    new VertexPositionColor(Vertices[1], color), // TR
                    new VertexPositionColor(Vertices[2], color), // BR
                    },
                    0,
                    3,
                    TwoSidedQuadIndices,
                    0,
                    1
                );
            }
        }
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, Vector3[] vertices, Vector3[] Normals, Color[] colors)
        {
            DisableTexture();

            VertexPositionColorNormal[] vertexPositions = new VertexPositionColorNormal[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexPositions[i] = new VertexPositionColorNormal(vertices[i], colors[i], Normals[i]);
            }

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorNormal>(
                    PrimitiveType.TriangleList,
                    vertexPositions,
                    0,
                    vertexPositions.Count(),
                    indices,
                    0,
                    indices.Count()/3
                );
            }
        }
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, VertexPositionColorNormal[] vertexPositions, int vertexOffset, int numVertices)
        {
            DisableTexture();
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColorNormal>(
                    PrimitiveType.TriangleList,
                    vertexPositions,
                    vertexOffset,
                    numVertices,
                    indices,
                    0,
                    indices.Count() / 3
                );
            }
        }
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, VertexPositionColor[] vertexPositions, int vertexOffset, int numVertices)
        {
            DisableTexture();
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    vertexPositions,
                    vertexOffset,
                    numVertices,
                    indices,
                    0,
                    indices.Count() / 3
                );
            }
        }
    }
}
