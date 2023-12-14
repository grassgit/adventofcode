<Query Kind="Statements" />

var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

var lines = input.Split("\r\n");
var direction = new Dictionary<char, int> { { 'L', 0 }, { 'R', 1 } };
var path = lines[0]
	.Select(x => direction[x])
	.ToArray()
//.Dump()
;

var node = lines
	.Skip(2)
	.Select(x => Regex.Match(x, @"(?<n>\w+) = \((?<l>\w+), (?<r>\w+)\)"))
	.Select(x => new
	{
		Node = x.Groups["n"].Value,
		Next = new[] { x.Groups["l"].Value, x.Groups["r"].Value }
	})
	.ToDictionary(x => x.Node)
	//.Dump()
	;

var step = 0L;
var current = node.Keys.Where(x => x.EndsWith("A"))
.ToArray()
.Dump()
;

//step = 42090427L;
//current = @"PNH
//FPZ
//GKG
//MLZ
//NCD
//DPZ".Split("\r\n");
var exit = node.Keys.Where(x => x.EndsWith("Z"))
.Select((x, i) => (x, i))
.ToDictionary(x => x.x, x => x.i);

var exits = new long[current.Length].Select(x => new long[current.Length]).ToArray();
//current = current.Take(3).ToArray();
var next = new string[current.Length];
var solutions = new Dictionary<int, (long Offset, long Interval)>();
var done = new bool[current.Length];
while (true)
{
	for (var cnt = 0; cnt < current.Length; cnt++)
	{
		next[cnt] = node[current[cnt]].Next[path[step % path.Length]];

		if (next[cnt].EndsWith('Z'))
		{
			if (exits[cnt][exit[next[cnt]]] == 0)
			{
				exits[cnt][exit[next[cnt]]] = step;
			}
			else if ((step - exits[cnt][exit[next[cnt]]]) % path.Length == 0)
			{
				$"Repeat {cnt} {next[cnt]}: first {exits[cnt][exit[next[cnt]]]} step {step}".Dump();
				//if (solutions
				done[cnt] = true;
				if (!solutions.ContainsKey(cnt))
					solutions.Add(cnt, (exits[cnt][exit[next[cnt]]], step - exits[cnt][exit[next[cnt]]]));
			}
		}
	}

	if (done.All(x => x))
		break;
	current = next;
	step++;

	if (current.All(x => x.EndsWith("Z")))
		break;
	//if (current.Count(x => x.EndsWith("Z")) == 2)
	//	break;
	if (step % 50_000_000 == 0)
		$"Step {step}".Dump();
	if (current.Count(x => x.EndsWith("Z")) == 5)
		$"Step {step}: {string.Join(" ", current)}".Dump();
}

solutions.Dump();

// Matching first three positions > 67477477
// without breaking on done.all > ~4 minutes
// with done and interval run > 0.15 seconds
var intervals = solutions.Values.ToArray();
var position = intervals.Select(x => x.Offset).ToArray();

// To lazy to calculate from here just force it
while (true)
{
	var max = position.Max();
	for (var cnt = 0; cnt < position.Length; cnt++)
	{
		while (position[cnt] < max)
			position[cnt] += intervals[cnt].Interval;
	}

	if (position.Distinct().Count() == 1)
		break;
}

position.Dump();

//while (true)
//{
//	current = current
//		.Select(x=> node[x].Next[path[step % path.Length]])
//		//.Dump()
//		.ToArray();
//	step++;
//
//	if (step > 100000000)
//		throw new Exception(@"Escape");
//	if (current.Count(x => x.EndsWith("Z")) >= 4)
//		break;
//}
current.Dump();
step.Dump();
// part 1: 20221
// part 2: 14616363770446 > low, 14616363770450> high | Correct: 14616363770447
