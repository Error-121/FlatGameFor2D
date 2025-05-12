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

		private List<FlatBody> bodyList;
		private Color[] colors;

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
			
			
			int bodyCount = 10;
			float padding = MathF.Abs(right - left) * 0.05f;

			this.bodyList = new List<FlatBody>(bodyCount);
			this.colors = new Color[bodyCount];

			for (int i = 0; i < bodyCount; i++)
			{
				int type = RandomHelper.RandomInteger(0, 2);
				type = (int)ShapeType.Circle; // TODO: remove this line to use random shape type

				FlatBody body = null;

				float x = RandomHelper.RandomSingle(left + padding, right - padding);
				float y = RandomHelper.RandomSingle(top - padding, bottom + padding);


				if (type == (int)ShapeType.Circle)
				{
					if (!FlatBody.CreateCircleBody(new FlatVector(x, y), 2f, 0.5f, false, 1f, out body, out string errorMessage))
					{
						throw new Exception();
					}
				}
				else if (type == (int)ShapeType.Box)
				{
					if (!FlatBody.CreateBoxBody(new FlatVector(x, y), 2f, 0.5f, false, 1f, 1f, out body, out string errorMessage))
					{
						throw new Exception();
					}
				}
				else 
				{
					throw new Exception("Unknown shape type");
				}

				this.bodyList.Add(body);
				this.colors[i] = RandomHelper.RandomColor();

			}

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

				float dx = 0f;
				float dy = 0f;
				float speed = 8f;

				if (keyboard.IsKeyDown(Keys.Left)) { dx --; }
				if (keyboard.IsKeyDown(Keys.Right)) { dx++; }
				if (keyboard.IsKeyDown(Keys.Up)) { dy++; }
				if (keyboard.IsKeyDown(Keys.Down)) { dy--; }

				if (dx != 0f || dy != 0f)
				{
					FlatVector direction = FlatMath.Normalize( new FlatVector(dx, dy));
					FlatVector velocity = direction * speed * FlatUtil.GetElapsedTimeInSeconds(gameTime);
					this.bodyList[0].Move(velocity);
				}
			}

			for (int i = 0; i < this.bodyList.Count -1 ; i++)
			{
				FlatBody bodyA = this.bodyList[i];

				for (int j = i + 1; j < this.bodyList.Count; j++)
				{
					FlatBody bodyB = this.bodyList[j];

					if (Collisions.IntersectCircles(bodyA.Position, bodyA.radius, bodyB.Position, bodyB.radius, out FlatVector normal, out float depth)) 
					{
						bodyA.Move(-normal * depth / 2f);
						bodyB.Move(normal * depth / 2f);
					}
				}
			}



			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this.screen.Set();
			this.GraphicsDevice.Clear(new Color(50, 60, 70));

			

			this.shapes.Begin(this.camera);

			for (int i = 0; i < bodyList.Count; i++)
			{
				FlatBody body = bodyList[i];
				Vector2 position = FlatConverter.ToVector2(body.Position);

				if (body.shapeType is ShapeType.Circle)
				{
					shapes.DrawCircleFill(position, body.radius, 26, colors[i]);
					shapes.DrawCircle(position, body.radius, 26, Color.Red);
				}
				else if (body.shapeType is ShapeType.Box)
				{
					this.shapes.DrawBox(position, body.width, body.height, 26, Color.Blue);
				}
				
			}

			this.shapes.End();

			this.screen.Unset();
			this.screen.Present(this.sprites);

			base.Draw(gameTime);
		}
	}
}
