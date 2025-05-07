using FlatLibraryFor2D.Graphics;
using FlatLibraryFor2D.Input;
using FlatLibraryFor2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using FlatPhysicsEngineFor2D;

namespace FlatGameFor2D
{
    public class Game1 : Game
    {
		private GraphicsDeviceManager graphics;
		private Screen screen;
		private Sprites sprites;
		private Shapes shapes;
		private Camera camera;


		private FlatVector vectorA = new FlatVector(3f, 4f);


		public Game1()
		{
			this.graphics = new GraphicsDeviceManager(this);
			this.graphics.SynchronizeWithVerticalRetrace = true;

			this.Content.RootDirectory = "Content";
			this.IsMouseVisible = true;
			this.IsFixedTimeStep = true;

			const double UpdatePerSecond = 60d;
			this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatePerSecond));
		}

		protected override void Initialize()
		{

			FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);

			this.screen = new Screen(this, 1280, 768);
			this.sprites = new Sprites(this);
			this.shapes = new Shapes(this);
			this.camera = new Camera(this.screen);
			this.camera.Zoom = 5;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			FlatKeyboard keyboard = FlatKeyboard.Instance;
			FlatMouse mouse = FlatMouse.Instance;

			keyboard.Update();
			mouse.Update();

			if (keyboard.IsKeyAvailable)
			{
				if (keyboard.IsKeyClicked(Keys.Escape))
				{
					this.Exit();
				}

				if (keyboard.IsKeyClicked(Keys.A))
				{
					this.camera.IncZoom();
				}

				if (keyboard.IsKeyClicked(Keys.S))
				{
					this.camera.DecZoom();
				}

			}
		}

		protected override void Draw(GameTime gameTime)
		{
			this.screen.Set();
			this.GraphicsDevice.Clear(new Color(50, 60, 70));

			FlatVector normarlized = FlatPhysicsEngineFor2D.FlathMath.Normalize(this.vectorA);

			this.shapes.Begin(this.camera);
			//this.shapes.DrawCircle(0, 0, 32, 32, Color.White);
			this.shapes.DrawLine(Vector2.Zero, FlatConverter.ToVector2(this.vectorA), Color.Red);
			this.shapes.DrawLine(Vector2.Zero, FlatConverter.ToVector2(normarlized), Color.Blue);
			this.shapes.DrawCircle(Vector2.Zero, 1f, 24, Color.White);
			this.shapes.End();

			this.screen.Unset();
			this.screen.Present(this.sprites);

			base.Draw(gameTime);
		}
	}
}
