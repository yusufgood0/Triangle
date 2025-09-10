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
    internal class CrystalBall
    {
        static Model _model = new Sphere(Vector3.Zero, 32, 5);
        Vector3 orbOffset = new Vector3(40, 40, 60);

        public void setPos(Player player)
        {
            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(player._angle.X, -player._angle.Y, 0);
            Vector3 orbVector = Vector3.Transform(orbOffset, quaternion) + player.EyePos;

            Model sphere = new Sphere(orbVector, 30, 50);
        }
    }
}