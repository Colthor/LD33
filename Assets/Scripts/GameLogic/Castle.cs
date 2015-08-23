﻿using UnityEngine;
using System.Collections;
//using Person;


namespace mjc_ld33
{

	public class Castle : MonoBehaviour
	{
		const int CASTLE_MAX_STRENGTH = 16; //Castles over this strength will use the highest value sprite.
		public GameScript controller = null;
		public Person liege = null;
		static Sprite[] CastleSprites = null;
		float StrengthPerSprite = 1.0f;

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
		
		}
		
		// Update is called once per frame
		void Update ()
		{
			int spriteIndex = (int)(Morale() * (float)troops)/4;
			if (spriteIndex > 3) spriteIndex = 3;
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
			//if(controller.CastleIsSelected(this))
			{
				if(null!=liege) liege.DrawConnections();
			}
			
			//float scale = 0.25f + 0.075f * Morale() * (float) troops;
			//transform.localScale = new Vector3(scale, scale, 1);

		
		}
		void OnGUI()
		{
			Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-0.2f, -0.15f, 0.0f));
			string labelText = troops + "/" + max_troops + " troops\n" + (int)(Morale()*100f) + "% morale";
			if(null != liege)
			{
				labelText = liege.GetName() + "\n" + labelText;
			}
			else
			{
				labelText = "Abandoned";
			}
			GUI.Label(new Rect(pos.x, Screen.height-pos.y ,300,300), labelText);

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

		//Returns true if we won, false if we didn't.
		public bool Attack(Castle target)
		{
			float my_strength = (float) troops * Morale();
			float target_strength = (float) target.troops * target.Morale();
			bool won = my_strength > target_strength;

			if(won)
			{
				target.liege.Kill();
				target.troops = target.max_troops;
				this.troops -= (int)target_strength;
				target.liege = controller.GetNewLiege(liege.GetDynasty());
				if(null != target.liege) target.liege.holding = target;
			}
			else
			{
				this.liege.Kill();
				this.liege = null;
				this.troops = 0;
				target.troops -= (int)my_strength;
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
