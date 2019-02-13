using System;

namespace Swaelo_Server
{
    public class PathFindingEntity : ServerEntity
    {
        public PathFindingEntity(Vector3 StartPosition)
        {
            Type = "Fox Princess";
            Entity = new Entity(new Box(StartPosition, 1, 1, 1).CollisionInformation, 1);
            Entity.Position = StartPosition;
            //Box EntityCollider = new Box(StartPosition, 1, 1, 1);
            //Entity = new Entity(EntityCollider.CollisionInformation, 1);
            //Entity.Position = StartPosition;
        }

        /*
        //Moving/Rotating the Entity
        private readonly EntityMover Mover;
        private readonly EntityRotator Rotator;
        //Define target rotation and position over time
        private readonly Path<Quaternion> RotationPath;
        private readonly Path<Vector3> PositionPath;
        //Used to evaluate paths in the update method
        private double PathTime;
        //We're going to use a speed-controlled curve that wraps another curve.
        private CardinalSpline3D WrappedPositionCurve;
        //Speed at which the entity moves and turns while navigating around
        private QuaternionSlerpCurve SlerpCurve;

        public PathFindingEntity(Vector3 StartPosition, Vector3 PatrolStart, Vector3 PatrolEnd)
        {
            Type = "Fox Princess";
            Entity = new Box(StartPosition, 1, 1, 1);
            //This is the internal curve.
            //Speed-controlled curves let the user specify speeds at which an evaluator
            //will move along the curve.  Non-speed controlled curves can move at variable
            //rates based on the interpolation.
            WrappedPositionCurve = new CardinalSpline3D();
            WrappedPositionCurve.PreLoop = CurveEndpointBehavior.Mirror;
            WrappedPositionCurve.PostLoop = CurveEndpointBehavior.Mirror;
            //Start the curve up above the patrol points
            WrappedPositionCurve.ControlPoints.Add(-1, PatrolStart);
            WrappedPositionCurve.ControlPoints.Add(0f, PatrolEnd);
            ////Add a few random control points to the list
            //var Random = new Random();
            //for (int i = 1; i <= 10; i++)
            //{
            //    WrappedPositionCurve.ControlPoints.Add(i, new Vector3(
            //        (float)Random.NextDouble() * 20 - 10,
            //        (float)Random.NextDouble() * 12,
            //        (float)Random.NextDouble() * 20 - 10));
            //}
            PositionPath = WrappedPositionCurve;
            //Speed at which the entity moves and turns while navigating around
            SlerpCurve = new QuaternionSlerpCurve();
            SlerpCurve.ControlPoints.Add(0, Quaternion.Identity);
            SlerpCurve.ControlPoints.Add(1, Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));
            SlerpCurve.ControlPoints.Add(2, Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.Pi));
            SlerpCurve.ControlPoints.Add(3, Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));
            SlerpCurve.ControlPoints.Add(4, Quaternion.Identity);
            //Loop it back on itself
            SlerpCurve.PostLoop = CurveEndpointBehavior.Mirror;
            RotationPath = SlerpCurve;
            Mover = new EntityMover(Entity);
            //Offset the place that the mover tries to reach a little.
            //Now, when the entity spins, it acts more like a hammer swing than a saw.
            Mover.LocalOffset = new Vector3(3, 0, 0);
            Rotator = new EntityRotator(Entity);
            //Add the entity and its movers to the space
            Globals.space.Add(Entity);
            Globals.space.Add(Mover);
            Globals.space.Add(Rotator);
        }

        public void Update(float dt)
        {
            //Increment the time.  Note that the space's timestep is used
            //instead of the method's dt.  This is because the demos, by
            //default, update the space once each game update.  Using the
            //space's update time keeps things synchronized.
            //If the engine is using internal time stepping,
            //the passed in dt should be used instead (or put this logic into
            //an updateable that runs with space updates).
            PathTime += Globals.space.TimeStepSettings.TimeStepDuration;
            Mover.TargetPosition = PositionPath.Evaluate(PathTime);
            Rotator.TargetOrientation = RotationPath.Evaluate(PathTime);
        }
        */
    }
}
