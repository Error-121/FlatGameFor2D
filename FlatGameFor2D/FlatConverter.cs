using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using FlatPhysicsEngineFor2D;

namespace FlatGameFor2D
{
	public static class FlatConverter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ToVector2(FlatVector vector)
		{
			return new Vector2(vector.X, vector.Y);
		}
	}
}
