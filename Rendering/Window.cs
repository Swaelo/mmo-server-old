using System;
using BEPUphysics;
using BEPUutilities;
using ConversionHelper;
using BEPUphysicsDrawer;
using BEPUphysicsDrawer.Font;
using BEPUphysicsDrawer.Lines;
using BEPUphysicsDrawer.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Server.Rendering
{
    public class Window : Game
    {
        //Rendering tools
        public GraphicsDeviceManager Graphics;
        public Camera Camera;
        public ModelDrawer ModelDrawer;
        public BoundingBoxDrawer BoundingBoxDrawer;
        public BasicEffect LineDrawer;
        public TextDrawer TextDrawer;
        public SpriteBatch UIDrawer;

        //Content
        private SpriteFont DataFont;

        //Input
        public KeyboardState KeyboardInput;
        public KeyboardState PreviousKeyboardInput;
        public MouseState MouseInput;
        public MouseState PreviousMouseInput;

        //FPS calculation
        private double FPSLastTime;
        private double FPSTotalSinceLast;
        private double FPSToDisplay;
        private double AveragePhysicsTime;
        private int FPSTotalFramesSinceLast;

        //the game window will display the last 10 debug messages that were printed out
        private static string[] Messages;
        public static void DisplayMessage(string NewMessage)
        {
            if (Messages == null)
                return;
            //Move all the previous messages down a line then display the new message to the last line
            for (int i = 8; i > 0; i--)
                Messages[i] = Messages[i - 1];
            Messages[0] = NewMessage;
        }
        public static void EditPreviousMessage(string Message)
        {
            Messages[0] = Message;
        }

        //Initializes the monogame window with the specified size
        public Window(int WindowWidth, int WindowHeight)
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            Graphics.PreferredBackBufferWidth = WindowWidth;
            Graphics.PreferredBackBufferHeight = WindowHeight;

            Messages = new string[10];
            for (int i = 0; i < 10; i++)
                Messages[i] = "";

            Camera = new Camera(BEPUutilities.Vector3.Zero, 0, 0, BEPUutilities.Matrix.CreatePerspectiveFieldOfViewRH(BEPUutilities.MathHelper.PiOver4, WindowWidth / (float)WindowHeight, .1f, 10000), this);
            Camera.LockedUp = BEPUutilities.Vector3.Up;
            Camera.ViewDirection = new BEPUutilities.Vector3(0, -1.5f, 1);
            Camera.Position = new BEPUutilities.Vector3(-52, 94, -85);

            Exiting += CloseWindow;
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            ModelDrawer.Dispose();
            UIDrawer.Dispose();
        }

        protected override void Initialize()
        {
            ModelDrawer = new InstancedModelDrawer(this);
            BoundingBoxDrawer = new BoundingBoxDrawer(this);

            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Server does not currently run at a fixed timestep
            IsFixedTimeStep = false;

            //Load font data and set up the UI and Text Drawers
            DataFont = Content.Load<SpriteFont>("DataFont");
            LineDrawer = new BasicEffect(GraphicsDevice);
            UIDrawer = new SpriteBatch(GraphicsDevice);
            TextDrawer = new TextDrawer(UIDrawer, DataFont, Color.White);

            ModelDrawer.Clear();

            Physics.WorldSimulator.InitializeSimulation(this);
            Physics.WorldSimulator.CleanUp();

            GC.Collect();
        }

        public bool WasKeyPressed(Keys Key)
        {
            return KeyboardInput.IsKeyDown(Key) && PreviousKeyboardInput.IsKeyUp(Key);
        }

        protected override void Update(GameTime gameTime)
        {
            PreviousKeyboardInput = KeyboardInput;
            KeyboardInput = Keyboard.GetState();
            PreviousMouseInput = MouseInput;
            MouseInput = Mouse.GetState();

            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Keep the mouse within the screen when its being used by the window
            if (!IsMouseVisible)
                Mouse.SetPosition(200, 200);

            //Tab toggles mouse use
            if (WasKeyPressed(Keys.Tab))
                IsMouseVisible = !IsMouseVisible;

            //Toggle wireframe rendering with G
            if (WasKeyPressed(Keys.G))
                ModelDrawer.IsWireframe = !ModelDrawer.IsWireframe;

            //Update the server simulation
            Physics.WorldSimulator.Update(DeltaTime);

            //Render everything
            ModelDrawer.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //clear the render buffer
            GraphicsDevice.Clear(new Color(.41f, .41f, .41f, 1));

            var ViewMatrix = Camera.ViewMatrix;
            var ProjectionMatrix = Camera.ProjectionMatrix;

            ModelDrawer.Draw(ViewMatrix, ProjectionMatrix);
            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Microsoft.Xna.Framework.Matrix.Identity;
            LineDrawer.View = MathConverter.Convert(ViewMatrix);
            LineDrawer.Projection = MathConverter.Convert(ProjectionMatrix);
            BoundingBoxDrawer.Draw(LineDrawer, Physics.WorldSimulator.Space);

            base.Draw(gameTime);

            UIDrawer.Begin();
            int Bottom = GraphicsDevice.Viewport.Bounds.Height;
            int Right = GraphicsDevice.Viewport.Bounds.Width;
            FPSTotalSinceLast += gameTime.ElapsedGameTime.TotalSeconds;
            FPSTotalFramesSinceLast++;
            if(gameTime.TotalGameTime.TotalSeconds - FPSLastTime > .25f && gameTime.ElapsedGameTime.TotalSeconds > 0)
            {
                double Average = FPSTotalSinceLast / FPSTotalFramesSinceLast;
                FPSLastTime = gameTime.TotalGameTime.TotalSeconds;
                FPSToDisplay = Math.Round(1 / Average, 1);
                AveragePhysicsTime = Math.Round(1000 * Physics.WorldSimulator.PhysicsTime, 1);
                FPSTotalSinceLast = 0;
                FPSTotalFramesSinceLast = 0;
            }

            TextDrawer.Draw("FPS: ", (int)FPSToDisplay, new Microsoft.Xna.Framework.Vector2(50, Bottom - 150));
            TextDrawer.Draw("Physics Time (ms): ", (int)AveragePhysicsTime, new Microsoft.Xna.Framework.Vector2(50, Bottom - 133));
            TextDrawer.Draw("Collision Pairs: ", Physics.WorldSimulator.Space.NarrowPhase.Pairs.Count, new Microsoft.Xna.Framework.Vector2(50, Bottom - 116));

            //Draw the debug messages
            for(int i = 0; i < 10; i++)
                TextDrawer.Draw(Messages[i], new Microsoft.Xna.Framework.Vector2(10, 10 + (25 * i)));

            UIDrawer.End();
        }
    }
}
