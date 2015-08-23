using UnityEngine;
using System.Collections;
//using Person;


namespace mjc_ld33
{

	public class Castle : MonoBehaviour
	{
		public GameScript controller = null;
		public Person liege = null;

		public int troops;
		public int max_troops;

		// Use this for initialization
		void Start ()
		{
		
		}
		
		// Update is called once per frame
		void Update ()
		{
			if(controller.CastleIsSelected(this))
			{
				if(null!=liege) liege.DrawConnections();
			}

			float scale = 0.1f + 0.09f * Morale() * (float) troops;
			transform.localScale = new Vector3(scale, scale, 1);
		
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
			if(null!=liege) liege.DrawConnections();

			if(Input.GetMouseButtonDown(0))
			{ //Left click
				controller.SetLeftClick(this);
			}
			else if (Input.GetMouseButtonDown(1))
			{
				controller.SetRightClick(this);
			}
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
				target.liege.holding = target;
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

	}

} //namespace
