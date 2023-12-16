<Query Kind="Program" />

void Main()
{
	var input =
		File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", ".txt"))
		//File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", "-1.txt"))
		;

	input.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
		.Select(x => new
		{
			Value = x,
			Other = MapB(x[0] - 'A'),
			Me = MapA(x[2] - 'X'),
			// Part 2
			Desired = (Result)(x[2] - 'X'),
			Play = UnCheck(MapA(x[0] - 'A'), (Result)(x[2] - 'X'))
		})
		.Dump()
		.Select(x => new
		{
			x.Value,
			x.Other,
			x.Me,
			Result = Check(x.Me, x.Other),
			Score = Score(Check(x.Me, x.Other), x.Me),
			// Part 2
			DesiredScore = Score(Check(x.Play, x.Other), x.Play)
		})
		//.Dump()
		.Aggregate(new { Part1 = 0, Part2 = 0 }, (a, x) => new { Part1 = a.Part1 + x.Score, Part2 = a.Part2 + x.DesiredScore })
		.Dump();

	// part 1: 12276
	// part 2: 9975

	PlayerA MapA(int c) => (PlayerA)(c % 3);
	PlayerB MapB(int c) => (PlayerB)(5 + ((c - 1) % 3));
	Result Check(PlayerA o, PlayerB s) => 2 - (Result)(((int)s - (int)o) % 3);
	PlayerA UnCheck(PlayerA o, Result s) => MapA((int)s + (int)o + 2);

	int Score(Result r, PlayerA s) => 3 * (int)r + (int)s + 1;

	Enum.GetValues<PlayerA>()
		.SelectMany(o => Enum.GetValues<PlayerB>()
			.Select(s => new
			{
				Me = o,
				Me1 = (int)o,
				Opponent = s,
				Opponent1 = (int)s,
				Result = Check(o, s),
				r = (int)Check(o, s),
				b = (int)o,
				Score = Score(Check(o, s), o),
			}))
			.Dump();
}


enum Result
{
	Win = 2, // Z
	Lose = 0, // X
	Draw = 1 // Y
}

enum PlayerA
{
	Rock = 0, // A
	Paper = 1, // B
	Scissors = 2 // cc
}


enum PlayerB
{
	Rock = 4, // X
	Paper = 5, // Y
	Scissors = 3 // Z
}
