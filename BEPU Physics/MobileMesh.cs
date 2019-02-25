using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Swaelo_Server
{
    public class MobileMesh : Entity<MobileMeshCollidable>
    {
        public MobileMesh(Vector3[] vertices, int[] indices, AffineTransform localTransform, MobileMeshSolidity solidity)
        {
            Vector3 center;
            var shape = new MobileMeshShape(vertices, indices, localTransform, solidity, out center);
            Initialize(new MobileMeshCollidable(shape));
            Position = center;
        }

        public MobileMesh(Vector3[] vertices, int[] indices, AffineTransform localTransform, MobileMeshSolidity solidity, float mass)
        {
            Vector3 center;
            var shape = new MobileMeshShape(vertices, indices, localTransform, solidity, out center);
            Initialize(new MobileMeshCollidable(shape), mass);
            Position = center;
        }

    }
}
