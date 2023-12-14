<Query Kind="Program" />

void Main()
{

	var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

	bool HasSmudge(long a, long b) => Math.Log2(a ^ b) % 1 == 0;

	IEnumerable<int> Potentials(IEnumerable<long> data) => data.Select((x, i) => new { x, i }).Skip(1).Zip(data, (a, b) => a.x == b || HasSmudge(a.x, b) ? a.i : -1).Where(x => x != -1);

	bool IsMirror(IEnumerable<long> data, int start) => data.Take(start).Reverse().Zip(data.Skip(start), (a, b) => a == b).All(x => x);
	bool IsSmudged(IEnumerable<long> data, int start) =>
		new[] { data
			.Take(start)
			.Reverse()
			.Zip(data.Skip(start), (a, b) => a == b ? 0 : HasSmudge(a, b) ? 1 : 2)
			.ToArray()
			//.Dump(start.ToString())
		}
		.Where(x=>!x.Any(y=>y==2))
		.Where(x=>x.Count(y=>y==1) == 1)
		.Count() == 1;

	var split = Regex
		.Matches(Regex.Replace(input, @"^[ \t]+", "", RegexOptions.Multiline), @"((?<l>[#.]+)(\r\n|$))+")
		.Select(x => new
		{
			Lines = x.Groups["l"].Captures.Select(z => z.Value).ToArray()
		})
		//.Skip(1).Take(1)
		//.Dump()
		.Select(x => new
		{
			x.Lines,
			Transpose = x.Lines[0].Select((_, c) => string.Join("", x.Lines.Select((x) => x[c])))
		})
		.Select(x => new
		{
		L = x.Lines,
			Lines = x.Lines.Select(x => x.Select((c, i) => c == '#' ? (long)Math.Pow(2, i+1) : 0).Sum()),
			Transpose = x.Transpose.Select(x => x.Select((c, i) => c == '#' ? (long)Math.Pow(2, i+1) : 0).Sum()),
		})
		//.Dump()
		.Select(x => new
		{
		x.L,
			x.Lines,
			x.Transpose,
			H = Potentials(x.Lines).Select(p => new { p, r = IsSmudged(x.Lines, p) }).Where(x => x.r).Select(x => x.p),
			V = Potentials(x.Transpose).Select(p => new { p, r = IsSmudged(x.Transpose, p) }).Where(x => x.r).Select(x => x.p)
		})
		//.Where(x => x.H.Count() + x.V.Count() != 1)
		.Dump()
		.Sum(x => x.H.Sum(z => z) * 100 + x.V.Sum(z => z))
		.Dump();
		//part 1: 30802
		//part 2: 37876
}

// You can define other methods, fields, classes and namespaces here

