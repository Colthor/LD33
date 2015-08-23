using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace mjc_ld33
{
	using dynasty = System.Collections.Generic.List<Person>;

	public class DynastyGen
	{
		static string[] family_names_src = new string[] {
			"Lannister", "Osborne", "de Montfort", "Drake", "Gaunt", "Cromwell", "Plantagenet",
			"Dracula", "Harefoot", "Achan", "Thatcher", "de Wolff", "Eriksson", "Sigurd", "Orange"
		};
		
		private static int dynasty_counter = 0;

		private int NextDynasty()
		{
			return ++dynasty_counter;
		}

		private List<string> availableNames = null;
		private List<int> availableBanners = null;


		//If this ever gets worked on outside a jam, these could really do with refactoring.
		public Dictionary<int, dynasty> dynastiesGenerated = new Dictionary<int, dynasty>();
		public Dictionary<int, string> dynastyNames = new Dictionary<int, string>();
		public Dictionary<int, int> dynastyFlags = new Dictionary<int, int>();



		public DynastyGen(int set_bannerCount)
		{
			availableNames = new List<string>();
			availableNames.AddRange(family_names_src);
			availableBanners = new List<int>();
			for(int i = 0; i < set_bannerCount; i++) availableBanners.Add(i);
		}

		private string GetFamilyName()
		{
			string name = availableNames[Random.Range(0, availableNames.Count)];
			availableNames.Remove(name);
			return name;
		}

		private int GetFamilyBanner()
		{
			int flag = availableBanners[Random.Range(0, availableBanners.Count)];
			availableBanners.Remove(flag);
			return flag;
		}

		private void FindSpouse(Person p, List<Person> unmarried, List<Person> married)
		{
			const int SPOUSE_FIND_ATTEMPTS = 2;
			//try to find spouse in unmarried list.
			bool success = false;
			int attempts = 0;
			while (!success && attempts < SPOUSE_FIND_ATTEMPTS && unmarried.Count > 0)
			{
				attempts++;
				Person spouse = unmarried[Random.Range(0, unmarried.Count)];
				if(spouse.GetDynasty() != p.GetDynasty())
				{
					p.AddSpouse(spouse);
					unmarried.Remove(spouse);
					unmarried.Remove(p);
					if(Random.Range(0,2) == 0)
					{
						married.Add(p);
					}
					else
					{
						married.Add(spouse);
					}
					success = true;
				}
				
			}
		}

		//Returns IDs of dynasties generated
		//TODO: Param for "bonus" family members if we need a larger dynasty (eg. the player)
		public int[] GenerateIntertwinedDynasties(int dynCount)
		{
			float GENERATE_NEW_SPOUSE_CHANCE = 2.0f/( 3f * (float)dynCount);//0.33f;

			dynasty[] createdDyns = new dynasty[dynCount];
			int[] dynastyIDs = new int[dynCount];

			List<Person> unmarried = new List<Person>();
			List<Person> married = new List<Person>(); //people to generate kids for - only need 1 from each couple

			//Generate dynastic heads
			for(int i = 0; i < dynCount; i++)
			{
				dynastyIDs[i] = NextDynasty();
				createdDyns[i] = new dynasty();
				dynastyNames.Add(dynastyIDs[i], GetFamilyName());
				dynastyFlags.Add(dynastyIDs[i], GetFamilyBanner());
				Person head = new Person(dynastyIDs[i], 1, dynastyNames[dynastyIDs[i]]);
				createdDyns[i].Add(head);
				unmarried.Add(head);

				//Generate heads' siblings
				int numSibs = Random.Range(1, 4);
				for(int s = 0; s < numSibs; s++)
				{
					Person sibling = new Person(dynastyIDs[i], 2, dynastyNames[dynastyIDs[i]]);
					//head.AddSibling(sibling);
					createdDyns[i].Add(sibling);
					unmarried.Add(sibling);
				}
				foreach(Person p in createdDyns[i])
				{
					foreach(Person q in createdDyns[i])
					{
						if( p != q) p.AddSibling(q);
					}
				}
			}

			//Generate marriages (and new spouses if we like)
			for(int i = 0; i < dynCount; i++)
			{
				List<Person> CreatedSpouses = new List<Person>();
				foreach(Person p in createdDyns[i])
				{
					if(!p.IsMarried())
					{
						if(Random.Range(0.0f, 1.0f) < GENERATE_NEW_SPOUSE_CHANCE)
						{ //New spouse out of nowhere
							Person newSpouse = new Person(dynastyIDs[i], 3, dynastyNames[dynastyIDs[i]]);
							p.AddSpouse(newSpouse);
							CreatedSpouses.Add(newSpouse);
							unmarried.Remove(p);
							married.Add(p);
						}
						else if(dynCount > 1)
						{
							FindSpouse(p, unmarried, married);
						}
					}
				}

				//Add newly created spouses to dynasty
				foreach(Person p in CreatedSpouses)
				{
					createdDyns[i].Add(p);
				}
				CreatedSpouses = null;

			}


			List<Person> addedKids = new List<Person>();
			//Generate Children
			foreach(Person p in married)
			{
				int kidDyn = p.GetDynasty();
				int kidDynIndex = -1;
				for(int i = 0; i < dynCount; i++)
				{
					if(dynastyIDs[i] == kidDyn)
					{
						kidDynIndex = i;
						break;
					}
				}
				int numKids = Random.Range(0,5);
				for(int k = 0; k < numKids; k++)
				{
					Person kid = new Person(kidDyn, 4, dynastyNames[kidDyn]);
					addedKids.Add(kid);
					unmarried.Add(kid);
					p.AddChild(kid);
					createdDyns[kidDynIndex].Add(kid);
				}
			}

			//TODO: Another round of marrying here? Add a few more connections.
			foreach(Person p in addedKids)
			{
				if(!p.IsMarried()) FindSpouse(p, unmarried, married);
			}

			//Add created dynasties to the list of generated dynasties
			for(int i = 0; i < dynCount; i++)
			{
				dynastiesGenerated.Add(dynastyIDs[i], createdDyns[i]);
			}
			unmarried = null;
			married = null;
			createdDyns = null;
			return dynastyIDs;

		}

		public Person GetUnlandedMember(int dynastyID)
		{
			dynasty d = dynastiesGenerated[dynastyID];
			Person rP = null;

			if (null != d)
			{
				foreach(Person p in d)
				{
					if (null == p.holding)
					{
						rP = p;
						break;
					}
				}
			}
			else
			{
				Debug.Log("Dynasty ID " + dynastyID + " is null!");
			}

			return rP;

		}

	}

}//namespace