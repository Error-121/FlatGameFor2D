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
		private GraphicsDeviceManager graphics;
		private Screen screen;
		private Sprites sprites;
		private Shapes shapes;
		private Camera camera;

		private FlatWorld world;

		private List<Color> colors;
		private List<Color> outlineColors;

		private Vector2[] vertexBuffer;


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
			this.camera.Zoom = 24;

			this.camera.GetExtents(out float left, out float right, out float top, out float bottom);
			
			
			
			this.colors = new List<Color>();
			this.outlineColors = new List<Color>();

			this.world = new FlatWorld();
			
			float padding = MathF.Abs(right - left) * 0.10f;
			
			
			if (!FlatBody.CreateBoxBody(right - left - padding * 2, 3f, new FlatVector(0, -10), 1f, true, 0.5f, out FlatBody groundBody, out string errorMessage))
			{
				throw new Exception(errorMessage);
			}

			this.world.AddBody(groundBody);

			this.colors.Add(Color.DarkGreen);
			this.outlineColors.Add(Color.White);

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

			// add box body
			if (mouse.IsLeftMouseButtonPressed())
			{
				float width = RandomHelper.RandomSingle(1f, 2f);
				float height = RandomHelper.RandomSingle(1f, 2f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

				if(!FlatBody.CreateBoxBody(width, height, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMessage))
				{
					throw new Exception(errorMessage);
				}

				this.world.AddBody(body);
				this.colors.Add(RandomHelper.RandomColor());
				this.outlineColors.Add(Color.White);
			}
			// add circle body
			if (mouse.IsRightMouseButtonPressed())
			{
				float radius = RandomHelper.RandomSingle(0.75f, 1.25f);

				FlatVector mouseWorldPosition = FlatConverter.ToFlatVector(mouse.GetMouseWorldPosition(this, this.screen, this.camera));

				if (!FlatBody.CreateCircleBody(radius, mouseWorldPosition, 2f, false, 0.6f, out FlatBody body, out string errorMessage))
				{
					throw new Exception(errorMessage);
				}

				this.world.AddBody(body);
				this.colors.Add(RandomHelper.RandomColor());
				this.outlineColors.Add(Color.White);
			}

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

			this.world.Step(FlatUtil.GetElapsedTimeInSeconds(gameTime));

			

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this.screen.Set();
			this.GraphicsDevice.Clear(new Color(50, 60, 70));

			

			this.shapes.Begin(this.camera);

			for (int i = 0; i < this.world.BodyCount; i++)
			{
				if (!this.world.GetBody(i, out FlatBody body))
				{
					throw new Exception("Body not found at the specified index");
				}

				Vector2 position = FlatConverter.ToVector2(body.Position);

				if (body.shapeType is ShapeType.Circle)
				{
					shapes.DrawCircleFill(position, body.radius, 26, colors[i]);
					shapes.DrawCircle(position, body.radius, 26, this.outlineColors[i]);
				}
				else if (body.shapeType is ShapeType.Box)
				{
					FlatConverter.ToVector2Array(body.GetTransformedVertices(), ref this.vertexBuffer);
					shapes.DrawPolygonFill(this.vertexBuffer, body.triangles, this.colors[i]);
					shapes.DrawPolygon(this.vertexBuffer, this.outlineColors[i]);
				}
				
			}

			this.shapes.End();

			this.screen.Unset();
			this.screen.Present(this.sprites);

			base.Draw(gameTime);
		}

		private void WrapScreen()
		{
			this.camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

			float viewWidth = camMax.X - camMin.X;
			float viewHeight = camMax.Y - camMin.Y;

			for (int i = 0; i < this.world.BodyCount; i++)
			{
				if (!this.world.GetBody(i, out FlatBody body))
				{
					throw new Exception();

				}

				if (body.Position.X < camMin.X) { body.MoveTo(body.Position + new FlatVector(viewWidth, 0f)); }
				if (body.Position.X > camMax.X) { body.MoveTo(body.Position - new FlatVector(viewWidth, 0f)); }
				if (body.Position.Y < camMin.Y) { body.MoveTo(body.Position + new FlatVector(0f, viewHeight)); }
				if (body.Position.Y > camMax.Y) { body.MoveTo(body.Position - new FlatVector(0f, viewHeight)); }

			}
		}
	}
}
