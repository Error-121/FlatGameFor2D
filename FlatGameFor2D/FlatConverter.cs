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
			return new Vector2(vector._X, vector._Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FlatVector ToFlatVector(Vector2 vector)
		{
			return new FlatVector(vector.X, vector.Y);
		}

		public static void ToVector2Array(FlatVector[] src, ref Vector2[] dst) 
		{
			if (dst is null || src.Length != dst.Length) 
			{
				dst = new Vector2[src.Length];
			}

			for (int i = 0; i < src.Length; i++)
			{
				FlatVector vector = src[i];
				dst[i] = new Vector2(vector._X, vector._Y);
			}

		}
			
	}


}
