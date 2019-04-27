// ================================================================================================================================
// File:        WorldSimulator.cs
// Description: Handles all the server side physics simulation / game logic while the server is running
// ================================================================================================================================

using System;
using System.Diagnostics;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUutilities.Threading;
using Server.Items;

namespace Server.Physics
{
    public static class WorldSimulator
    {
        private static Rendering.Window Window;     //monogame window used to render whats going on with the server
        private static int AccumulatedPhysicsFrames;
        private static double AccumulatedPhysicsTime;
        private static double PreviousTimeMeasurement;
        private static ParallelLooper ParallelLooper;   //assigns however many system threads are available so they can be used by the server to perform jobs
        public static double PhysicsTime { get; private set; }
        public static Space Space;
        
        public static Pathfinding.NavMesh TestLevelNavMesh; //Navigation mesh used for AI pathfinding
        public static Pathfinding.NavMesh WorldNavMesh;
        public static Data.TerrainMesh LevelMesh;   //Level collision mesh used for physics

        public static Entities.EnemyEntity Enemy;   //test enemy
        
        public static Rendering.FPSCharacterControls FPSController; //FPS controller which contains fps camera inside it to move around the server with

        public static void InitializeSimulation(Rendering.Window GameWindow)
        {
            Window = GameWindow;

            //Initialize the parallel looper and assign any extra cpu threads to it
            ParallelLooper = new ParallelLooper();
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                    ParallelLooper.AddThread();
            }
            
            //Initialize the item manager and load all its nessacery information out from the database
            Items.ItemManager.InitializeItemManager();
            
            //Set up the bepu physics world space simulation
            Space = new Space(ParallelLooper);
            Space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            //Position the scene camera then attach it to the fps controller
            Rendering.Window.Instance.Camera.Position = new Vector3(-3.49f, 5.14f, 4.50f);
            Rendering.Window.Instance.Camera.ViewDirection = new Vector3(0.92f, -0.16f, -0.33f);
            FPSController = new Rendering.FPSCharacterControls(Space, Rendering.Window.Instance.Camera, Rendering.Window.Instance);
            FPSController.CharacterController.Body.Tag = "noDisplayObject";

            //Place a ground plane for everyone to stand upon
            Box Ground = new Box(Vector3.Zero, 50, -1, 50);
            Ground.BecomeKinematic();
            GameWindow.ModelDrawer.Add(Ground);
            WorldSimulator.Space.Add(Ground);

            //Place an enemy entity into the level
            Enemy = new Entities.EnemyEntity(new Vector3(10, 0, 10));

            //Place some items onto the ground for the players to pick up
            ItemManager.AddPotionPickup(Potions.HealingPotion, new Vector3(0, 0, -2.5f));
            ItemManager.AddPotionPickup(Potions.ManaPotion, new Vector3(1, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.AstorasStraightSword, new Vector3(2, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.Kusabimaru, new Vector3(3, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.CrusadersShield, new Vector3(4, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.BattleHelm, new Vector3(5, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.LeftBonemouldPauldron, new Vector3(6, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.RightBonemouldPauldron, new Vector3(7, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.LeftClothGlove, new Vector3(8, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.RightClothGlove, new Vector3(9, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.MysteriousAmulet, new Vector3(10, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.LeatherCloak, new Vector3(11, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.EpicPurpleShirt, new Vector3(12, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.OldPants, new Vector3(13, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.LeftNormalBoot, new Vector3(14, 0, -2.5f));
            ItemManager.AddEquipmentPickup(ItemList.RightNormalBoot, new Vector3(15, 0, -2.5f));
        }

        public static void Update(float DeltaTime)
        {
            //Log what time this update function started
            long UpdateStart = Stopwatch.GetTimestamp();
            
            //Update all the entities AI which are active in the game world right now
            Entities.EntityManager.UpdateEntities(DeltaTime);
            
            //Update the physics scene
            Space.Update();

            //update the camera and debug character controller
            Rendering.Window Game = Rendering.Window.Instance;
            FPSController.Update(DeltaTime, Game.PreviousKeyboardInput, Game.KeyboardInput);

            //Calculate the physics time values based on how long this frame update took
            long UpdateEnd = Stopwatch.GetTimestamp();
            AccumulatedPhysicsTime += (UpdateEnd - UpdateStart) / (double)Stopwatch.Frequency;
            AccumulatedPhysicsFrames++;
            PreviousTimeMeasurement += DeltaTime;
            if(PreviousTimeMeasurement > .3f)
            {
                PreviousTimeMeasurement -= .3f;
                PhysicsTime = AccumulatedPhysicsTime / AccumulatedPhysicsFrames;
                AccumulatedPhysicsTime = 0;
                AccumulatedPhysicsFrames = 0;
            }
        }

        public static void CleanUp()
        {
            if(ParallelLooper != null)
                ParallelLooper.Dispose();
        }
    }
}
