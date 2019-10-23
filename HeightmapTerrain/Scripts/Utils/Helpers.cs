using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Input;

namespace HeightmapTerrain.Scripts.Utils
{
    public class Helpers
    {
        public static BoundingBox FromPoints(VertexPositionNormalTexture[] verts)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            for (int i = 0; i < verts.Length; ++i)
            {
                var v = verts[i];
                Vector3.Min(ref min, ref v.Position, out min);
                Vector3.Max(ref max, ref v.Position, out max);
            }

            return new BoundingBox(min, max);
        }

        public static BoundingBox FromPoints(VertexPositionNormalColor[] verts)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            for (int i = 0; i < verts.Length; ++i)
            {
                var v = verts[i];
                Vector3.Min(ref min, ref v.Position, out min);
                Vector3.Max(ref max, ref v.Position, out max);
            }

            return new BoundingBox(min, max);
        }
    }
}
