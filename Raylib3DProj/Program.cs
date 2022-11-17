using System.Numerics;
using Raylib_cs;
using Raylib3DProj;

public static unsafe class Program
{
	private const float mouseSpeed = 0.01f;
	private const float playerSpeed = 1f;
	private const float staminaRate = 0.2f;

	private const int itemSlotCount = 5;

	public static Camera3D camera;
	public static List<Sprite> sprites;
	public static List<ItemPickup> itemPickups;
	public static List<Item?> items;
	public static int itemSelected;
	public static double stamina;
	
	public static void Main(string[] args)
	{
		#region Initialization
		
		var screenWidth = 640;
		var screenHeight = 480;

		Raylib.InitWindow(screenWidth, screenHeight, 
			"Baldi's Basics Raylib Port (not official)");
		
		Raylib.SetExitKey(KeyboardKey.KEY_DELETE);
		Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);
		Raylib.SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
		Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		
		camera = new Camera3D(new Vector3(1f, 0.5f, 1f), 
			new Vector3(2f, 0.5f, 2f), 
			new Vector3(0f, 1f, 0f), 
			60f, 
			CameraProjection.CAMERA_PERSPECTIVE);

		stamina = 1f;
		sprites = new List<Sprite>();
		itemPickups = new List<ItemPickup>();
		items = new List<Item?>();
		for (var i = 0; i < itemSlotCount; i++)
		{
			items.Add(null);
		}
		itemSelected = 0;

		Raylib.DisableCursor();

		#endregion

		#region Texture Loading

		var atlas = Raylib.LoadTexture("resources/atlas.png");
		var billboard = Raylib.LoadTexture("resources/billboard.png");
		var itemTexture = Raylib.LoadTexture("resources/item.png");
		var cursor = Raylib.LoadTexture("resources/cursor.png");

		var imMap = Raylib.LoadImage("resources/cubicmap.png");

		#endregion

		#region Map Loading

		var cubicmap = Raylib.LoadTextureFromImage(imMap);
		var mesh = Raylib.GenMeshCubicmap(imMap, Vector3.One);
		var model = Raylib.LoadModelFromMesh(mesh);

		model.materials[0].maps[(int)MaterialMapIndex.MATERIAL_MAP_DIFFUSE].texture = atlas;
		
		var mapPixels = Raylib.LoadImageColors(imMap);
		Raylib.UnloadImage(imMap);

		var mapPosition = new Vector3(0,0f,0);
		
		sprites.Add(new Sprite(billboard, new Vector3(5f, 0.5f,5f)));

		var zestyBar = new Item
		{
			itemBehaviour = new ZestyBar(),
			texture = itemTexture,
			name = "Energy Flavored Zesty Bar"
		};
		
		itemPickups.Add(new ItemPickup
		{
			item = zestyBar,
			position = new Vector3(3,0.5f,3)
		});
		
		#endregion

		while (!Raylib.WindowShouldClose())
		{
			#region Rotation

			camera.target = Raymath.Vector3RotateByAxisAngle(camera.target - camera.position,
				camera.up,  -Raylib.GetMouseDelta().X * mouseSpeed) + camera.position;

			#endregion
			
			#region Movement

			var direction = Vector3.Zero;

			if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
			{
				direction += 
					Raymath.Vector3RotateByAxisAngle(camera.target - camera.position, 
						camera.up, 0 * Raylib.DEG2RAD);
			}
			if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
			{
				direction += 
					Raymath.Vector3RotateByAxisAngle(camera.target - camera.position, 
						camera.up, 90 * Raylib.DEG2RAD);
			}
			if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
			{
				direction += 
					Raymath.Vector3RotateByAxisAngle(camera.target - camera.position, 
						camera.up, 180 * Raylib.DEG2RAD);
			}
			if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
			{
				direction += 
					Raymath.Vector3RotateByAxisAngle(camera.target - camera.position, 
						camera.up, 270 * Raylib.DEG2RAD);
			}

			direction = Raymath.Vector3Normalize(direction);

			if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && stamina > 0 
			                                                 && direction.Length() > 0)
			{
				direction *= 2;
				stamina -= Raylib.GetFrameTime() * staminaRate;
			}
			if (stamina < 1 && direction.Length() == 0)
			{
				stamina += Raylib.GetFrameTime() * staminaRate;
			}

			direction *= Raylib.GetFrameTime() * playerSpeed;

			var oldCameraPos = camera.position;
			var oldCameraTarget = camera.target;

			camera.position += direction;
			camera.target += direction;

			#endregion
			
			#region Collision
			
			var playerPos = new Vector2(camera.position.X, camera.position.Z);
			var playerRadius = 0.1f; 
			
			for (var y = 0; y < cubicmap.height; y++)
			{
				for (var x = 0; x < cubicmap.width; x++)
				{
					if ((mapPixels[y*cubicmap.width + x].r == 255) &&
					    (Raylib.CheckCollisionCircleRec(playerPos, playerRadius,
						    new Rectangle (-0.5f + x*1.0f, 
							    -0.5f + y*1.0f, 1.0f, 1.0f))))
					{
						camera.position = oldCameraPos;
						camera.target = oldCameraTarget;
					}
				}
			}

			#endregion

			#region Drawing

			Raylib.BeginDrawing();

				Raylib.BeginMode3D(camera);

					Raylib.ClearBackground(Color.WHITE);
					Raylib.DrawModel(model, Vector3.Zero, 1, Color.WHITE);

					foreach (var sprite in sprites)
					{
						Raylib.DrawBillboard(camera, sprite.texture, sprite.position, 1, Color.WHITE);
					}
					
					foreach (var itemPickup in itemPickups)
					{
						Raylib.DrawBillboard(camera, itemPickup.texture, 
							itemPickup.position, 0.1f, Color.WHITE);
					}

				Raylib.EndMode3D();
				
				Raylib.DrawText(Raylib.GetFPS().ToString() + " FPS", 5, 5, 30, 
					Color.DARKGRAY);
				
				// Staminometer	
				Raylib.DrawRectangle
				(25, screenHeight - (int)(screenHeight * 0.025f) - 25, 
					(int)(screenWidth * 0.2f), (int)(screenHeight * 0.025f), 
					Color.RED);

				Raylib.DrawRectangle
				(25, screenHeight - (int)(screenHeight * 0.025f) - 25, 
					(int)(screenWidth * 0.2f * Raymath.Clamp((float)stamina, 0, 1)), 
					(int)(screenHeight * 0.025f),
					Color.GREEN);

				var flag = false;

				foreach (var itemPickup in itemPickups)
				{
					var raycast = Raylib.GetRayCollisionSphere(
						new Ray(camera.position, 
							Raymath.Vector3Normalize(camera.target - camera.position)),
						itemPickup.position, 0.1f);

					if (raycast.hit && raycast.distance < 1f)
					{
						flag = true;
					}
				}
				
				if(flag)
					Raylib.DrawTexture(cursor, screenWidth/2, screenHeight/2, 
						Color.WHITE);

				// Item Slots
				for (var i = 0; i < items.Count; i++)
				{
					var item = items[i];

					var rect = new Rectangle(screenWidth - (screenHeight * 0.05f) -
					                         (screenHeight * 0.05f + 5) * (items.Count - i - 1) - 5, 5,
											screenHeight * 0.05f, screenHeight * 0.05f);
					
					Raylib.DrawRectangle((int)rect.x, (int)rect.y, (int)rect.width, 
						(int)rect.height, i == itemSelected ? Color.RED : Color.WHITE);
					if (item == null) continue;
					
					Raylib.DrawTexturePro(item.texture, 
						new Rectangle(0,0, itemTexture.width, itemTexture.height), 
						rect,
						Vector2.Zero, 
						5,
						Color.WHITE);
				}
				
				if(items[itemSelected] != null)
					Raylib.DrawText(items[itemSelected].name, 
						(int)(screenWidth - items[itemSelected].name.Length * 8f * 
							(screenHeight / 480)), 
						(int)(screenHeight * 0.05f + 15), 14 * (screenHeight / 480), 
						Color.DARKGRAY);

			Raylib.EndDrawing(); 

			#endregion

			#region Input

			if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
			{
				if(Raylib.IsCursorHidden())
					Raylib.EnableCursor();
				else
					Raylib.DisableCursor();
			}

			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
			{
				foreach (var itemPickup in itemPickups)
				{
					var raycast = Raylib.GetRayCollisionSphere(
						new Ray(camera.position, 
							Raymath.Vector3Normalize(camera.target - camera.position)),
						itemPickup.position, 0.1f);

					if (raycast.hit && raycast.distance < 1f)
					{
						if(items[itemSelected] == null)
						{
							items[itemSelected] = itemPickup.item;
							itemPickups.Remove(itemPickup);
							break;
						}

						var flag2 = false;
						for (var i = 0; i < items.Count; i++)
						{
							if (items[i] != null) continue;
							
							items[i] = itemPickup.item;
							flag2 = true;
							break;
						}
        
						if(!flag2)
							items[itemSelected] = itemPickup.item;
						
						itemPickups.Remove(itemPickup);
						break;
					}
				}
			}
			if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
			{
				if (items[itemSelected] != null)
				{
					if (items[itemSelected].itemBehaviour.Use())
						items[itemSelected] = null;
				}
			}

			if (Raylib.GetMouseWheelMove() < 0)
			{
				itemSelected++;
				itemSelected = itemSelected == items.Count ? 0 : itemSelected;
			}
			if (Raylib.GetMouseWheelMove() > 0)
			{
				itemSelected--;
				itemSelected = itemSelected < 0 ? items.Count - 1 : itemSelected;
			}

			#endregion

			if (Raylib.IsWindowResized())
			{
				screenWidth = Raylib.GetScreenWidth();
				screenHeight = Raylib.GetScreenHeight();
			}
		}

		Raylib.CloseWindow();
	}
}