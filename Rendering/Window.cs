// ================================================================================================================================
// File:        Window.cs
// Description: Handles the MonoGame window used to display whats going on in the servers world simulation
// ================================================================================================================================

using System;
using ConversionHelper;
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
        //Singleton instance reference for easy global access to current game window
        public static Window Instance;

        //Rendering tools
        public GraphicsDeviceManager Graphics;
        public ModelDrawer ModelDrawer;
        public BoundingBoxDrawer BoundingBoxDrawer;
        public BasicEffect LineDrawer;
        public TextDrawer TextDrawer;
        public SpriteBatch UIDrawer;
        
        public Rendering.Camera Camera;

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
            //Assign the static instance variable to point to this monogame window for easy global access
            Instance = this;

            //Initialize the graphics device and tell its where all the assets are that will be used during runtime
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            //Set the size and position of the game window
            Graphics.PreferredBackBufferWidth = WindowWidth;
            Graphics.PreferredBackBufferHeight = WindowHeight;
            this.Window.Position = new Point(10, 10);

            //Initialize the debug message log
            Messages = new string[10];
            for (int i = 0; i < 10; i++)
                Messages[i] = "";

            //Set up the scene camera
            //Camera = new Debug.Camera(new BEPUutilities.Vector3(-10, 7, 5), 0, 0, BEPUutilities.Matrix.CreatePerspectiveFieldOfViewRH(Microsoft.Xna.Framework.MathHelper.PiOver4, Graphics.PreferredBackBufferWidth / (float)Graphics.PreferredBackBufferHeight, .1f, 10000));
            Camera = new Rendering.Camera(new BEPUutilities.Vector3(-10, 7, 5), 0, 0, BEPUutilities.Matrix.CreatePerspectiveFieldOfViewRH(Microsoft.Xna.Framework.MathHelper.PiOver4, Graphics.PreferredBackBufferWidth / (float)Graphics.PreferredBackBufferHeight, .1f, 10000));

            //Register the application deconstruction function
            Exiting += CloseWindow;
        }

        //Initialize the window at a specific location
        public Window(int Width, int Height, int XPos, int YPos)
        {
            //Assign the static instance variable to point to this monogame window for easy global access
            Instance = this;

            //Initialize the graphics device and tell its where all the assets are that will be used during runtime
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            //Set the size and position of the game window
            Graphics.PreferredBackBufferWidth = Width;
            Graphics.PreferredBackBufferHeight = Height;
            this.Window.Position = new Point(XPos, YPos);

            //Initialize the debug message log
            Messages = new string[10];
            for (int i = 0; i < 10; i++)
                Messages[i] = "";

            //Set up the scene camera
            //Camera = new Debug.Camera(new BEPUutilities.Vector3(-10, 7, 5), 0, 0, BEPUutilities.Matrix.CreatePerspectiveFieldOfViewRH(Microsoft.Xna.Framework.MathHelper.PiOver4, Graphics.PreferredBackBufferWidth / (float)Graphics.PreferredBackBufferHeight, .1f, 10000));
            Camera = new Rendering.Camera(new BEPUutilities.Vector3(-10, 7, 5), 0, 0, BEPUutilities.Matrix.CreatePerspectiveFieldOfViewRH(Microsoft.Xna.Framework.MathHelper.PiOver4, Graphics.PreferredBackBufferWidth / (float)Graphics.PreferredBackBufferHeight, .1f, 10000));

            //Register the application deconstruction function
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
            //Poll and store this frames keyboard and mouse input
            PreviousKeyboardInput = KeyboardInput;
            KeyboardInput = Keyboard.GetState();
            PreviousMouseInput = MouseInput;
            MouseInput = Mouse.GetState();

            //Calculate this frames delta time value
            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Keep the mouse within the screen when its being used by the window
            if (!IsMouseVisible)
                Mouse.SetPosition(200, 200);

            //Tab toggles mouse use
            if (WasKeyPressed(Keys.Tab))
            {
                IsMouseVisible = !IsMouseVisible;
                if(Physics.WorldSimulator.FPSController != null)
                {
                    if (Physics.WorldSimulator.FPSController.IsActive)
                        Physics.WorldSimulator.FPSController.Deactivate();
                    else
                        Physics.WorldSimulator.FPSController.Activate();
                }
            }

            //F2 - have the enemy seek to the server ghost
            if (WasKeyPressed(Keys.F2))
                Physics.WorldSimulator.Enemy.SeekLocation(Physics.WorldSimulator.FPSController.CharacterController.Body.BufferedStates.Entity.Position);

            //Escape - Shut down the entire server
            if (WasKeyPressed(Keys.Escape))
                this.Exit();

            //G - Toggle wireframe rendering
            if (WasKeyPressed(Keys.G))
                ModelDrawer.IsWireframe = !ModelDrawer.IsWireframe;

            //F12 - Shut down the server
            if (WasKeyPressed(Keys.F12))
            {
                this.Exit();
                return;
            }

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

            //Grab the view and projection matrix from the scene camera for rendering
            var ViewMatrix = Camera.ViewMatrix;
            var ProjectionMatrix = Camera.ProjectionMatrix;

            //Rendering all the scene geometry, entities etc.
            ModelDrawer.Draw(ViewMatrix, ProjectionMatrix);
            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Microsoft.Xna.Framework.Matrix.Identity;
            LineDrawer.View = MathConverter.Convert(ViewMatrix);
            LineDrawer.Projection = MathConverter.Convert(ProjectionMatrix);
            BoundingBoxDrawer.Draw(LineDrawer, Physics.WorldSimulator.Space);

            base.Draw(gameTime);

            //Draw everything in the UI, FPS counter, debug message log etc.
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

            //Display FPS and Physics values
            TextDrawer.Draw("FPS: ", (int)FPSToDisplay, new Microsoft.Xna.Framework.Vector2(50, Bottom - 150));
            TextDrawer.Draw("Physics Time (ms): ", (int)AveragePhysicsTime, new Microsoft.Xna.Framework.Vector2(50, Bottom - 133));
            TextDrawer.Draw("Collision Pairs: ", Physics.WorldSimulator.Space.NarrowPhase.Pairs.Count, new Microsoft.Xna.Framework.Vector2(50, Bottom - 116));

            //Instruct the user they can toggle FPS controls with the TAB key
            TextDrawer.Draw("TAB: Toggle FPS controls", new Microsoft.Xna.Framework.Vector2(50, Bottom - 100));
            TextDrawer.Draw("F1: Display FPS controller position/rotation", new Microsoft.Xna.Framework.Vector2(50, Bottom - 83));
            TextDrawer.Draw("F2: Have test entity seek to the controllers position", new Microsoft.Xna.Framework.Vector2(50, Bottom - 66));

            //Draw the debug messages
            for (int i = 0; i < 10; i++)
                TextDrawer.Draw(Messages[i], new Microsoft.Xna.Framework.Vector2(10, 10 + (25 * i)));

            UIDrawer.End();
        }
    }
}
