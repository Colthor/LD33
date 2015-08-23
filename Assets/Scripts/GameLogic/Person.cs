using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Castle;

namespace mjc_ld33
{

	public class Person
	{
		private static int person_counter = 0;
		
		private int NextPersonID()
		{
			return ++person_counter;
		}
		
		public const float SPOUSE_FACTOR = 0.5f;
		public const float SIBLING_FACTOR = 0.2f;
		public const float CHILD_FACTOR = 0.35f; //Not to be confused with Macaulay Culkin

		int id;
		Person spouse = null;
		Person parent1 = null;
		Person parent2 = null;
		List<Person> siblings;
		List<Person> children;

		bool alive = true;

		public Castle holding = null;
		private int dynasty;

		public Person(int set_dynasty)
		{
			id=NextPersonID();
			siblings = new List<Person>();
			children = new List<Person>();
			dynasty = set_dynasty;
		}

		public string GetName()
		{
			return dynasty.ToString() + "_" + id.ToString();
		}

		public int GetDynasty()
		{
			return dynasty;
		}

		private string PrintListPeeps(List<Person> l)
		{
			string peeps = "";
			foreach(Person p in l)
			{
				peeps += p.GetName() + ", ";
			}
			return peeps;
		}

		public string PrintRelationships()
		{
			string rels = GetName() + "\n";
			rels += "Spouse: ";
			if(null == spouse)
			{
				rels += "null";
			}
			else
			{
				rels += spouse.GetName();
			}
			rels += "\nSiblings: " + PrintListPeeps(siblings) + "\n";
			rels += "Children: " + PrintListPeeps(children) + "\n";

			return rels;
		}

		public void AddSpouse(Person newSpouse)
		{
			spouse = newSpouse;
			newSpouse.spouse = this;
		}
		
		public void AddSibling(Person newSibling)
		{
			siblings.Add(newSibling);
			newSibling.siblings.Add(this);
		}
		
		public void AddChild(Person newChild)
		{
			foreach(Person p in children) p.AddSibling(newChild);
			newChild.parent1 = this;
			newChild.parent2 = spouse;
			children.Add(newChild);
			if(null != spouse)
			{
				spouse.children.Add(newChild);
			}
		}

		public void Kill()
		{
			alive = false;
		}

		public bool IsAlive()
		{
			return alive;
		}

		public bool IsMarried()
		{
			return !(null == spouse);
		}

		public float Morale()
		{
			int dead_children=0, dead_siblings=0, alive_children=0, alive_siblings=0;
			bool spouse_alive = true;
			if(spouse != null)
			{
				spouse_alive = spouse.IsAlive();
			}
			foreach (Person p in siblings)
			{
				if(p.IsAlive())
				{
					alive_siblings++;
				}
				else
				{
					dead_siblings++;
				}
			}
			foreach (Person p in children)
			{
				if(p.IsAlive())
				{
					alive_children++;
				}
				else
				{
					dead_children++;
				}
			}

			float morale = 1.0f;
			morale -= (float)dead_siblings * SIBLING_FACTOR;
			morale -= (float)dead_children * CHILD_FACTOR;
			if(!spouse_alive) morale -= SPOUSE_FACTOR;

			return morale;
		}

		private void DrawTo(Person p, Color col)
		{
			
			if(null != p && p.IsAlive() && null != p.holding)
			{
				Debug.DrawLine(holding.transform.position, p.holding.transform.position, col);
				holding.DrawConnectionTo(p.holding, col);
			}

		}

		public void DrawConnections()
		{
			if(null != holding)
			{
				DrawTo(spouse, Color.red);
				
				DrawTo(parent1, Color.magenta);
				DrawTo(parent2, Color.magenta);


				foreach(Person p in siblings)
				{
					DrawTo(p, Color.blue);
				}

				foreach(Person p in children)
				{
					DrawTo(p, Color.yellow);

				}
			}
		}
	}

}//namespace
