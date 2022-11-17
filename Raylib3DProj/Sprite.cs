using System.Numerics;
using Raylib_cs;

namespace Raylib3DProj;

public struct Sprite
{
	public Sprite(Texture2D texture, Vector3 position)
	{
		this.texture = texture;
		this.position = position;
	}

	public Texture2D texture;
	public Vector3 position;
}