<Query Kind="Program" />

void Main()
{

	var input =
	File.ReadAllText(Util.CurrentQueryPath.Replace("12-2","12") + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;
	var repeat = 5;
	var data = Regex.Matches(input + "\r\n", @"^\s*(?<l>(?<b>[?.#]+) (?<n>((?<a>\d+)(,|\r\n))+))", RegexOptions.Multiline)
		//.Skip(2).Take(1)
		.Select(x => new
		{
			//Source1 = string.Join("?", new int[1].Select(_ => x.Groups["b"].Value)),
			Source = string.Join("?", new int[repeat].Select(_ => x.Groups["b"].Value)),
			//N = x.Groups["n"].Value,
			//Numbers1 = new int[1].SelectMany(_ => x.Groups["a"].Captures.OrderBy(c => c.Index).Select(c => int.Parse(c.Value))).ToArray(),
			Numbers = new int[repeat].SelectMany(_ => x.Groups["a"].Captures.OrderBy(c => c.Index).Select(c => int.Parse(c.Value))).ToArray()
		})
		//.Select(x => new { x.Source1, x.N, Result = Process(x.Source, x.Numbers), Result1 = Process(x.Source1, x.Numbers1) })
		.Select(x => new { Result = Process(x.Source, x.Numbers) })
		.ToArray()
		//.Dump()
		.Sum(x=>x.Result)
		.Dump();

	//45817930018 > low  -1 when False
	//45817930035 > low int overflow!! results in 0
	// correct: 37366887898686
	$"Cache got hit {_cacheHit.Values.Where(x => x != 0).Count()}. Preventing {_cacheHit.Values.Sum()} tail permutations from being evaluated. Only {data - _cacheHit.Values.Sum()} permutations were actually calculated".Dump();
}

long Process(string tail, int[] numbers)
{
	var s = Advance(tail.TrimEnd('.'), numbers, out var c) ? c : 0;
	return s;
}

string CanMatch(string tail, int[] numbers)
{
	var match = Regex.Match(tail, $"^(?<m>[?#]{{{numbers[0]}}})([?.]|$)");
	if (!match.Success)
	{
		return null;
	}

	return match.Groups["m"].Value;
}
bool log = false;
private Dictionary<string, long> _cache = new();
private Dictionary<string, long> _cacheHit = new();

bool Advance(string tail, int[] numbers, out long count)
{
	var prefix = log ? $"{new string(' ', 20 - tail.Length)}{tail} - {string.Join(",", numbers)}  {(numbers.Length - 1, tail.Length - 1)}" : "";
	tail = tail.TrimStart('.');
	var key = $"{tail} - {string.Join(",", numbers)}";
	if (_cache.TryGetValue(key, out var k))
	{
		if (log) $"{prefix} * c ({tail}) {k}".Dump();
		_cacheHit[key] += k;
		count = k;
		//index[(numbers.Length - 1, tail.Length - 1, 0)] += k;
		return count > 0;
	}
	count = 0;
	if (numbers.Length == 0)
	{
		return !tail.Contains('#');
	}
	else if (tail.Length < numbers.Sum(x => x) + numbers.Length - 1)
	{
		return false;
	}

	if (tail[0] == '?' && tail.Length > 1 && Advance(tail[1..], numbers, out long c1))
	{
		if (log) $"{prefix} * advance # {count}+{c1} ({tail[1..]})".Dump();
		count += c1;
	}
	var match = CanMatch(tail, numbers);
	if (match != null)
	{
		if (log) $"{prefix} - advance".Dump();

		if (numbers.Length == 1 && Regex.IsMatch(tail[match.Length..], "^[?.]*$"))
		{
			if (log) $"{prefix} * e {count}".Dump();
			count += 1;
		}
		else if (tail.Length > match.Length && Advance(tail[(match.Length + 1)..], numbers.Skip(1).ToArray(), out long c2))
		{
			if (log) $"{prefix} * -1 {count}+ {c2} ({tail[(match.Length + 1)..]})".Dump();
			count += c2;
		}
	}

	_cache.Add(key, count);
	_cacheHit.Add(key, 0);

	if (log) $"!{key} o {count} ".Dump();

	return count > 0;
}