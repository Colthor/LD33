using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace mjc_ld33
{

	public class GameScript : MonoBehaviour {
		
		DynastyGen dg = new DynastyGen();
		int[] ai_dynasties = null;
		int player_dynasty = 0;
		// Use this for initialization
		void Start ()
		{
			int CASTLES_ACROSS = 5;
			int CASTLES_DOWN = 5;

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
					Vector2 uc = Random.insideUnitCircle*0.4f;
					Vector3 pos = new Vector3(x - (float)(CASTLES_ACROSS-1)/2f + uc.x, y - (float)(CASTLES_DOWN-1)/2f + uc.y, 0);
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
								newCastle.max_troops = Random.Range(4, 16);
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
		
		// Update is called once per frame
		void Update () {
		
		}

		private Castle selectedCastle = null;

		public bool CastleIsSelected(Castle c)
		{
			return c == selectedCastle;
		}

		public void SetLeftClick(Castle c)
		{
			selectedCastle = c;
		}

		public Person GetNewLiege(int dynasty)
		{
			return dg.GetUnlandedMember(dynasty);
		}

		public void SetRightClick(Castle c)
		{
			if(null != selectedCastle && null != selectedCastle.liege && selectedCastle.liege.GetDynasty() == player_dynasty)
			{
				if(null == c.liege || c.liege.GetDynasty() != player_dynasty) selectedCastle.Attack(c);
			}
		}
	}
}//namespace
