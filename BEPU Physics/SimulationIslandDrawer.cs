using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Swaelo_Server
{
    /// <summary>
    /// Renders bounding boxes of simulation islands.
    /// </summary>
    public class SimulationIslandDrawer
    {
        Game game;
        public SimulationIslandDrawer(Game game)
        {
            this.game = game;
        }

        RawList<VertexPositionColor> boundingBoxLines = new RawList<VertexPositionColor>();
        Dictionary<SimulationIsland, Microsoft.Xna.Framework.BoundingBox> islandBoundingBoxes = new Dictionary<SimulationIsland, Microsoft.Xna.Framework.BoundingBox>();

        public void Draw(Effect effect, Space space)
        {
            if (space.Entities.Count > 0)
            {
                Microsoft.Xna.Framework.BoundingBox box;
                foreach (var entity in space.Entities)
                {
                    var island = entity.ActivityInformation.SimulationIsland;
                    if (island != null)
                    {
                        if (islandBoundingBoxes.TryGetValue(island, out box))
                        {
                            box = Microsoft.Xna.Framework.BoundingBox.CreateMerged(MathConverter.Convert(entity.CollisionInformation.BoundingBox), box);
                            islandBoundingBoxes[island] = box;
                        }
                        else
                        {
                            islandBoundingBoxes.Add(island, MathConverter.Convert(entity.CollisionInformation.BoundingBox));
                        }
                    }
                }
                foreach (var islandBoundingBox in islandBoundingBoxes)
                {
                    Color colorToUse = islandBoundingBox.Key.IsActive ? Color.DarkGoldenrod : Color.DarkGray;
                    Microsoft.Xna.Framework.Vector3[] boundingBoxCorners = islandBoundingBox.Value.GetCorners();
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[0], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[1], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[2], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[3], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[4], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[5], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], colorToUse));

                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[6], colorToUse));
                    boundingBoxLines.Add(new VertexPositionColor(boundingBoxCorners[7], colorToUse));

                }

                if (space.DeactivationManager.SimulationIslands.Count > 0)
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, boundingBoxLines.Elements, 0, islandBoundingBoxes.Count * 12);
                    }
                islandBoundingBoxes.Clear();
                boundingBoxLines.Clear();
            }
        }
    }
}
