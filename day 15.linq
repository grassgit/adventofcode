<Query Kind="Program" />

void Main()
{

	var input =
		File.ReadAllText(Util.CurrentQueryPath + ".input")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
		;

	int Hash(string value) => value.Aggregate(0, (a, x) => ((a + x) * 17) % 256);

	Hash("HASH").Dump();
	var boxes = new int[256].Select(x => new Dictionary<string, (int Index, int Value)>() { { "", (0, -1) } }).ToArray();

	Regex.Matches(input, @"(?<s>(?<l>[^,=-]+)(?<a>(=(?<v>\d))|-))(?=(,|$))")
		.Select(x =>
		new
		{
			Source = x.Value,
			Label = x.Groups["l"].Value,
			Box = Hash(x.Groups["l"].Value),
			Add = x.Groups["a"].Value != "-",
			Value = x.Groups["v"].Success ? int.Parse(x.Groups["v"].Value) : -1
		})
		.Aggregate(
			boxes,
			(a, x) => (x.Add
				? boxes[x.Box].ContainsKey(x.Label) ? boxes[x.Box].Set(x.Label, z => (z.Index, x.Value)) : boxes[x.Box].Append(x.Label, (boxes[x.Box].Max(z => z.Value.Index) + 1, x.Value))
				: boxes[x.Box].Remove(x.Label)) ? boxes : boxes
		)
		.Select(x => x.Where(y => y.Key != "").OrderBy(x => x.Value.Index).Select(b => new { Label = b.Key, Value = b.Value.Value }))
		.SelectMany((x, b) => x.Select((y, s) => (b + 1) * (s + 1) * y.Value))
		.Sum()
		.Dump();
	//.Select(Hash)
	//.Sum()
	//.Dump();

	// part 1: 509784
	// part 2: 230197
}

static class Ext
{
	public static bool Set<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TValue, TValue> set)
	{
		source[key] = set(source[key]);
		return true;
	}
	public static bool Append<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value)
	{
		source.Add(key, value);
		return true;
	}
}