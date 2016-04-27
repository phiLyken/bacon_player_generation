using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class PosColor{
	public Positions pos;
	public Color col;
}
public class AuctionUI : MonoBehaviour {

	public Image PosBG;
	public Text PosTF;
	public Image LevelBG;
	public Text LevelTF;

	public Text AgeTF;

	public List<Color> qualities;

	public List<PosColor> colors;

	public void SetAuction(Auction auction){
		PosBG.color = GetColForPos(auction.Position);
		LevelBG.color = GetColForLevel(auction.Level);

		PosTF.text = auction.Position.ToString();
		LevelTF.text = auction.Level.ToString("F1");

		AgeTF.text = auction.Age.ToString();
	}

	Color GetColForPos(Positions pos){
		foreach(PosColor config in colors){
			if( config.pos == pos) return config.col;
		}

		return Color.green;
	}

	Color GetColForLevel(float l){
		if(l < 6) return qualities[0];
		if(l < 9) return qualities[1];

		return qualities[2];
	}
}
