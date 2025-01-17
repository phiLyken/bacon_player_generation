﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Used for weighted lookup of "Positions"
/// </summary>
[System.Serializable]
public class PositionWeight{
	public Positions position;
	public float weight;
}

/// <summary>
/// Rule for the occurence and the min and max value, used for "Age" and "Level"
/// </summary>
[System.Serializable]
public class AuctionGenerationRule{
	public int count;
	public float min;
	public float max;
}


public enum Positions{
	GK, STR, CB, LW, LB, DMC, AMC, RB, RW, MC
}


public class Auction  {
	
	public Positions Position;
	public int Age;
	public float Level;

	public Auction(Positions _pos, float _level, float _age){
		Age = (int) Mathf.Round(_age);
		Level = _level;
		Position = _pos;
	}

	public override string ToString ()
	{
		return Position.ToString()+" | lvl "+Level+" | age "+Age;
	}
}


public class AuctionGenerator : MonoBehaviour {

	//LevelConfigs + Age Configs MUST BE OF THE SAME LENGTH
	public List<AuctionGenerationRule> PlayerLevelConfigs;
	public List<AuctionGenerationRule> PlayerAgeConfigs;

	//PositionImportance and Startweights MUST BE IN SAME ORDER
	public List<PositionWeight> PositionImportance;
	public List<PositionWeight> PositionStartWeights;

	//These buckets will be filled 
	List<Positions> position_bucket;
	List<float> age_bucket;
	List<float> level_bucket;


	public List<PosConfig> position_configs;

	void Start(){
	//	GenerateMarket();
	}


	public void GenerateMarket(){

		DeleteUI();

		//fill the age and level buckets
		level_bucket = CreateNumericBucket(PlayerLevelConfigs);
		age_bucket = CreateNumericBucket(PlayerAgeConfigs);

		//fill the position bucket
		position_bucket = CreatePositionBucket3(position_configs, level_bucket.Count);

		//use the buckets to create auctions
		List<Auction> auctions = GenerateAuctionsFromBuckets(age_bucket, level_bucket, position_bucket);

		foreach(Auction auction in auctions){
//			Debug.Log(auction.ToString());
		}

		GenerateUIList(auctions);
	}

	public void GenerateMarket2(){

		DeleteUI();

		//fill the age and level buckets
		level_bucket = CreateNumericBucket(PlayerLevelConfigs);
		age_bucket = CreateNumericBucket(PlayerAgeConfigs);

		//fill the position bucket
		position_bucket = CreatePositionBucket4(PositionStartWeights, level_bucket.Count);

		//use the buckets to create auctions
		List<Auction> auctions = GenerateAuctionsFromBuckets(age_bucket, level_bucket, position_bucket);

		foreach(Auction auction in auctions){
			//Debug.Log(auction.ToString());
		}

		GenerateUIList(auctions);
	}


	List<float> CreateNumericBucket(List<AuctionGenerationRule> rules){
		
		List<float> bucket = new List<float>();

		foreach(AuctionGenerationRule rule in rules){
			for(int i = 0; i < rule.count; i++){
				bucket.Add( Random.Range(rule.min, rule.max));
			}
		}

		return bucket;
	}

	/// <summary>
	/// 1. Creates a bucket with positions
	/// 2. To determine a position we use a weighted list of positions
	/// 3. The weights of this list will change in every iteration
	/// 3.1. The weight of the choosen position will be set to 0
	/// 3.2. The weight of ALL OTHER positions will be increased by a "static" value, defined in "PositionImportance"
	/// 3.3. -> For each iteration over the "weighted positions" the weights are altered
	/// </summary>
	List<Positions> CreatePositionBucket(List<PositionWeight> start_weights, List<PositionWeight> importance, int count){
		
		List<Positions> bucket = new List<Positions>();

		//we use the already configured list of start weights for the first iteration, and then continue iterating with that list, it will be modified
		List<PositionWeight> current_weights = new List<PositionWeight>(start_weights);

		for(int i = 0; i < count;i++){
			//find a position from the weighted list
			Positions pos = GetWeightedPosition(current_weights);

			//update the weighted list (chosen position will be set to 0, all the others will be increased)
			UpdateWeights(current_weights,importance, pos);

			bucket.Add(pos);
		}

		return bucket;

	}


	/// <summary>
	/// Updates the weights.
	/// </summary>
	/// <param name="current">list that should be changed</param>
	/// <param name="importance">list should be in same order than "current", values in "current" will be increased 1:1 by importance</param>
	/// <param name="reset_pos">This is the position whose weight will be reduced to 0</param>
	void UpdateWeights(List<PositionWeight> current, List<PositionWeight> importance, Positions reset_pos){
		Debug.Log("updating weights pos:"+ reset_pos);

	//	current.ForEach( pw => pw.weight - 5 );

	
		for(int i = 0; i < current.Count; i++){
			if(current[i].position == reset_pos){
				current[i].weight = 0;
			} else{
				current[i].weight += importance[i].weight;
			}
		}

	}


	List<Auction> GenerateAuctionsFromBuckets(List<float> ages, List<float> levels, List<Positions> positions){
		
		//All lists must be of the same length, if not, configuration has been messed up
		if(ages.Count != levels.Count || levels.Count != positions.Count) {
			Debug.LogWarning("different bucket sizes age"+ages.Count+" levels "+levels.Count+" pos "+positions.Count);
			return null;
		}

		List<Auction> auctions = new List<Auction>();

		//for now, the number of auctions is simply the length of one of the lists (since they are all the same length)
		int auction_count = ages.Count;

		//in each iteration one item will be returned from the bucket (this also removes the item from the bucket)
		for(int i = 0; i<auction_count;i++){
			auctions.Add(
				new Auction( 
					GetRandomItemFromList(positions),
					GetRandomItemFromList(levels), 
					GetRandomItemFromList(ages)
				)
			);
		}

		return auctions;

	}

	//returns random item from a list and removes it
	static T GetRandomItemFromList<T> (List<T> items){
		T item = items[Random.Range(0, items.Count)];
		items.Remove(item);
		return item;
	}


	Positions GetWeightedPosition(List<PositionWeight> positions){
		
		float total_chance = 0;

		//calculate sum of weights
		foreach(PositionWeight weighted_item in positions){
			total_chance+= weighted_item.weight;
		}

		//determine random value for sum of weights
		float r = Random.value * total_chance;
		float last = 0;

		//get the item for the r value
		for(int i = 0; i < positions.Count; i++){
			if(r > last && r < last + positions[i].weight){
				return positions[i].position;
			}

			last+= positions[i].weight;
		}

		//oh oh
		Debug.LogWarning("no position from weightable list found!! :(");
		return Positions.GK;
	}


	void GenerateUIList(List<Auction> auctions){

	
		auctions = OrderAuctions.OrderByPos(auctions);

		foreach(Auction auction in auctions){
			GameObject obj = Instantiate( Resources.Load("auction_item") )as GameObject;
			obj.transform.SetParent(transform, false);
			obj.GetComponent<AuctionUI>().SetAuction(auction);
		}
	}

	void DeleteUI(){
		var Children = new List<GameObject>();
		foreach(Transform child in transform) Children.Add(child.gameObject);
		Children.ForEach(child => Destroy(child));
	}


	[System.Serializable]
	public class PosConfig{
		public Positions pos;
		public int RollCount;
		public float Min;
		public float Max;

		public float accumulated;
	}


	List<Positions> CreatePositionBucket2(List<PosConfig> configs, int numOfPlayers){

		List<Positions> positions = new List<Positions>();
		Dictionary<Positions, float> position_raw = new Dictionary<Positions, float>();

		float total = 0;

		foreach(PosConfig position in configs){
			
			position_raw.Add(position.pos, 0);

			for(int i = 0; i < configs.Count; i++){
				position_raw[position.pos] += Random.Range(position.Min, position.Max);
			}

			total += position_raw[position.pos];
		}

		int added_positions = 0;

		foreach(var pair in position_raw){	

			int count = (int) ( ( ((pair.Value) / total) * numOfPlayers) + 0.5f);
			added_positions += count;

			for(int i = 0; i < count; i++){
				positions.Add( pair.Key);
			}
		}

		Debug.Log(added_positions.ToString() );

		positions[5] += numOfPlayers-added_positions;


		return positions;
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

	List<Positions> CreatePositionBucket4(List<PositionWeight> start_weights, int numOfPlayers){

		List<Positions> bucket = new List<Positions>();

		for(int i = 0; i < numOfPlayers;i++){
			//find a position from the weighted list
			Positions pos = GetWeightedPosition(start_weights);
			bucket.Add(pos);
		}

		return bucket;
	}


}
