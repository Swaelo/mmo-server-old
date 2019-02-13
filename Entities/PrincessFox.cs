using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swaelo_Server
{
    public class PrincessFox
    {
        public Entity Body;
        public Entity Head;
        private UniversalJoint NeckJoint;
        private SwingLimit NeckLimit;
        public Entity Tail;
        private UniversalJoint TailJoint;
        public Entity LeftEar, RightEar;
        private BallSocketJoint LeftEarJoint, RightEarJoint;

        private Entity FrontLeftLeg, FrontRightLeg, BackLeftLeg, BackRightLeg;
        private Entity FrontLeftKnee, FrontRightKnee, BackLeftKnee, BackRightKnee;
        private RevoluteJoint FrontLeftKneeJoint, FrontRightKneeJoint, BackLeftKneeJoint, BackRightKneeJoint;
        private RevoluteJoint FrontLeftLegJoint, FrontRightLegJoint, BackLeftLegJoint, BackRightLegJoint;
        private PointOnLineJoint FrontLeftLineJoint, FrontRightLineJoint, BackLeftLineJoint, BackRightLineJoint;

        public PrincessFox()
        {
            //Reference the physics space as we are sending a lot of things into it here
            Space Space = Globals.space;

            //Create the body of the fox
            Body = new Box(new Vector3(0, 0, 0), 4, 2, 2, 20);
            Space.Add(Body);

            //Create the head of the fox
            Head = new Cone(Body.Position + new Vector3(3.2f, 3f, 0), 1.5f, .7f, 4);
            Head.OrientationMatrix = Matrix3x3.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            Space.Add(Head);
            //Attach the head to the body
            NeckJoint = new UniversalJoint(Body, Head, Head.Position + new Vector3(-.8f, 0, 0));
            Space.Add(NeckJoint);
            //Stop the head from swinging around too much
            NeckLimit = new SwingLimit(Body, Head, Vector3.Right, Vector3.Right, MathHelper.PiOver4);
            Space.Add(NeckLimit);

            //Create the tail of the fox
            Tail = new Box(Body.Position + new Vector3(-3f, 1f, 0), 1.6f, .1f, .1f, 4);
            Space.Add(Tail);
            //Prevent the tail from twisting off
            TailJoint = new UniversalJoint(Body, Tail, Tail.Position + new Vector3(.8f, 0, 0));
            Space.Add(TailJoint);

            //Create the ears of the fox
            LeftEar = new Box(Head.Position + new Vector3(-.2f, 0, -.65f), .01f, .7f, .2f, 1);
            RightEar = new Box(Head.Position + new Vector3(-.2f, 0, .65f), .01f, .7f, .3f, 1);
            Space.Add(LeftEar);
            Space.Add(RightEar);
            //Attach the ears to the head
            LeftEarJoint = new BallSocketJoint(Head, LeftEar, Head.Position + new Vector3(-.2f, .35f, -.65f));
            RightEarJoint = new BallSocketJoint(Head, RightEar, Head.Position + new Vector3(-.2f, .35f, .65f));
            Space.Add(LeftEarJoint);
            Space.Add(RightEarJoint);

            //-----Create the 4 legs of the fox
            //---front left leg
            FrontLeftLeg = new Box(Body.Position + new Vector3(-1.8f, -.5f, 1.5f), .5f, 3, .2f, 20);
            Space.Add(FrontLeftLeg);
            FrontLeftKnee = new Cylinder(Body.Position + new Vector3(-1.8f, .3f, 1.25f), .1f, .7f, 10);
            FrontLeftKnee.OrientationMatrix = Matrix3x3.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            Space.Add(FrontLeftKnee);
            //connect the knee to the body
            FrontLeftKneeJoint = new RevoluteJoint(Body, FrontLeftKnee, FrontLeftKnee.Position, Vector3.Forward);
            //motorize the connection?
            FrontLeftKneeJoint.Motor.IsActive = true;
            FrontLeftKneeJoint.Motor.Settings.VelocityMotor.GoalVelocity = 1;
            Space.Add(FrontLeftKneeJoint);
            //connect the leg to the knee
            FrontLeftLegJoint = new RevoluteJoint(FrontLeftKnee, FrontLeftLeg, FrontLeftKnee.Position + new Vector3(0, .6f, 0), Vector3.Forward);
            Space.Add(FrontLeftLegJoint);
            //connect the leg to the body
            FrontLeftLineJoint = new PointOnLineJoint(FrontLeftLeg, Body, FrontLeftLeg.Position, Vector3.Up, FrontLeftLeg.Position + new Vector3(0, -.4f, 0));
            Space.Add(FrontLeftLineJoint);
            FrontLeftKnee.OrientationMatrix *= Matrix3x3.CreateFromAxisAngle(Vector3.Forward, MathHelper.Pi);
            //---front right leg
            FrontRightLeg = new Box(Body.Position + new Vector3(1.8f, -.5f, 1.5f), .5f, 3, .2f, 20);
            Space.Add(FrontRightLeg);
            FrontRightKnee = new Cylinder(Body.Position + new Vector3(1.8f, .3f, 1.25f), .1f, .7f, 10);
            FrontRightKnee.OrientationMatrix = Matrix3x3.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            Space.Add(FrontRightKnee);
            //connect the knee to the body
            FrontRightKneeJoint = new RevoluteJoint(Body, FrontRightKnee, FrontRightKnee.Position, Vector3.Forward);
            //motorize the connection?
            FrontRightKneeJoint.Motor.IsActive = true;
            FrontRightKneeJoint.Motor.Settings.VelocityMotor.GoalVelocity = 1;
            Space.Add(FrontRightKneeJoint);
            //connect the leg to the knee
            FrontRightLegJoint = new RevoluteJoint(FrontRightKnee, FrontRightLeg, FrontRightKnee.Position + new Vector3(0, .6f, 0), Vector3.Forward);
            Space.Add(FrontRightLegJoint);
            //connect the leg to the body
            FrontRightLineJoint = new PointOnLineJoint(FrontRightLeg, Body, FrontRightLeg.Position, Vector3.Up, FrontRightLeg.Position + new Vector3(0, -.4f, 0));
            Space.Add(FrontRightLineJoint);
            FrontRightKnee.OrientationMatrix *= Matrix3x3.CreateFromAxisAngle(Vector3.Forward, MathHelper.Pi);
            //---back left leg
            BackLeftLeg = new Box(Body.Position + new Vector3(-1.8f, -.5f, -1.5f), .5f, 3, .2f, 20);
            Space.Add(BackLeftLeg);
            BackLeftKnee = new Cylinder(Body.Position + new Vector3(-1.8f, .3f, -1.25f), .1f, .7f, 10);
            BackLeftKnee.OrientationMatrix = Matrix3x3.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            Space.Add(BackLeftKnee);
            //connect the knee to the body
            BackLeftKneeJoint = new RevoluteJoint(Body, BackLeftKnee, BackLeftKnee.Position, Vector3.Forward);
            //motorize the connection?
            BackLeftKneeJoint.Motor.IsActive = true;
            BackLeftKneeJoint.Motor.Settings.VelocityMotor.GoalVelocity = 1;
            Space.Add(BackLeftKneeJoint);
            //connect the leg to the knee
            BackLeftLegJoint = new RevoluteJoint(BackLeftKnee, BackLeftLeg, BackLeftKnee.Position + new Vector3(0, .6f, 0), Vector3.Forward);
            Space.Add(BackLeftLegJoint);
            //connect the leg to the body
            BackLeftLineJoint = new PointOnLineJoint(BackLeftLeg, Body, BackLeftLeg.Position, Vector3.Up, BackLeftLeg.Position + new Vector3(0, -.4f, 0));
            Space.Add(BackLeftLineJoint);
            BackLeftKnee.OrientationMatrix *= Matrix3x3.CreateFromAxisAngle(Vector3.Forward, MathHelper.Pi);
            //---back right leg
            BackRightLeg = new Box(Body.Position + new Vector3(1.8f, -.5f, -1.5f), .5f, 3, .2f, 20);
            Space.Add(BackRightLeg);
            BackRightKnee = new Cylinder(Body.Position + new Vector3(1.8f, .3f, -1.25f), .1f, .7f, 10);
            BackRightKnee.OrientationMatrix = Matrix3x3.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            Space.Add(BackRightKnee);
            //connect the knee to the body
            BackRightKneeJoint = new RevoluteJoint(Body, BackRightKnee, BackRightKnee.Position, Vector3.Forward);
            //motorize the connection?
            BackRightKneeJoint.Motor.IsActive = true;
            BackRightKneeJoint.Motor.Settings.VelocityMotor.GoalVelocity = 1;
            Space.Add(BackRightKneeJoint);
            //connect the leg to the knee
            BackRightLegJoint = new RevoluteJoint(BackRightKnee, BackRightLeg, BackRightKnee.Position + new Vector3(0, .6f, 0), Vector3.Forward);
            Space.Add(BackRightLegJoint);
            //connect the leg to the body
            BackRightLineJoint = new PointOnLineJoint(BackRightLeg, Body, BackRightLeg.Position, Vector3.Up, BackRightLeg.Position + new Vector3(0, -.4f, 0));
            Space.Add(BackRightLineJoint);
            BackRightKnee.OrientationMatrix *= Matrix3x3.CreateFromAxisAngle(Vector3.Forward, MathHelper.Pi);
        }
    }
}