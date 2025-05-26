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

		private List<Color> _colors;
		private List<Color> _outlineColors;

		private Vector2[] _vertexBuffer;

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

			FlatUtil.SetRelativeBackBufferSize(this._graphics, 0.85f);

			this._screen = new Screen(this, 1280, 768);
			this._sprites = new Sprites(this);
			this._shapes = new Shapes(this);
			this._camera = new Camera(this._screen);
			this._camera.Zoom = 24;

			this._camera.GetExtents(out float left, out float right, out float top, out float bottom);
			
			
			
			this._colors = new List<Color>();
			this._outlineColors = new List<Color>();

			this._world = new FlatWorld();
			
			float padding = MathF.Abs(right - left) * 0.10f;
			
			
			if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, new FlatVector(0, -10), 1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
			{
				throw new Exception(errorMessage);
			}

			this._world.AddBody(groundBody);

			this._colors.Add(Color.DarkGreen);
			this._outlineColors.Add(Color.White);




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
				float width = RandomHelper.RandomSingle(1f, 2f);
				float height = RandomHelper.RandomSingle(1f, 2f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this._screen, this._camera));

				if(!FlatBody.CreateBoxBody(width, height, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMessage))
				{
					throw new Exception(errorMessage);
				}

				this._world.AddBody(body);
				this._colors.Add(RandomHelper.RandomColor());
				this._outlineColors.Add(Color.White);
			}
			// add circle body
			if (mouse.IsRightMouseButtonPressed())
			{
				float radius = RandomHelper.RandomSingle(0.75f, 1.25f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this._screen, this._camera));

				if (!FlatBody.CreateCircleBody(radius, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMessage))
				{
					throw new Exception(errorMessage);
				}

				this._world.AddBody(body);
				this._colors.Add(RandomHelper.RandomColor());
				this._outlineColors.Add(Color.White);
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

#if false
				float dx = 0f;
				float dy = 0f;
				float forceMagnitude = 24f;

				if (keyboard.IsKeyDown(Keys.Left)) { dx --; }
				if (keyboard.IsKeyDown(Keys.Right)) { dx++; }
				if (keyboard.IsKeyDown(Keys.Up)) { dy++; }
				if (keyboard.IsKeyDown(Keys.Down)) { dy--; }

				if(!this.world.GetBody(0, out FlatBody body))
				{
					throw new Exception("Body not found at the specified index");
				}

				if (dx != 0f || dy != 0f)
				{
					FlatVector forceDirection = FlatMath.Normalize( new FlatVector(dx, dy));
					FlatVector force = forceDirection * forceMagnitude;
					body.AddForce(force);
				}

				if (keyboard.IsKeyDown(Keys.R))
				{
					body.Rotate(MathF.PI / 2f * FlatUtil.GetElapsedTimeInSeconds(gameTime));
				}
#endif

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
			
			for (int i = 0; i < this._world.BodyCount; i++)
			{
				if (!this._world.GetBody(i, out FlatBody body))
				{
					throw new ArgumentException();
				}
				
				FlatAABB box = body.GetAABB();

				if (box._max._Y < viewBottom)
				{
					this._world.RemoveBody(body);
					this._colors.RemoveAt(i);
					this._outlineColors.RemoveAt(i);
				}
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this._screen.Set();
			this.GraphicsDevice.Clear(new Color(50, 60, 70));

			

			this._shapes.Begin(this._camera);

			for (int i = 0; i < this._world.BodyCount; i++)
			{
				if (!this._world.GetBody(i, out FlatBody body))
				{
					throw new Exception("Body not found at the specified index");
				}

				Vector2 position = FlatConverter.ToVector2(body.Position);

				if (body.shapeType is ShapeType.Circle)
				{
					_shapes.DrawCircleFill(position, body._radius, 26, _colors[i]);
					_shapes.DrawCircle(position, body._radius, 26, this._outlineColors[i]);
				}
				else if (body.shapeType is ShapeType.Box)
				{
					FlatConverter.ToVector2Array(body.GetTransformedVertices(), ref this._vertexBuffer);
					_shapes.DrawPolygonFill(this._vertexBuffer, body._triangles, this._colors[i]);
					_shapes.DrawPolygon(this._vertexBuffer, this._outlineColors[i]);
				}
				
			}

			List<FlatVector> contactPoints = this._world?._ContactPointsList;
			for (int i = 0; i < contactPoints.Count; i++)
			{
				_shapes.DrawBoxFill(FlatConverter.ToVector2(contactPoints[i]), 0.5f, 0.5f, Color.Orange);
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

		private void WrapScreen()
		{
			this._camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

			float viewWidth = camMax.X - camMin.X;
			float viewHeight = camMax.Y - camMin.Y;

			for (int i = 0; i < this._world.BodyCount; i++)
			{
				if (!this._world.GetBody(i, out FlatBody body))
				{
					throw new Exception();

				}

				if (body.Position._X < camMin.X) { body.MoveTo(body.Position + new FlatVector(viewWidth, 0f)); }
				if (body.Position._X > camMax.X) { body.MoveTo(body.Position - new FlatVector(viewWidth, 0f)); }
				if (body.Position._Y < camMin.Y) { body.MoveTo(body.Position + new FlatVector(0f, viewHeight)); }
				if (body.Position._Y > camMax.Y) { body.MoveTo(body.Position - new FlatVector(0f, viewHeight)); }

			}
		}
	}
}
