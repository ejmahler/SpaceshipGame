using System.Collections.Generic;
using System;

public class Utilities {
	private static Random randomGenerator = new Random();

	public static T ChooseRandom<T>(List<T> items) {
		int index = randomGenerator.Next (items.Count);
		return items[index];
	}
}
