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
    internal struct Square
    {
        public Triangle[] triangles { get; }
        public Vector3 Average { get => (triangles[0].Average + triangles[1].Average) / 2; }
        public Square(Triangle T1, Triangle T2)
        {
            triangles = new Triangle[] { T1, T2 };
        }
        public Color ApplyShading(Vector3 lightDirection, Color triangleColor, Color lightColor)
        {
            lightDirection.Normalize();

            Triangle triangle = triangles[0];
            Vector3 side1 = triangle.P1 - triangle.P2;
            Vector3 side2 = triangle.P1 - triangle.P3;
            Vector3 normalDir = Vector3.Cross(side1, side2);
            normalDir.Normalize();

            // Calculate the difference in rays between the light direction and the normal vector using Vector3.Dot
            float dotProduct = Vector3.Dot(normalDir, lightDirection);

            // mix colors based on the difference in rays
            return Color.Lerp(triangleColor, lightColor, dotProduct);
        }
        public Square(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            float distance12 = Vector3.DistanceSquared(p1, p2);
            float distance13 = Vector3.DistanceSquared(p1, p3);
            float distance14 = Vector3.DistanceSquared(p1, p4);

            if (distance12 >= distance13 && distance12 >= distance14)
            {
                triangles = new Triangle[]
                {
                    new Triangle(p1, p2, p3),
                    new Triangle(p1, p2, p4)
                };
            }
            else if (distance13 >= distance14)
            {
                triangles = new Triangle[]
                {
                    new Triangle(p1, p3, p2),
                    new Triangle(p1, p3, p4)
                };
            }
            else
            {
                triangles = new Triangle[]
                {
                    new Triangle(p1, p4, p2),
                    new Triangle(p1, p4, p3)
                };
            }
        }
        public void Draw(
            ref TextureBuffer screenBuffer,
            Color color,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int distance
            )
        {
            foreach (Triangle triangle in triangles)
            {
                // Draw each triangle in the square
                triangle.Draw(
                    ref screenBuffer,
                    color,
                    cameraPosition,
                    pitch,
                    yaw,
                    distance
                );
            }
        }
    }
}
