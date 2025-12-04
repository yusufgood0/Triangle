using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SlimeGame.Models;
using SlimeGame.Models.Shapes;
namespace SlimeGame.GameAsset
{
    internal struct CrystalBall(Vector3 orbOffset)
    {
        static int SphereQuality = 40;
        static Sphere _model = new Sphere(Vector3.Zero, 30, SphereQuality, Color.White);
        static float _swirlSpeed = 0.1f;
        static Color BackGroundColor = Color.Teal;

        float _colorValue;
        Vector3 _position;
        float _rotation;
        
        public void Draw(SpellBook spellBook, WorldDraw worldDraw, GraphicsDevice graphicsDevice)
        {
            var VerticesAndIndeces = GetVerticesAndIndeces(spellBook);
            worldDraw.DrawMesh(
                graphicsDevice,
                VerticesAndIndeces.Indeces,
                VerticesAndIndeces.Vertices,
                0,
                VerticesAndIndeces.Vertices.Count()
                );
        }
        public void SetPosition(Player player)
        {
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(player.Angle.X, -player.Angle.Y, 0);
            _position = Vector3.Transform(orbOffset, rotation) + player.EyePos;
        }
        public void UpdateHighlights(SpellBook spellbook, Player player)
        {
            _rotation += _swirlSpeed;
            _rotation %= MathF.Tau;
            _model.SetRotation(Vector3.Zero, new (-player.Angle.X - _rotation, 0));

            /* Slowly changes colorValue to target value */
            float target = 1 - spellbook.ElementsCount * .3f;
            if (_colorValue < target)
            {
                _colorValue = Math.Min(_colorValue + 0.02f, target);
            }
            else if (_colorValue > target)
            {
                _colorValue = Math.Max(_colorValue - 0.02f, target);
            }
        }
        public (VertexPositionColorNormal[] Vertices, int[] Indeces) GetVerticesAndIndeces(SpellBook spellbook)
        {

            VertexPositionColorNormal[] Vertices = _model.Move(_position).Vertices;
            int[] Indeces = _model.Move(_position).Indeces;

            /* Gets colors from spellbook */
            Color[] colors = spellbook.ElementColors;

            /* Background color of orb */
            Color backGroundColor = BackGroundColor * _colorValue;

            /* Iterates and draws each triangle in the orb */
            for (int i = 0; i < Vertices.Length; i++)
            {
                int columns = SphereQuality + 1;
                int lerpAmount = i % columns;
                int colorIndex = i / columns % 3;
                Color color = colors[colorIndex];
                if (color == Color.Black)
                {
                    Vertices[i].Color = backGroundColor;
                    continue;
                }
                Vertices[i].Color = Color.Lerp(color, backGroundColor, lerpAmount / (float)columns);
            }
            return (Vertices, Indeces);

            /* List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < _model.Triangles.Length; i++)
            {
                Triangle tri = _model.Triangles[i];
                Color baseColor = Color.FromNonPremultiplied(
                    this.colorValue,
                    this.colorValue,
                    this.colorValue,
                    200
                    );
                // Apply highlights
                Color finalColor = baseColor;
                for (int j = 0; j < _highlights.Length; j++)
                {
                    int highlightIndex = (_swirlPos + (SphereQuality * j)) % (SphereQuality * 3);
                    if (i == highlightIndex)
                    {
                        finalColor = Color.FromNonPremultiplied(
                            Math.Min(255, baseColor.R + 55),
                            Math.Min(255, baseColor.G + 55),
                            Math.Min(255, baseColor.B + 55),
                            255
                            );
                    }
                }
                triangles.Add(
                    new Triangle(
                        tri.P1 + _position,
                        tri.P2 + _position,
                        tri.P3 + _position,
                        finalColor
                        )
                    );
            }
            return triangles.ToArray(); */
        }
    }
}