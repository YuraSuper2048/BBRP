using System.Numerics;
using Raylib_cs;

namespace Raylib3DProj;

public struct ItemPickup
{
	public Vector3 position;
	public Item item;
	public Texture2D texture => item.texture;
}