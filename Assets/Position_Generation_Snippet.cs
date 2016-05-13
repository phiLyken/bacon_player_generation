using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Position_Generation_Snippet : MonoBehaviour {

	[System.Serializable]
	public class PosConfig{
		public Positions pos;
		public int RollCount;
		public float Min;
		public float Max;

		public float accumulated;
	}


	public enum Positions{
		GK, STR, CB, LW, LB, DMC, AMC, RB, RW, MC
	}



	List<Positions> CreatePositionBucket3(List<PosConfig> configs, int numOfPlayers){

		List<Positions> positions = new List<Positions>();

		//Due to randomness we dont know when we have enough players, so fill the list until we have enough
		while(positions.Count <  numOfPlayers){

			//In each iteration we cycle through the position confis
			foreach(PosConfig pos_config in configs){		


				//Each configs has defined number of "rolls"..
				for(int i = 0; i<pos_config.RollCount;i++){		

					//For each roll we generate a uniformly distributed random float and add it to an accumulated value for each position
					pos_config.accumulated+= Random.Range(pos_config.Min,pos_config.Max);

					//When we reach "1" we increase the number of positions of this type
					if(pos_config.accumulated > 1.0f){						
						positions.Add( pos_config.pos);
						pos_config.accumulated -= 1.0f;
						if(positions.Count == numOfPlayers) {
							return positions;
						}
					}
				}
			}
		}
		return positions;
	}

}
