using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class OrderAuctions   {

	public static List<Auction> OrderByPos(List<Auction> list){

		return list.OrderByDescending( i => i.Position.ToString()	).ToList();
	}
}
