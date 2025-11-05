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
namespace SlimeGame
{
    internal struct CrystalBall(Vector3 orbOffset)
    {
        public int SwirlPos => _swirlPos;
        //public Model Model => _model.Move(_position);
        public Vector3 Position => _position;

        public static int SphereQuality = 60;
        public float SwirlSpeed => _swirlSpeed;
        public int colorValue;

        static GenericModel _model = new Sphere(Vector3.Zero, 30, SphereQuality, Color.White);
        Vector3 _position;
        Vector3[] _highlights = new Vector3[3];
        int _swirlPos = 0;
        int _swirlSpeed = 1;

        public void SetPosition(Player player)
        {
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(player.Angle.X, -player.Angle.Y, 0);
            _position = Vector3.Transform(orbOffset, rotation) + player.EyePos;
        }
        public void UpdateHighlights(SpellBook spellbook)
        {
            _swirlPos = (_swirlPos + _swirlSpeed) % (SphereQuality * 3);

            /* Slowly changes colorValue to target value */
            int target = 200 - spellbook.ElementsCount * 45;
            if (this.colorValue < target)
            {
                this.colorValue = Math.Min(this.colorValue + 5, target);
            }
            else if (this.colorValue > target)
            {
                this.colorValue = Math.Max(this.colorValue - 5, target);
            }
        }
        public Shape[] GetRenderModel(SpellBook spellbook)
        {

            Shape[] shapes = _model.Move(_position).Shapes;

            /* Gets colors from spellbook */
            Color[] colors = spellbook.ElementColors;

            /* Background color of orb */
            Color backGroundColor = new Color(this.colorValue, 0, this.colorValue);

            /* Iterates and draws each triangle in the orb */
            for (int i = 0; i < shapes.Length; i++)
            {
                int iPlusSwirlPosition = this.SwirlPos + i;
                int columns = CrystalBall.SphereQuality + 1;
                int lerpAmount = iPlusSwirlPosition % columns;
                int colorIndex = iPlusSwirlPosition / columns % 3;
                Color color = colors[colorIndex];
                if (color == Color.Black)
                {
                    shapes[i].Color = backGroundColor;
                    continue;
                }
                shapes[i].Color = Color.Lerp(color, backGroundColor, lerpAmount / (float)columns);
            }
            return shapes;

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