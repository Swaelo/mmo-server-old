using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Swaelo_Server
{
    public class WorldRenderer : Game
    {
        public Camera Camera;

        private WorldSimulator ServerSimulation;
        public WorldSimulator GetWorld() { return ServerSimulation; }

        //Rendering Variables
        public GraphicsDeviceManager Graphics;

        //Rendering tools
        public ModelDrawer ModelDrawer;
        public LineDrawer ConstraintDrawer;
        public ContactDrawer ContactDrawer;
        public BoundingBoxDrawer BoundingBoxDrawer;
        public SimulationIslandDrawer SimulationIslandDrawer;
        public BasicEffect LineDrawer;
        public SpriteBatch UIDrawer;
        public TextDrawer DataTextDrawer;
        public TextDrawer TinyTextDrawer;

        //Content
        private SpriteFont dataFont;
        private SpriteFont tinyFont;
        private Texture2D controlsMenu;

        //FPS calculation variables
        private double FPSlastTime;
        private double FPStotalSinceLast;
        private double FPStoDisplay;
        private double averagePhysicsTime;
        private int FPStotalFramesSinceLast;

        //Terminal Control Console
        public string[] ControlConsoleOutputLines;
        public int ControlConsoleLineHistorySize;
        public void InitializeControlConsole(int LineHistorySize)
        {
            ControlConsoleLineHistorySize = LineHistorySize;
            ControlConsoleOutputLines = new string[ControlConsoleLineHistorySize];
        }
        public void UpdateControlConsole(float deltaTime)
        {

        }
        public void DrawControlConsole(float deltaTime)
        {


            /*
             * 
                DataTextDrawer.Draw("FPS: ", (int)FPStoDisplay, new Microsoft.Xna.Framework.Vector2(50, bottom - 150));
                DataTextDrawer.Draw("Physics Time (ms): ", (int)averagePhysicsTime, new Microsoft.Xna.Framework.Vector2(50, bottom - 133));
                DataTextDrawer.Draw("Collision Pairs: ", Globals.space.NarrowPhase.Pairs.Count, new Microsoft.Xna.Framework.Vector2(50, bottom - 116));
             * */
        }

        //Input
        public KeyboardState KeyboardInput;
        public KeyboardState PreviousKeyboardInput;
        public GamePadState GamePadInput;
        public GamePadState PreviousGamePadInput;
        public MouseState MouseInput;
        public MouseState PreviousMouseInput;

        //Display Booleans        
        public bool displayEntities = true;
        public bool displayUI = true;
        public bool displayActiveEntityCount = true;
        public bool displayConstraints = true;
        private bool displayMenu;
        private bool displayContacts;
        private bool displayBoundingBoxes;
        private bool displaySimulationIslands;

        public WorldRenderer(int WindowWidth, int WindowHeight)
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Graphics.PreferredBackBufferWidth = WindowWidth;
            Graphics.PreferredBackBufferHeight = WindowHeight;
            Camera = new Camera(Vector3.Zero, 0, 0, Matrix.CreatePerspectiveFieldOfViewRH(MathHelper.PiOver4, Graphics.PreferredBackBufferWidth / (float)Graphics.PreferredBackBufferHeight, .1f, 10000));

            Exiting += ExitServerGame;
        }

        private void ExitServerGame(object sender, EventArgs e)
        {
            ServerSimulation.CleanUp();
            ModelDrawer.Dispose();
            LineDrawer.Dispose();
            ConstraintDrawer.Dispose();
            UIDrawer.Dispose();
            controlsMenu.Dispose();
        }

        protected override void Initialize()
        {
            ModelDrawer = new InstancedModelDrawer(this);

            ConstraintDrawer = new LineDrawer(this);
            ConstraintDrawer.DisplayTypes.Add(typeof(GrabSpring), typeof(DisplayGrabSpring));
            ConstraintDrawer.DisplayTypes.Add(typeof(MotorizedGrabSpring), typeof(DisplayMotorizedGrabSpring));

            ContactDrawer = new ContactDrawer(this);
            BoundingBoxDrawer = new BoundingBoxDrawer(this);
            SimulationIslandDrawer = new SimulationIslandDrawer(this);

            //setup terminal control console
            InitializeControlConsole(8);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            dataFont = Content.Load<SpriteFont>("DataFont");
            tinyFont = Content.Load<SpriteFont>("TinyFont");

            controlsMenu = Content.Load<Texture2D>("bepuphysicscontrols");

            IsFixedTimeStep = false;

            LineDrawer = new BasicEffect(GraphicsDevice);

            UIDrawer = new SpriteBatch(GraphicsDevice);

            DataTextDrawer = new TextDrawer(UIDrawer, dataFont, Color.White);
            TinyTextDrawer = new TextDrawer(UIDrawer, tinyFont, Color.White);

            Mouse.SetPosition(200, 200);

            ModelDrawer.Clear();
            ConstraintDrawer.Clear();

            if (ServerSimulation != null)
                ServerSimulation.CleanUp();

            Type SimulationType = typeof(WorldSimulator);
            Globals.world = (WorldSimulator)Activator.CreateInstance(SimulationType, new object[] { this });
            ServerSimulation = Globals.world;

            foreach (Entity e in Globals.space.Entities)
            {
                if ((string)e.Tag != "noDisplayObject")
                    ModelDrawer.Add(e);
                else
                    e.Tag = null;
            }
            for (int i = 0; i < Globals.space.Solver.SolverUpdateables.Count; i++)
            {
                //Add the solver updateable and match up the activity setting.
                LineDisplayObjectBase objectAdded = ConstraintDrawer.Add(Globals.space.Solver.SolverUpdateables[i]);
                if (objectAdded != null)
                    objectAdded.IsDrawing = Globals.space.Solver.SolverUpdateables[i].IsActive;
            }

            GC.Collect();
        }

        public bool WasKeyPressed(Keys key)
        {
            return KeyboardInput.IsKeyDown(key) && PreviousKeyboardInput.IsKeyUp(key);
        }

        public bool WasButtonPressed(Buttons button)
        {
            return GamePadInput.IsButtonDown(button) && PreviousGamePadInput.IsButtonUp(button);
        }

        protected override void Update(GameTime gameTime)
        {
            PreviousKeyboardInput = KeyboardInput;
            KeyboardInput = Keyboard.GetState();
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            PreviousMouseInput = MouseInput;
            MouseInput = Mouse.GetState();

            //Keep the mouse within the screen
            if (!IsMouseVisible)
                Mouse.SetPosition(200, 200);

            //Allow quit with escape key
            if (KeyboardInput.IsKeyDown(Keys.Escape))
                Exit();

            //Display camera location and facing direction with F1
            if (WasKeyPressed(Keys.F1))
            {
                Vector3 CameraPosition = ServerSimulation.ServerCamera.Camera.Position;
                Log.Out("Cam: " + CameraPosition);
            }

            //Tab toggle mouse lock
            if (WasKeyPressed(Keys.Tab))
                IsMouseVisible = !IsMouseVisible;

            //Toggle wireframe with G
            if (WasKeyPressed(Keys.G))
                ModelDrawer.IsWireframe = !ModelDrawer.IsWireframe;

            //Allow control console to do whatever it wants
            UpdateControlConsole(dt);

            //Update everything
            ServerSimulation.Update(dt);

            //Render everything
            if (displayConstraints)
                ConstraintDrawer.Update();
            if (displayEntities)
                ModelDrawer.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(.41f, .41f, .45f, 1));

            var viewMatrix = Camera.ViewMatrix;
            var projectionMatrix = Camera.ProjectionMatrix;

            if (displayEntities)
                ModelDrawer.Draw(viewMatrix, projectionMatrix);

            if (displayConstraints)
                ConstraintDrawer.Draw(viewMatrix, projectionMatrix);

            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Microsoft.Xna.Framework.Matrix.Identity;
            LineDrawer.View = MathConverter.Convert(viewMatrix);
            LineDrawer.Projection = MathConverter.Convert(projectionMatrix);

            if (displayContacts)
                ContactDrawer.Draw(LineDrawer, Globals.space);

            if (displayBoundingBoxes)
                BoundingBoxDrawer.Draw(LineDrawer, Globals.space);

            if (displaySimulationIslands)
                SimulationIslandDrawer.Draw(LineDrawer, Globals.space);

            base.Draw(gameTime);

            UIDrawer.Begin();
            int bottom = GraphicsDevice.Viewport.Bounds.Height;
            int right = GraphicsDevice.Viewport.Bounds.Width;
            if (displayUI)
            {
                FPStotalSinceLast += gameTime.ElapsedGameTime.TotalSeconds;
                FPStotalFramesSinceLast++;
                if (gameTime.TotalGameTime.TotalSeconds - FPSlastTime > .25 && gameTime.ElapsedGameTime.TotalSeconds > 0)
                {
                    double avg = FPStotalSinceLast / FPStotalFramesSinceLast;
                    FPSlastTime = gameTime.TotalGameTime.TotalSeconds;
                    FPStoDisplay = Math.Round(1 / avg, 1);
                    averagePhysicsTime = Math.Round(1000 * ServerSimulation.PhysicsTime, 1);
                    FPStotalSinceLast = 0;
                    FPStotalFramesSinceLast = 0;
                }

                //Display console control terminal
                DrawControlConsole((int)averagePhysicsTime);

                DataTextDrawer.Draw("FPS: ", (int)FPStoDisplay, new Microsoft.Xna.Framework.Vector2(50, bottom - 150));
                DataTextDrawer.Draw("Physics Time (ms): ", (int)averagePhysicsTime, new Microsoft.Xna.Framework.Vector2(50, bottom - 133));
                DataTextDrawer.Draw("Collision Pairs: ", Globals.space.NarrowPhase.Pairs.Count, new Microsoft.Xna.Framework.Vector2(50, bottom - 116));
                if (displayActiveEntityCount)
                {
                    int countActive = 0;
                    for (int i = 0; i < Globals.space.Entities.Count; i++)
                    {
                        if (Globals.space.Entities[i].ActivityInformation.IsActive)
                            countActive++;
                    }
                    DataTextDrawer.Draw("Active Objects: ", countActive, new Microsoft.Xna.Framework.Vector2(50, bottom - 99));
                }
                DataTextDrawer.Draw("Press F1 to output camera postion", new Microsoft.Xna.Framework.Vector2(50, bottom - 82));

                ServerSimulation.DrawUI();
            }
            if (displayMenu)
            {
                UIDrawer.Draw(controlsMenu, new Rectangle(right / 2 - 400, bottom / 2 - 300, 800, 600), Color.White);
            }
            UIDrawer.End();
        }
    }
}