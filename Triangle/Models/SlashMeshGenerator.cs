using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SlimeGame.Models
{
    public static class SlashMeshGenerator
    {
        public static (
            List<(int, int, int)> TriangleIndices,
            List<(int, int, int, int)> QuadIndices,
            Vector3[] Vertices
        ) GenerateSlashMeshIndexed(
            int segments = 16,
            float length = 2f,
            float width = 0.25f,
            float thickness = 0.05f,
            float curveStrength = 0.6f
        )
        {
            if (segments < 2) segments = 2;

            // ------------------------------
            // 1. BUILD ALL CROSS-SECTION VERTICES
            // ------------------------------
            Vector3[] vertices = new Vector3[segments * 4];

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);

                float x = t * length;
                float y = (float)Math.Sin(t * Math.PI) * curveStrength * length;

                Vector3 center = new Vector3(x, y, 0);

                float tAhead = Math.Min(1f, t + 1f / (segments - 1));

                float xA = tAhead * length;
                float yA = (float)Math.Sin(tAhead * Math.PI) * curveStrength * length;
                Vector3 ahead = new Vector3(xA, yA, 0);

                Vector3 forward = ahead - center;
                if (forward.LengthSquared() < 1e-6f)
                    forward = new Vector3(1, 0, 0);
                forward.Normalize();

                Vector3 up = Vector3.UnitY;
                Vector3 right = Vector3.Cross(forward, up);
                if (right.LengthSquared() < 1e-6f)
                {
                    up = Vector3.UnitZ;
                    right = Vector3.Cross(forward, up);
                }
                right.Normalize();

                Vector3 hr = right * (width * 0.5f);
                Vector3 hu = up * (thickness * 0.5f);

                int baseIndex = i * 4;

                vertices[baseIndex + 0] = center + hr + hu; // TR
                vertices[baseIndex + 1] = center - hr + hu; // TL
                vertices[baseIndex + 2] = center - hr - hu; // BL
                vertices[baseIndex + 3] = center + hr - hu; // BR
            }

            // ------------------------------
            // 2. BUILD INDICES
            // ------------------------------
            var tri = new List<(int, int, int)>();
            var quad = new List<(int, int, int, int)>();

            // side quads for each segment
            for (int i = 0; i < segments - 1; i++)
            {
                int a = i * 4;
                int b = (i + 1) * 4;

                // each side is a quad:
                quad.Add((a + 0, a + 1, b + 1, b + 0)); // top
                quad.Add((a + 1, a + 2, b + 2, b + 1)); // left
                quad.Add((a + 2, a + 3, b + 3, b + 2)); // bottom
                quad.Add((a + 3, a + 0, b + 0, b + 3)); // right
            }

            // ------------------------------
            // 3. CAP QUADS
            // ------------------------------
            int start = 0;
            int end = (segments - 1) * 4;

            quad.Add((start + 0, start + 1, start + 2, start + 3)); // root
            quad.Add((end + 2, end + 1, end + 0, end + 3));         // tip (reverse)

            // ------------------------------
            // 4. TRIANGLES FOR FULL GEOMETRY
            // ------------------------------
            foreach (var q in quad)
            {
                tri.Add((q.Item1, q.Item2, q.Item3));
                tri.Add((q.Item1, q.Item3, q.Item4));
            }

            return (tri, quad, vertices);
        }
    }
}
