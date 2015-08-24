using UnityEngine;
using System.Collections;
//using Person;


namespace mjc_ld33
{

	public class Castle : MonoBehaviour
	{
		const int CASTLE_MAX_STRENGTH = 15; //Castles over this strength will use the highest value sprite.
		
		static Sprite[] CastleSprites = null;
		static float StrengthPerSprite = 1.0f;

		static GUIStyle fontStyle = null;


		public GameScript controller = null;
		public Person liege = null;

		public int troops;
		public int max_troops;
		private bool mouse_over = false;

		float emit_rate = .25f;
		float emit_time;
		bool emit_this_frame = false;

		// Use this for initialization
		void Start ()
		{
			emit_time = Time.time;
			if(null == CastleSprites)
			{
				CastleSprites = Resources.LoadAll<Sprite>("Graphics/CastleStates");
				StrengthPerSprite = (float)CASTLE_MAX_STRENGTH/(float)(CastleSprites.GetUpperBound(0)+1);
			}
			if(null==fontStyle)
			{
				fontStyle = new GUIStyle(controller.GetUISkin().label);
				fontStyle.fontSize = 22;
			}
		
		}
		
		// Update is called once per frame
		void Update ()
		{
			int spriteIndex = (int)(Morale() * (float)troops/StrengthPerSprite);
			if (spriteIndex > CastleSprites.GetUpperBound(0)) spriteIndex = CastleSprites.GetUpperBound(0);
			GetComponent<SpriteRenderer>().sprite = CastleSprites[spriteIndex];

			if(emit_time + emit_rate < Time.time)
			{
				emit_time = Time.time;
				emit_this_frame = true;
				//scale *= 2f;
			}
			else
			{
				emit_this_frame = false;
			}

			if(null!=liege)
			{
				liege.DrawConnections();
				transform.Find("Flag").GetComponent<SpriteRenderer>().sprite = controller.GetBanner(liege.GetDynasty());
			}
			else
			{
				transform.Find("Flag").GetComponent<SpriteRenderer>().sprite = null;
			}

		}


		void OnGUI()
		{
			GUI.skin = controller.GetUISkin();
			Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-0.35f, -0.15f, 0.0f));
			pos.y = Screen.height - pos.y;
			string labelText;
			if(null != liege)
			{
				float height = fontStyle.lineHeight - 7f;
				labelText = liege.GetName() + "\n";
				GUI.Label(new Rect(pos.x, pos.y ,300,300), labelText, fontStyle);
				labelText = troops + "/" + max_troops + " troops";
				GUI.Label(new Rect(pos.x, pos.y + height ,300,300), labelText, fontStyle);
				labelText =  (int)(Morale()*100f) + "% morale";
				GUI.Label(new Rect(pos.x, pos.y + 2f*height ,300,300), labelText, fontStyle);
			}
			else
			{
				labelText = "Abandoned";
				GUI.Label(new Rect(pos.x, pos.y ,300,300), labelText, fontStyle );
			}

		}
		
		void OnMouseOver()
		{
			//Debug.DrawLine(Vector3.zero, transform.position);
			//if(null!=liege) liege.DrawConnections();
			mouse_over = true;

			if(Input.GetMouseButtonDown(0))
			{ //Left click
				controller.SetLeftClick(this);
			}
			else if (Input.GetMouseButtonDown(1))
			{
				controller.SetRightClick(this);
			}
		}

		void OnMouseExit()
		{
			mouse_over = false;
		}

		public float Morale()
		{
			float morale = 0.0f;

			if (liege != null) morale = liege.Morale();

			return morale;
		}

		public float Strength()
		{
			return (float) troops * Morale();
		}

		//Returns true if we won, false if we didn't.
		public bool Attack(Castle target)
		{
			if(null == target.liege) return true;
			controller.DrawAttack(this.transform.position, target.transform.position, new Color(.75f, 0f, 0f));
			float my_strength = Strength();
			float target_strength = target.Strength();
			bool won = my_strength > target_strength;

			if(won)
			{
				target.liege.Kill();
				target.troops = target.max_troops;
				this.troops -= (int)(target_strength/Morale());
				target.liege = controller.GetNewLiege(liege.GetDynasty());
				if(null != target.liege) target.liege.holding = target;
			}
			else if(0 == target_strength)
			{ //everybody dies from supreme miserableness.
				target.liege.Kill();
				this.liege.Kill();
				target.liege = null;
				this.liege = null;
			}
			else
			{
				this.liege.Kill();
				this.liege = null;
				this.troops = 0;
				target.troops -= (int)(my_strength/target.Morale());
			}

			return won;
		}

		public void DrawConnectionTo(Castle target, Color col)
		{
			if(emit_this_frame)
			{
				float strength = 0.4f;
				if (controller.CastleIsSelected(this) || mouse_over) strength = 1.0f;
				controller.DrawLine(transform.position, target.transform.position, col, strength, 0.0f);
			}
		}

	}

} //namespace
