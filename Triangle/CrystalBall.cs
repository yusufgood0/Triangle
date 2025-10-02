using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace Triangle
{
    internal struct CrystalBall(Vector3 orbOffset)
    {
        public int SwirlPos => _swirlPos;
        public Model Model => _model.Move(_position);
        public Vector3 Position => _position;

        public static int SphereQuality = 60;
        public float SwirlSpeed => _swirlSpeed;
        public int colorValue;

        static Model _model = new Sphere(Vector3.Zero, 30, SphereQuality);
        Vector3 _position;
        Vector3[] _highlights = new Vector3[3];
        int _swirlPos = 0;
        int _swirlSpeed = 1;

        public void SetPosition(Player player)
        {
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(player._angle.X, -player._angle.Y, 0);
            _position = Vector3.Transform(orbOffset, rotation) + player.EyePos;
        }
        public void UpdateHighlights()
        {
            _swirlPos = (_swirlPos + _swirlSpeed) % (SphereQuality * 3);
        }
    }
}