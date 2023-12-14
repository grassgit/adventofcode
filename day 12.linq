<Query Kind="Program" />

void Main()
{

	var input =
	File.ReadAllText(Util.CurrentQueryPath + ".input")
//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;

	// This version just dies at repeat>1 on the sample
	var repeat = 1;
	var data = Regex.Matches(input + "\r\n", @"^\s*(?<l>(?<b>[?.#]+) (?<n>((?<a>\d+)(,|\r\n))+))", RegexOptions.Multiline)
	//.Skip(2).Take(1)
	.Select(x => new
	{
		Source = string.Join("?", new int[repeat].Select(_ => x.Groups["b"].Value)),
		//Groups = x.Groups["g"].Captures.Select(c => new { c.Value, c.Index, State = c.Value[0] == '.' ? State.Op : c.Value[0] == '#' ? State.Dam : State.Un }),
		Numbers = new int[repeat].SelectMany(_ => x.Groups["a"].Captures.Select(c => new { Value = int.Parse(c.Value), c.Index })).ToArray()
	})
	.ToArray();


		data
			.AsParallel()
			.Select(x => new
			{
				x.Source,
				x.Numbers,
				Results = Process(x.Source, x.Numbers.Select(y => y.Value).ToArray(), 0)
					.DistinctBy(y => string.Join("-", y.Where(x => x.Used > 0).Select(z => z.Index)))
				.ToArray()
			})
			.ToArray()
			.Select(x => new
			{
				x.Source,
				//x.Results,
				Num = string.Join("-", x.Numbers.Select(z => z.Value)),
				Count = x.Results.Count(),
				//Nice = x.Results.Select(y => string.Join("-", y.Where(x => x.Used > 0).Select(z => z.Index)) + " | " + string.Join("", y.Select(z => z.Value))).OrderBy(x=>x).ToArray()
			})
			.ToArray()
			.Dump()
			.Sum(x => x.Count)
			.Dump()
			//.First().Results.Skip(1).Dump()
			;
	// part 1 7916
}
IEnumerable<Consume[]> Process(string value, int[] numbers, int offset)
{
	//$"{offset} {string.Join(",", numbers)} {value}".Dump();

	if (numbers.Length == 0 && Regex.Match(value, "^[.?]*$").Success)
	{
		yield return new[] { new Consume(value, 0, offset) };
	}
	else if (numbers.Any() && value.Length >= (numbers.Sum(x => x + 1) - 1))
	{
		var next = numbers.First();
		var match = Regex.Match(value, @$"(?<space>[?#]{{{next},}}?)");
		int end = 0;
		if (match.Success && (match.Index == 0 || !value[..(match.Index - 1)].Contains('#')))
		{
			//if (match.Index> 0)
			//	match.Dump();
			Consume result = null;
			try
			{
				end = match.Index + numbers[0];

				result = new Consume(match.Index == 0 ? value[..(match.Index + numbers[0])] : $"[{value[..match.Index]}]{value[match.Index..(match.Index + numbers[0])]}", next, offset + match.Index)
				{
					//Match = match,
					//Info = $"{match.Index == 0} {!value[..(match.Index)].Contains('#')} {(match.Index == 0 ? "-" :value[..(match.Index)])}" 
				};

				//result.ToString().Dump();
			}
			catch (Exception e)
			{
				e.Dump();
			}

			if (!match.Value.Contains("#"))
			{
				var skipThis = Process(value[end..], numbers, offset + end);
				foreach (var n in skipThis)
					yield return new[] { new Consume("[" + value[..end] + "]", 0, offset) }.Concat(n).ToArray();
			}

			if (match.Value.Length > 1 && match.Value.StartsWith("?"))
			{
				var skipThis = Process(value[1..], numbers, offset + 1);
				foreach (var n in skipThis)
					yield return new[] { new Consume("[" + value[0..1] + "]", 0, offset) }.Concat(n).ToArray();
			}

			if (value.Length > end && (value[end] == '?' || value[end] == '.'))
			{
				var withThis = Process(value[(end + 1)..], numbers.Skip(1).ToArray(), offset + end + 1);
				foreach (var n in withThis)
					yield return new[] { result with { Value = result.Value + "[" + value[end] + "]" } }.Concat(n).ToArray();
			}
			else if (numbers.Length == 1 && !value[end..].Contains('#'))
			{
				yield return new[] { result with { Value = result.Value + (value.Length > end ? "[" + value[end..] + "]" : "") } };
			}
		}
	}
	else
	{
		//"end".Dump();
	}
}


enum State { Dam, Op, Un }

record Consume(string Value, int Used, int Index)
{
	public Match Match { get; init; }
	public string Info { get; set; }
}