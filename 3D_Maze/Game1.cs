using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _3D_Maze
{
    //3DP Projekt - bbm2h21amr - 3. Semester - Senju Murase

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Song song;
        private SoundEffect johnny;

        private Camera camera;
        private Maze maze;
        private BasicEffect effect;
        //effect describes to the rendering system how the pixels
        //on the display should be constructed based on the code

        private float moveScale = 1.5f;
        private float rotateScale = MathHelper.PiOver2;
        //both used in order to move the camera/player

        private GoalObject goalObject;

        //Texture2D hedge;
        private Texture2D HUD;
        private Texture2D Message;
        private Texture2D face;

        private bool goal = false;
        private bool scared = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera = new Camera(new Vector3(0.5f, 0.5f, 0.5f), 0, GraphicsDevice.Viewport.AspectRatio, 0.05f, 100f);
            //sets the camera - default position
            effect = new BasicEffect(GraphicsDevice);
            maze = new Maze(GraphicsDevice);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            goalObject = new GoalObject(this.GraphicsDevice, camera.Position, 10f, Content.Load<Texture2D>("carpet"));

            //hedge = Content.Load<Texture2D>("hedge");
            HUD = Content.Load<Texture2D>("HUD");
            Message = Content.Load<Texture2D>("Message");
            face = Content.Load<Texture2D>("face");

            johnny = Content.Load<SoundEffect>("johnny");
            SoundEffect.MasterVolume = 0.5f;

            this.song = Content.Load<Song>("bgm");
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
            //plays bgm
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //exits the game when "Esc" key is pressed

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyState = Keyboard.GetState();
            //returns which keys are pressed
            float moveAmount = 0;

            if (keyState.IsKeyDown(Keys.Right))
            {
                //rotates the camera to the right
                camera.Rotation = MathHelper.WrapAngle(camera.Rotation - (rotateScale * elapsed));
            }

            if (keyState.IsKeyDown(Keys.Left))
            {
                //rotates the camera to the left
                camera.Rotation = MathHelper.WrapAngle(camera.Rotation + (rotateScale * elapsed));
            }

            if (keyState.IsKeyDown(Keys.Up))
            {
                //camera moves forwards (towards an object)

                //camera.MoveForward(moveScale * elapsed);
                moveAmount = moveScale * elapsed;
            }

            if (keyState.IsKeyDown(Keys.Down))
            {
                //camera moves backwards (away from an object)

                //camera.MoveForward(-moveScale * elapsed);
                moveAmount = -moveScale * elapsed;
            }

            if (moveAmount != 0)
            {
                Vector3 newLocation = camera.Rotate(moveAmount);
                bool moveOk = true;
                //IMPORTANT! The exception-handling takes place here
                //The Rotate() method in the Camera class checks whether the movement requested by the player is allowed
                //Only when it is allowed (moveOk = true), the camera moves according to command

                if (newLocation.X < 0 || newLocation.X > Maze.mazeWidth)
                    moveOk = false;
                if (newLocation.Z < 0 || newLocation.Z > Maze.mazeHeight)
                    moveOk = false;

                foreach (BoundingBox box in maze.DetectWallCollision((int)newLocation.X, (int)newLocation.Z))
                {
                    //when player gets in contact with the wall, moveOk is set to false
                    //meaning, the player will not be able to move towards / pass the wall (anymore)
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                        moveOk = false;
                }

                if (moveOk && goal == false && scared == false) {
                    camera.MoveTowards(moveAmount);
                    //moves the camera by command (depending on the key pressed)
                    //as long as there is no dialog present
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        //replays song when new game is started
                        MediaPlayer.Play(song);
                    }
                }
            }

            if (goalObject.Bounds.Contains(camera.Position) == ContainmentType.Contains)
            {
                //when the player gets in contact with the goal object,
                //the goal object transfers itself to a new location
                goalObject.PositionObject(camera.Position, 5f);
                scared = true;
                MediaPlayer.Stop();
                johnny.Play();
                //plays soundeffect
            }

            if (scared == true && goal == false && keyState.IsKeyDown(Keys.Enter))
            {
                //when the dialog is opened, it can be closed by pressing the "Enter" key
                goal = true;
            }
            else if (scared == true && goal == true && keyState.IsKeyDown(Keys.N))
            {
                scared = false;
                goal = false;
            }

            goalObject.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);

            maze.Draw(camera, effect);

            goalObject.Draw(camera, effect);

            spriteBatch.Begin();
            if (goal == false && scared == false)
            {
                spriteBatch.Draw(HUD, new Vector2(0, 0), Color.White);
            }
            else if (goal == false && scared == true)
            {
                //Dialog is shown and HUD disappears when player makes to the goal
                spriteBatch.Draw(face, new Vector2(0, 0), Color.White);
            }
            else if (goal == true && scared == true)
            {
                spriteBatch.Draw(face, new Vector2(0, 0), Color.White);
                spriteBatch.Draw(Message, new Vector2(0, 0), Color.White);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}