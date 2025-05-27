using FlatLibraryFor2D;
using FlatLibraryFor2D.Graphics;
using FlatPhysicsEngineFor2D;
using Microsoft.Xna.Framework;
using System;

namespace FlatGameFor2D
{
	public sealed class FlatEntity
	{
		public readonly FlatBody _body;
		public readonly Color _color;

		public FlatEntity(FlatBody body)
		{
			this._body = body;
			this._color = RandomHelper.RandomColor();
		}

		public FlatEntity(FlatBody body, Color color)
		{
			this._body = body;
			this._color = color;
		}

		public FlatEntity(FlatWorld world, float radius, bool isStatic, FlatVector position) 
		{
			if (!FlatBody.CreateCircleBody(radius, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
			{
				throw new Exception(errorMessage);
			}
			body.MoveTo(position);
			this._body = body;
			world.AddBody(body);
			this._color = RandomHelper.RandomColor();
		}

		public FlatEntity(FlatWorld world, float width, float height, bool isStatic, FlatVector position)
		{
			if (!FlatBody.CreateBoxBody(width, height, 1f, isStatic, 0.5f, out FlatBody body, out string errorMessage))
			{
				throw new Exception(errorMessage);
			}
			body.MoveTo(position);
			this._body = body;
			world.AddBody(body);
			this._color = RandomHelper.RandomColor();
		}

		public void Draw(Shapes shapes)
		{
			Vector2 position = FlatConverter.ToVector2(this._body.Position);

			if (this._body._shapeType is ShapeType.Circle)
			{
				shapes.DrawCircleFill(position, this._body._radius, 26, this._color);
				shapes.DrawCircle(position, this._body._radius, 26, Color.White);
			}
			else if (this._body._shapeType is ShapeType.Box)
			{
				shapes.DrawBoxFill(position, this._body._width, this._body._height, this._body.Angle, this._color);
				shapes.DrawBox(position, this._body._width, this._body._height, this._body.Angle, Color.White);
			}
		}
	}
}
