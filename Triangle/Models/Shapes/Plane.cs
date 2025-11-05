using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Models.Shapes
{
    internal class Plane
    {
        Vector3 normal;
        float Distance;
        public Plane(Vector3 P1, Vector3 P2, Vector3 P3)
        {

            Vector3 side1 = P1 - P2;
            Vector3 side2 = P1 - P3;
            Vector3 normalDir = Vector3.Cross(side1, side2);
            normalDir.Normalize();
            Distance = Vector3.Dot(normal, P1);
        }
        public float GetSignedDistanceToPlane(Vector3 point)
        {
            return Vector3.Dot(normal, point) + Distance;
        }
        public bool InsidePlane(Vector3 point) => GetSignedDistanceToPlane(point) > 0;
    }
}
