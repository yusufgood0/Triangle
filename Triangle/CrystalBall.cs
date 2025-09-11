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

        public static int SphereQuality = 40;
        public float SwirlSpeed => _swirlSpeed;
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
        public void UpdateHighlights(Random rnd)
        {
            _swirlPos = (_swirlPos + _swirlSpeed) % (SphereQuality * 3);
            //if (_swirlPos > 100f || _swirlPos < -100f)
            //{
            //    _swirlSpeed = -_swirlSpeed;
            
            //for (int i = 0; i < _highlights.Length; i++)
            //{
            //    Quaternion rotation = Quaternion.CreateFromYawPitchRoll((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
            //    _highlights[0] = Vector3.Transform(_highlights[0], rotation);
            //}
        }
    }
}