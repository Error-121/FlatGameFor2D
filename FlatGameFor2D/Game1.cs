using FlatLibraryFor2D.Graphics;
using FlatLibraryFor2D.Input;
using FlatLibraryFor2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using FlatPhysicsEngineFor2D;
using System.Collections.Generic;

using FlatMath = FlatPhysicsEngineFor2D.FlatMath;

namespace FlatGameFor2D
{
    public class Game1 : Game
    {
		private GraphicsDeviceManager _graphics;
		private Screen _screen;
		private Sprites _sprites;
		private Shapes _shapes;
		private Camera _camera;
		private SpriteFont fontConsolas18;

		private FlatWorld _world;
		private List<FlatEntity> _entityList;
		private List<FlatEntity> _entityRemoveList;

		private Stopwatch _watch;


		private double _totalWorldStepTime = 0d;
		private int _totalBodyCount = 0;
		private int _totalSampleCount = 0;
		private Stopwatch _sampleTimer = new Stopwatch();

		private string _worldStepTimeString = string.Empty;
		private string _bodyCountString = string.Empty;

		public Game1()
		{
			this._graphics = new GraphicsDeviceManager(this);
			this._graphics.SynchronizeWithVerticalRetrace = true;

			this.Content.RootDirectory = "Content";
			this.IsMouseVisible = true;
			this.IsFixedTimeStep = true;

			const double UpdatePerSecond = 60d;
			this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatePerSecond));
		}

		protected override void Initialize()
		{
			this.Window.Position = new Point(10, 40);

			FlatUtil.SetRelativeBackBufferSize(this._graphics, 0.85f);

			this._screen = new Screen(this, 1280, 768);
			this._sprites = new Sprites(this);
			this._shapes = new Shapes(this);
			this._camera = new Camera(this._screen);
			this._camera.Zoom = 20;

			this._camera.GetExtents(out float left, out float right, out float top, out float bottom);


			this._entityList = new List<FlatEntity>();
			this._entityRemoveList = new List<FlatEntity>();
			this._world = new FlatWorld();
			
			float padding = MathF.Abs(right - left) * 0.10f;
			
			if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, 1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
			{
				throw new Exception(errorMessage);
			}

			groundBody.MoveTo(new FlatVector(0, -10));
			this._world.AddBody(groundBody);
			this._entityList.Add(new FlatEntity(groundBody, Color.DarkGreen));


			if (!FlatBody.CreateBoxBody(20f, 2f, 1f, true, 0.5f, out FlatBody ledgeBodyOne, out errorMessage))
			{
				throw new Exception(errorMessage);
			}

			ledgeBodyOne.MoveTo(new FlatVector(-10, 3)); // Move the ledge to a specific position
			ledgeBodyOne.Rotate(-MathHelper.TwoPi / 20f); // Rotate the ledge to create an angle
			this._world.AddBody(ledgeBodyOne);
			this._entityList.Add(new FlatEntity(ledgeBodyOne, Color.DarkGray));

			if (!FlatBody.CreateBoxBody(15f, 2f, 1f, true, 0.5f, out FlatBody ledgeBodyTwo, out errorMessage))
			{
				throw new Exception(errorMessage);
			}

			ledgeBodyTwo.MoveTo(new FlatVector(10, 10)); // Move the ledge to a specific position
			ledgeBodyTwo.Rotate(MathHelper.TwoPi / 20f); // Rotate the ledge to create an angle
			this._world.AddBody(ledgeBodyTwo);
			this._entityList.Add(new FlatEntity(ledgeBodyTwo, Color.DarkRed));


			this._watch = new Stopwatch();
			this._sampleTimer.Start();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
			this.fontConsolas18 = this.Content.Load<SpriteFont>("Consolas18");
		}

		protected override void Update(GameTime gameTime)
		{
			FlatKeyboard keyboard = FlatKeyboard.Instance;
			FlatMouse mouse = FlatMouse.Instance;

			keyboard.Update();
			mouse.Update();

			// add box body
			if (mouse.IsLeftMouseButtonPressed())
			{
				float width = RandomHelper.RandomSingle(2f, 3f);
				float height = RandomHelper.RandomSingle(2f, 3f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this._screen, this._camera));

				this._entityList.Add(new FlatEntity(this._world, width, height, false, mouseWorldPosition));
			}
			// add circle body
			if (mouse.IsRightMouseButtonPressed())
			{
				float radius = RandomHelper.RandomSingle(1f, 1.25f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this._screen, this._camera));

				this._entityList.Add(new FlatEntity(this._world, radius, false, mouseWorldPosition));
			}

			if (keyboard.IsKeyAvailable)
			{
				if (keyboard.IsKeyClicked(Keys.P))
				{
					Console.WriteLine($"BodyCount: {this._world.BodyCount}");
					Console.WriteLine($"StepTime: {Math.Round(this._watch.Elapsed.TotalMilliseconds, 4)}");
					Console.WriteLine();
				}
				if (keyboard.IsKeyClicked(Keys.Escape))
				{
					this.Exit();
				}

				if (keyboard.IsKeyClicked(Keys.A))
				{
					this._camera.IncZoom();
				}

				if (keyboard.IsKeyClicked(Keys.S))
				{
					this._camera.DecZoom();
				}

			}

			if (this._sampleTimer.Elapsed.TotalSeconds > 1d)
			{
				this._bodyCountString = "BodyCount: " + Math.Round(this._totalBodyCount / (double)this._totalSampleCount, 4).ToString();
				this._worldStepTimeString = "WorldStepTime: " + Math.Round(this._totalWorldStepTime / (double)this._totalSampleCount, 4).ToString();
				
				this._totalBodyCount = 0;
				this._totalWorldStepTime = 0d;
				this._totalSampleCount = 0;
				this._sampleTimer.Restart();
			}

			this._watch.Restart();
			this._world.Step(FlatUtil.GetElapsedTimeInSeconds(gameTime), 20);
			this._watch.Stop();

			this._totalWorldStepTime += this._watch.Elapsed.TotalMilliseconds;
			this._totalBodyCount += this._world.BodyCount;
			this._totalSampleCount++;

			this._camera.GetExtents(out _, out _, out float viewBottom, out _);

			this._entityRemoveList.Clear();

			for (int i = 0; i < this._entityList.Count; i++)
			{
				FlatEntity entity = this._entityList[i];
				FlatBody body = entity._body;

				if (body._isStatic)
				{
					continue; // Skip static bodies
				}

				FlatAABB box = body.GetAABB();

				if (box._max._Y < viewBottom)
				{
					this._entityRemoveList.Add(entity); // Add to remove list if the body is below the view
				}
			}

			for (int i = 0; i < this._entityRemoveList.Count; i++)
			{
				FlatEntity entity = this._entityRemoveList[i];
				this._world.RemoveBody(entity._body);
				this._entityList.Remove(entity);
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this._screen.Set();
			this.GraphicsDevice.Clear(new Color(50, 60, 70));

			

			this._shapes.Begin(this._camera);

			for (int i = 0; i < this._entityList.Count; i++)
			{
				this._entityList[i].Draw(this._shapes);
			}

			List<FlatVector> contactPoints = this._world?._ContactPointsList;
			for (int i = 0; i < contactPoints.Count; i++)
			{
				Vector2 contactPositon = FlatConverter.ToVector2(contactPoints[i]);
				_shapes.DrawBoxFill(contactPositon, 0.3f, 0.3f, Color.Red);
				_shapes.DrawBox(contactPositon, 0.3f, 0.3f, Color.White);
			}

			this._shapes.End();


			Vector2 stringSize = this.fontConsolas18.MeasureString(this._bodyCountString);

			this._sprites.Begin();
			this._sprites.DrawString(this.fontConsolas18, this._bodyCountString, new Vector2(0, 0), Color.White);
			this._sprites.DrawString(this.fontConsolas18, this._worldStepTimeString, new Vector2(0, stringSize.Y), Color.White);
			this._sprites.End();

			this._screen.Unset();
			this._screen.Present(this._sprites);

			base.Draw(gameTime);
		}
	}
}
