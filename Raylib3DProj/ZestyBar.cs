namespace Raylib3DProj;

public class ZestyBar : ItemBehaviour
{
	public override bool Use()
	{
		Program.stamina = 2;
		return true;
	}
}