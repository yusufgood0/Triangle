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
using SlimeGame.GameAssets.Enemies;

namespace SlimeGame.Drawing
{
    internal class WorldDraw
    {

        public BasicEffect BasicEffect;
        VertexPositionTexture[] TextureVertices;
        static readonly short[] TwoSidedQuadIndices = new short[] { 0, 1, 2, 2, 3, 0 };
        static readonly Vector2 TL = new Vector2(0, 0); // TL
        static readonly Vector2 TR = new Vector2(1, 0); // TR
        static readonly Vector2 BR = new Vector2(1, 1); // BR
        static readonly Vector2 BL = new Vector2(0, 1); // BL

        public Matrix World { get => BasicEffect.World; set => BasicEffect.World = value; }
        public Matrix View { get => BasicEffect.View; set => BasicEffect.View = value; }
        public Matrix Projection { get => BasicEffect.Projection; set => BasicEffect.Projection = value; }
        public Texture2D Texture { get => BasicEffect.Texture; set => BasicEffect.Texture = value; }
        public WorldDraw(GraphicsDevice graphicsDevice, bool doubleSided, float? daylightTime = null, Texture2D texture = null, Matrix? world = null, Matrix? view = null, Matrix? projection = null)
        {
            BasicEffect = new BasicEffect(graphicsDevice)
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

            float DayLightTime1 = daylightTime + 0.0f ?? 0.3f;
            float DayLightTime2 = daylightTime + 0.33f ?? 0.3f;
            float DayLightTime3 = daylightTime + 0.66f ?? 0.3f;

            BasicEffect.VertexColorEnabled = true;
            BasicEffect.FogEnabled = true;
            BasicEffect.FogColor = Color.Black.ToVector3();
            BasicEffect.Alpha = 1.0f;
            BasicEffect.PreferPerPixelLighting = false;
            BasicEffect.LightingEnabled = true;
            BasicEffect.CurrentTechnique = BasicEffect.Techniques[1];
            BasicEffect.AmbientLightColor = Color.Red.ToVector3();
            BasicEffect.EnableDefaultLighting();
            BasicEffect.DirectionalLight0.Enabled = true;
            BasicEffect.DirectionalLight1.Enabled = true;
            BasicEffect.DirectionalLight2.Enabled = true;
            BasicEffect.DirectionalLight0.Direction = SkyColorGenerator.GetSkyDirection(DayLightTime1);
            BasicEffect.DirectionalLight1.Direction = SkyColorGenerator.GetSkyDirection(DayLightTime2);
            BasicEffect.DirectionalLight2.Direction = SkyColorGenerator.GetSkyDirection(DayLightTime3);
            if (TimeSinceLastColorBlinkStarted < secondsPerBlink)
            {
                BasicEffect.DirectionalLight0.DiffuseColor = _blinkColor1;
                BasicEffect.DirectionalLight1.DiffuseColor = _blinkColor2;
                BasicEffect.DirectionalLight2.DiffuseColor = _blinkColor3;
                BasicEffect.FogStart = _blinkFogStart;
                BasicEffect.FogEnd = _blinkFogEnd;
            }
            else
            {
                BasicEffect.DirectionalLight0.DiffuseColor = SkyColorGenerator.GetSkyColor(DayLightTime1).ToVector3();
                BasicEffect.DirectionalLight1.DiffuseColor = SkyColorGenerator.GetSkyColor(DayLightTime2).ToVector3();
                BasicEffect.DirectionalLight2.DiffuseColor = SkyColorGenerator.GetSkyColor(DayLightTime3).ToVector3();
                BasicEffect.FogStart = _baseFogStart;
                BasicEffect.FogEnd = _baseFogEnd;
            }
            BasicEffect.DiffuseColor = Vector3.One;
            BasicEffect.SpecularPower = 1000f;
            BasicEffect.SpecularColor = Vector3.One;
            BasicEffect.EmissiveColor = Vector3.Zero;
        }
        public Vector3 Color1 { get => BasicEffect.DirectionalLight0.DiffuseColor; set => BasicEffect.DirectionalLight0.DiffuseColor = value; }
        public Vector3 Color2 { get => BasicEffect.DirectionalLight1.DiffuseColor; set => BasicEffect.DirectionalLight1.DiffuseColor = value; }
        public Vector3 Color3 { get => BasicEffect.DirectionalLight2.DiffuseColor; set => BasicEffect.DirectionalLight2.DiffuseColor = value; }
        static DateTime _timeWhenColorBlinkStarted = DateTime.MinValue;
        static float TimeSinceLastColorBlinkStarted => (float)(Game1.PlayingGameTime - _timeWhenColorBlinkStarted).TotalSeconds;
        const float secondsPerBlink = 0.4f;
        static bool IsBlinking = TimeSinceLastColorBlinkStarted < secondsPerBlink;
        static Vector3 BaseColor1 => Color.White.ToVector3() * lightStrength;
        static Vector3 BaseColor2 => Color.PaleGreen.ToVector3() * lightStrength;
        static Vector3 BaseColor3 => Color.LightSeaGreen.ToVector3() * lightStrength;
        static Vector3 _blinkColor1 = new Vector3(-1, -1, -1);
        static Vector3 _blinkColor2 = new Vector3(-1, -1, -1);
        static Vector3 _blinkColor3 = new Vector3(-1, -1, -1);
        const int _baseFogStart = 100;
        const int _baseFogEnd = 10000;
        const int _blinkFogStart = 10;
        const int _blinkFogEnd = 1000;
        const float lightStrength = 0.05f;

        public static void BlinkColors(Vector3 targetColor1, Vector3 targetColor2, Vector3 targetColor3)
        {
            _timeWhenColorBlinkStarted = Game1.PlayingGameTime;

            _blinkColor1 = targetColor1;
            _blinkColor2 = targetColor2;
            _blinkColor3 = targetColor3;

        }
        public void SetWorld(Matrix world)
        {
            BasicEffect.World = world;
        }
        public void SetView(Matrix view)
        {
            BasicEffect.View = view;
        }
        public void SetProjection(Matrix projection)
        {
            BasicEffect.Projection = projection;
        }
        public void SetTexture(Texture2D texture)
        {
            BasicEffect.Texture = texture;
            BasicEffect.TextureEnabled = true;
        }
        public void DisableTexture()
        {
            BasicEffect.TextureEnabled = false;
        }
        public void EnableDefaultLighting()
        {
            BasicEffect.LightingEnabled = true;
        }
        public void DisableDefaultLighting()
        {
            BasicEffect.LightingEnabled = false;
        }
        public void EnableTexture()
        {
            BasicEffect.TextureEnabled = true;
        }
        public void DrawQuad(GraphicsDevice graphicsDevice, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Texture2D texture)
        {
            SetTexture(texture);

            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    new VertexPositionTexture[]
                    {
                    new VertexPositionTexture(p1, TL), // TL
                    new VertexPositionTexture(p2, TR), // TR
                    new VertexPositionTexture(p3, BR), // BR
                    new VertexPositionTexture(p4, BL), // BL
                    },
                    0,
                    4,
                    TwoSidedQuadIndices,
                    0,
                    2
                );
            }
        }
        public void DrawQuad(GraphicsDevice graphicsDevice, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color, bool useLight = false)
        {
            DisableTexture();
            BasicEffect.LightingEnabled = useLight;
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    new VertexPositionColor[]
                    {
                    new VertexPositionColor(p1, color), // TL
                    new VertexPositionColor(p2, color), // TR
                    new VertexPositionColor(p3, color), // BR
                    new VertexPositionColor(p4, color), // BL
                    },
                    0,
                    4,
                    TwoSidedQuadIndices,
                    0,
                    2
                );
            }
            BasicEffect.LightingEnabled = true;
        }
        public void DrawTriangle(GraphicsDevice graphicsDevice, Vector3 p1, Vector3 p2, Vector3 p3, Color color, bool useLight = false)
        {
            DisableTexture();
            BasicEffect.LightingEnabled = useLight;
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    new VertexPositionColor[]
                    {
                    new VertexPositionColor(p1, color), // TL
                    new VertexPositionColor(p2, color), // TR
                    new VertexPositionColor(p3, color), // BR
                    },
                    0,
                    3,
                    TwoSidedQuadIndices,
                    0,
                    1
                );
            }
            BasicEffect.LightingEnabled = true;
        }
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, Vector3[] vertices, Vector3[] Normals, Color[] colors)
        {
            DisableTexture();

            VertexPositionColorNormal[] vertexPositions = new VertexPositionColorNormal[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexPositions[i] = new VertexPositionColorNormal(vertices[i], colors[i], Normals[i]);
            }

            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertexPositions,
                    0,
                    vertexPositions.Count(),
                    indices,
                    0,
                    indices.Count() / 3
                );
            }
        }
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, VertexPositionColorNormalTexture[] vertexPositions, int vertexOffset, int numVertices)
        {
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
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
        public void DrawMesh(GraphicsDevice graphicsDevice, int[] indices, VertexPositionColorNormal[] vertexPositions, int vertexOffset, int numVertices)
        {
            DisableTexture();
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
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
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
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
        public void DrawModel(GraphicsDevice GraphicsDevice, Model Model) { DrawModel(GraphicsDevice, Model, Matrix.Identity); }
        public void DrawModel(GraphicsDevice GraphicsDevice, Model Model, Matrix WorldMatrix)
        {
            Matrix storedWorld = BasicEffect.World;
            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = BasicEffect;
                }
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = WorldMatrix * BasicEffect.World;
                }
                mesh.Draw();
            }
            BasicEffect.World = storedWorld;
        }
        public void DrawPositionMarker(
                    GraphicsDevice GraphicsDevice,
                    Vector3 pos,
                    float size,
                    Color color
                    )
        {
            short[] indeces = new short[]
            {
                0,1,2, 2,3,0, // Top
                4,5,6, 6,7,4, // Bottom
                0,1,5, 5,4,0, // Front
                2,3,7, 7,6,2, // Back
                1,2,6, 6,5,1, // Right
                3,0,4, 4,7,3  // Left
            };
            float HalfSize = size / 2f;

            Vector3[] vertices = new Vector3[8]
            {
                // Front face (Z -)
                pos + new Vector3(-HalfSize, -HalfSize, -HalfSize), // front-bottom-left
                pos + new Vector3(HalfSize, -HalfSize, -HalfSize), // front-bottom-right
                pos + new Vector3(HalfSize, HalfSize, -HalfSize), // front-top-right
                pos + new Vector3(-HalfSize, HalfSize, -HalfSize), // front-top-left

                // Back face (Z +)
                pos + new Vector3(-HalfSize, -HalfSize, HalfSize), // back-bottom-left
                pos + new Vector3(HalfSize, -HalfSize, HalfSize), // back-bottom-right
                pos + new Vector3(HalfSize, HalfSize, HalfSize), // back-top-right
                pos + new Vector3(-HalfSize, HalfSize, HalfSize), // back-top-left
            };

            DisableTexture();
            DisableDefaultLighting();
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    Array.ConvertAll(vertices, i => new VertexPositionColor(i, color)),
                    0,
                    vertices.Length,
                    indeces,
                    0,
                    indeces.Length / 3
                );
            }
            EnableDefaultLighting();

        }
        public void DrawPositionMarker(
                    GraphicsDevice GraphicsDevice,
                    Vector3 min,
                    Vector3 max,
                    Color color
                    )
        {
            short[] indeces = new short[]
            {
                0,1,2, 2,3,0, // Top
                4,5,6, 6,7,4, // Bottom
                0,1,5, 5,4,0, // Front
                2,3,7, 7,6,2, // Back
                1,2,6, 6,5,1, // Right
                3,0,4, 4,7,3  // Left
            };

            Vector3[] vertices = new Vector3[8]
            {
                // Front face (Z -)
                new Vector3(min.X, min.Y, min.Z), // front-bottom-left
                new Vector3(max.X, min.Y, min.Z), // front-bottom-right
                new Vector3(max.X, max.Y, min.Z), // front-top-right
                new Vector3(min.X, max.Y, min.Z), // front-top-left

                // Back face (Z +)
                new Vector3(min.X, min.Y, max.Z), // back-bottom-left
                new Vector3(max.X, min.Y, max.Z), // back-bottom-right
                new Vector3(max.X, max.Y, max.Z), // back-top-right
                new Vector3(min.X, max.Y, max.Z), // back-top-left
            };
            DisableTexture();
            DisableTexture();
            foreach (var pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    Array.ConvertAll(vertices, i => new VertexPositionColor(i, color)),
                    0,
                    vertices.Length,
                    indeces,
                    0,
                    indeces.Length / 3
                );
            }
        }
    }
}
