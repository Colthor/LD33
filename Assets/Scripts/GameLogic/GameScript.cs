using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace mjc_ld33
{

	[RequireComponent (typeof (ParticleSystem))]
	public class GameScript : MonoBehaviour
	{
		enum GameStates {NOTPLAYING, PLAYING, VICTORY, DEFEAT};
		
		DynastyGen dg = null;
		int[] ai_dynasties = null;
		int player_dynasty = 0;
		ParticleSystem particleSys = null;
		Menu mainMenu;
		Menu escapeMenu;
		
		Sprite[] BannerSprites = null;
		GUISkin uiSkin = null;

		GameStates currentState = GameStates.NOTPLAYING;

		public GUISkin GetUISkin()
		{
			return uiSkin;
		}

		void ClearUpGame()
		{
			currentState = GameStates.NOTPLAYING;
			ai_dynasties = null;
			player_dynasty = 0;
			foreach (KeyValuePair<int, List<Person>> dynPair in dg.dynastiesGenerated)
			{
				foreach(Person p in dynPair.Value)
				{
					p.ClearUp();
				}
				dynPair.Value.Clear();
			}
			dg.dynastiesGenerated.Clear();
			dg = null;

			GameObject[] castles;
			castles = GameObject.FindGameObjectsWithTag("Castle");
			foreach(GameObject c in castles)
			{
				Destroy(c);
			}

		}

		void ExitGame()
		{
			Application.Quit();
		}

		void MainMenuDoStart()
		{
			mainMenu.Disable();
			StartGame();
		}

		void CloseEscapeMenu()
		{
			escapeMenu.Disable();
			ResetCameraPos();
		}

		void RestartGame()
		{
			escapeMenu.Disable();
			ClearUpGame();
			StartGame();
		}

		void InitMenus()
		{
			mainMenu = this.gameObject.AddComponent<Menu>();
			mainMenu.MenuTop = 500f;
			mainMenu.AddButtonItem("Start Game", MainMenuDoStart);
			
			escapeMenu = this.gameObject.AddComponent<Menu>();
			escapeMenu.AddButtonItem("Resume", CloseEscapeMenu);
			escapeMenu.AddButtonItem("Restart", RestartGame);

#if UNITY_STANDALONE
			mainMenu.AddButtonItem("Quit", ExitGame);
			escapeMenu.AddButtonItem("Quit", ExitGame);
#endif
			mainMenu.GuiSkin = uiSkin;
			escapeMenu.GuiSkin = uiSkin;
		}

		void ResetCameraPos()
		{
			
			Vector3 camPos = Camera.main.transform.position;
			camPos.x = 0f;
			Camera.main.transform.position = camPos;
		}

		// Use this for initialization
		void Start ()
		{
			particleSys = GetComponent<ParticleSystem>();
			
			BannerSprites = Resources.LoadAll<Sprite>("Graphics/Banners");
			uiSkin = Resources.Load<GUISkin>("UI/LD33");

			currentState = GameStates.NOTPLAYING;

			InitMenus();

			mainMenu.Enable();

			//StartGame();

		}
		
		// Update is called once per frame
		void Update ()
		{
			if(Input.GetKeyDown(KeyCode.F10)) currentState = GameStates.VICTORY;

			if(GameStates.NOTPLAYING != currentState && Input.GetKeyDown(KeyCode.Escape))
			{
				escapeMenu.Enable();
				
				Vector3 camPos = Camera.main.transform.position;
				camPos.x -= 5000f;
				Camera.main.transform.position = camPos;
			}
		
			if (GameStates.PLAYING == currentState)
			{
				DetectEndCondition();
				DrawSelectionIndicator();
			}

			if (GameStates.NOTPLAYING != currentState)
			{
				DetectEndCondition();
				DrawSelectionIndicator();
				GetComponent<SpriteRenderer>().sprite = GetBanner(player_dynasty);
			}
			else
			{
				GetComponent<SpriteRenderer>().sprite = null;
			}
		}

		private int CountPlayerUnlandedFamily()
		{
			int count = 0;
			foreach(Person p in dg.dynastiesGenerated[player_dynasty])
			{
				if(p.IsAlive() && null == p.holding) count++;
			}
			return count;
		}

		void OnGUI()
		{
			GUI.skin = uiSkin;
			GUIStyle bigfont = new GUIStyle();
			bigfont.fontSize = 200;

			switch(currentState)
			{
			case GameStates.DEFEAT:
				GUI.Label(new Rect(45, 5, 1000, 100), "Defeated!");
				GUI.Label(new Rect(800, 5, 1000, 100), "ESC for menu");
				break;
			case GameStates.VICTORY:
				GUI.Label(new Rect(45, 5, 1000, 100), "Victorious! The land belongs to the " + dg.dynastyNames[player_dynasty] + "s once more!");
				//GUI.Label(new Rect(800, 5, 1000, 100), "ESC for menu");
				break;
			case GameStates.PLAYING:
				GUI.Label(new Rect(45, 5, 1000, 100), "Unlanded " + dg.dynastyNames[player_dynasty] + "s: " + CountPlayerUnlandedFamily());
				
				GUI.Label(new Rect(600, 5, 1000, 100), "<color=red>Spouse</color> <color=blue>Sibling</color> <color=yellow>Child</color> <color=magenta>Parent</color>");
				break;
			case GameStates.NOTPLAYING:
				GUI.Label(new Rect(45, 45, 1000, 100), "Dienasties", bigfont);
				GUI.Label(new Rect(45, 250, 1000, 100), "Conquer the land that is rightfully yours!");
				GUI.Label(new Rect(45, 300, 1000, 100), "Your enemies are strong, but maybe family is a weakness?");
				GUI.Label(new Rect(45, 400, 1000, 100), "Left click selects, right click attacks. Use arrows and space in menus.");

				break;
			}
		}

		void StartGame()
		{
			currentState = GameStates.PLAYING;
			ResetCameraPos();
			int CASTLES_ACROSS = 5;
			int CASTLES_DOWN = 4;

			dg = new DynastyGen(BannerSprites.GetUpperBound(0)+1);
			
			ai_dynasties = dg.GenerateIntertwinedDynasties(3);
			player_dynasty = dg.GenerateIntertwinedDynasties(1)[0];
			
			string dynDebug = "";
			foreach (KeyValuePair<int, List<Person>> dynPair in dg.dynastiesGenerated)
			{
				foreach(Person p in dynPair.Value)
				{
					dynDebug += p.PrintRelationships() + "\n";
				}
			}
			Debug.Log(dynDebug);
			
			for(int x = 0; x < CASTLES_ACROSS; x++)
			{
				for(int y = 0; y < CASTLES_DOWN; y++)
				{
					const float PIXEL_SIZE = 1f/16f;
					Vector2 uc = Random.insideUnitCircle*0.2f;
					Vector3 pos = new Vector3(x - (float)(CASTLES_ACROSS-1)/2f + uc.x, y - (float)(CASTLES_DOWN-1)/2f + uc.y, 0);
					pos.x = pos.x * 1.6f - 0.15f;
					pos.y *= 1.4f;
					pos.x -= pos.x%PIXEL_SIZE;
					pos.y -= pos.y%PIXEL_SIZE;
					GameObject gOb = (GameObject)Instantiate(Resources.Load("Prefabs/Castle_prefab"), pos, Quaternion.identity);
					gOb.name = "Castle ( " + x + ", " + y + ")";
					Castle newCastle= gOb.GetComponent<Castle>();
					newCastle.controller = this;
					if(x == CASTLES_ACROSS -1 && y == CASTLES_DOWN -1)
					{
						//Player castle
						Person p = dg.GetUnlandedMember(player_dynasty);
						Debug.Assert(p != null);
						p.holding = newCastle;
						newCastle.liege = p;
						
					}
					else
					{
						int dynasty = Random.Range(0, ai_dynasties.GetUpperBound(0)+1);
						int count = 0;
						Person p = null;
						while(null == p && count <= ai_dynasties.GetUpperBound(0))
						{
							int dynIndex = (dynasty + count) % (ai_dynasties.GetUpperBound(0)+1);
							p = dg.GetUnlandedMember(ai_dynasties[dynIndex]);
							if(null != p)
							{
								newCastle.max_troops = Random.Range(7 - p.GetRank(), 18 - 2*p.GetRank());
								newCastle.troops = newCastle.max_troops;
								newCastle.liege = p;
								p.holding = newCastle;
								Debug.Log("Adding castle(" + x + ", " + y + ") to person " + p.GetName());
							}
							else
							{
								
								Debug.Log("Dynasty " + dynIndex + " has ran out of unlanded members.");
							}
							count++;
						}
						if(null == p) Destroy(gOb);
					}
				}
			}
		}


		private void DetectEndCondition()
		{
			int player_lieges = 0;
			int ai_lieges = 0;

			GameObject[] castles;
			castles = GameObject.FindGameObjectsWithTag("Castle");
			foreach(GameObject go in castles)
			{
				Castle c = go.GetComponent<Castle>();
				if(null != c.liege && c.liege.IsAlive())
				{
					if(c.liege.GetDynasty() == player_dynasty)
					{
						player_lieges++;
					}
					else
					{
						ai_lieges++;
					}
				}
			}
			
			if( 0 == player_lieges)
			{
				//Defeat!
				Debug.Log("Defeat!");
				currentState = GameStates.DEFEAT;
			}
			else if(0 == ai_lieges)
			{
				//Victory!
				Debug.Log("Victory!");
				currentState = GameStates.VICTORY;
			}
		}


		private Castle selectedCastle = null;

		public bool CastleIsSelected(Castle c)
		{
			return c == selectedCastle;
		}

		public void SetLeftClick(Castle c)
		{
			if(escapeMenu.IsEnabled()) return;
			selectedCastle = c;
		}

		public void SetRightClick(Castle c)
		{
			if(escapeMenu.IsEnabled()) return;
			if(null != selectedCastle && null != selectedCastle.liege && selectedCastle.liege.GetDynasty() == player_dynasty)
			{
				if(null == c.liege || c.liege.GetDynasty() != player_dynasty) selectedCastle.Attack(c);
			}
		}
		
		public Person GetNewLiege(int dynasty)
		{
			return dg.GetUnlandedMember(dynasty);
		}

		public Sprite GetBanner(int dynID)
		{
			return BannerSprites[dg.dynastyFlags[dynID]];
		}

		
		private void DrawSelectionIndicator()
		{
			if(null != selectedCastle)
			{
				const float CIRCLE_SEGS = 5.0f;
				const float CIRCLE_RADIUS = 0.25f;
				const float ROTATE_SPEED = 2f;
				float radsPerSeg = 2f * Mathf.PI/CIRCLE_SEGS;
				Vector3 centre = selectedCastle.transform.position;

				for(float angle = 0f; angle < Mathf.PI*2f; angle += radsPerSeg)
				{
					float totAng = Time.time*ROTATE_SPEED + angle;
					Vector3 offset = new Vector3(Mathf.Cos(totAng), Mathf.Sin(totAng), 0f)*CIRCLE_RADIUS;
					particleSys.Emit(centre+offset, Vector3.zero, 0.0625f, 0.1f, Color.white);

				}

			}
		}

		public void DrawLine(Vector3 start, Vector3 end, Color col, float strength, float thickness)
		{
			const float PARTICLES_PER_STRENGTH_PER_LENGTH=10f;
			float PARTICLE_SPEED = 0.5f * (int)strength;
			Vector3 dir = end-start;
			float length = dir.magnitude;
			Vector3 dirNorm = dir/length;
			Vector3 perpendicular = new Vector3(-dirNorm.y, dirNorm.x, 0);
			col.a = strength;
			float step = 1.0f/(PARTICLES_PER_STRENGTH_PER_LENGTH);
			for(float dist = 0; dist <= length - PARTICLE_SPEED; dist += step)
			{
				Vector3 pos = start + dirNorm * (dist + Random.Range(0f, step)) + perpendicular * Random.Range(-thickness*0.5f, thickness*0.5f);
				Vector3 vel = PARTICLE_SPEED * dirNorm;
				pos.z = 10f;
				particleSys.Emit(pos, vel, 0.0625f*strength, 1.0f, col);
			}

		}

		public void DrawAttack(Vector3 start, Vector3 end, Color col)
		{
			const float PARTICLES_PER_UNIT=50f;
			float PARTICLE_SPEED = 0.5f ;
			Vector3 dir = end-start;
			float length = dir.magnitude;
			Vector3 dirNorm = dir/length;
			Vector3 perpendicular = new Vector3(-dirNorm.y, dirNorm.x, 0);
			float step = 1.0f/(PARTICLES_PER_UNIT);
			for(float dist = 0; dist <= length; dist += step)
			{
				float scale = dist/length;
				Vector3 pos = start + dirNorm * dist;
				Vector3 vel = PARTICLE_SPEED * perpendicular * (2.5f-2f*scale) * Random.Range(0.5f, 1.0f);
				if(Random.Range(0f, 1.0f) < 0.5f) vel *= -1f;
				pos.z = 10f;
				particleSys.Emit(pos, vel, 0.0625f, .1f+.3f*scale, col);
			}
			
		}

	}//class
}//namespace
