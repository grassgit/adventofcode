<Query Kind="Program" />



void Main()
{
	var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;
	var lines = input.Split("\r\n");
	lines
		.Select(x => x.Split(" ").Select(int.Parse).ToArray())
		//.AsParallel()
		.Select((l, i) => new { Index = i, Inputs = l, Sequence = Sequence(l, new[] { l }).SkipLast(1).ToArray() })
		//.Dump()
		//.Select(x => Last(x.Inputs, x.Sequence))
		.Select(x => First(x.Inputs, x.Sequence))
		.Dump()
		.Sum()
		.Dump()
		;
}


int[][] Sequence(int[] inputs, int[][] prev = null)
{
	var steps = inputs
		.Skip(1)
		.Zip(inputs, (a, b) => a - b)
		.ToArray()
	;

	prev = prev.Concat(new[] { steps }).ToArray();

	if (steps.All(x => x == 0))
		return prev;
	else
		return Sequence(steps, prev);
}

int Last(int[] inputs, int[][] sequence) => sequence.Reverse().Aggregate(0, (a, x) => (a + x[^1]));
int First(int[] inputs, int[][] sequence) => sequence.Reverse().Aggregate(0, (a, x) => (x[0] - a)); //.Dump($"{a} - {x[0]}"));

// Sample: 114
// Part 1: 1980437560
// Part 2: 977