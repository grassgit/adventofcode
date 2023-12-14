<Query Kind="Statements" />

var input = 
	File.ReadAllText(Util.CurrentQueryPath + ".input")
	//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
;


var line = new Regex(@"^Game (?<num>\d+): ((?<set>[^;]+)(;|$))+$");

var available = new Dictionary<string, int>()
{
	{"red", 12 },
	{"green",13},
	{"blue",14}
};

input.Split("\r\n")
.Select(x => line.Match(x))
.Select(x => new
{
	Input = x.Value,
	Id = int.Parse(x.Groups["num"].Value),
	Sets = x.Groups["set"].Captures.ToArray()
		.Select(x=>x.Value.Trim(' ', ';'))
		.Select(x=>x
			.Split(",")
			.Select(y=>y.Trim())
			.Select(y=>y.Split(' '))
			.Select(y=>new { Count = int.Parse(y[0]), Color = y[1] })
			.Select(x=>new { x.Count, x.Color, Possible = x.Count <= available[x.Color] })
			)
		.Select(x => new { 
			Cubes = x,
			Possible = x.All(y => y.Possible),
			//Red = x.Where(y => y.Color == "red").Sum(y => y.Count),
			//Blue = x.Where(y => y.Color == "blue").Sum(y => y.Count),
			//Green = x.Where(y=>y.Color =="green").Sum(y=>y.Count) 
			})
})
.Select(x => new
{
	x.Input,
	x.Id,
	x.Sets,
	Required = x.Sets.SelectMany(y=>y.Cubes).GroupBy(x=>x.Color).ToDictionary(x=>x.Key, x=>x.Max(y=>y.Count))
})
//.Where(x=>x.Sets.All(y=>y.Possible))
.Select(x => new { x.Id, Power = x.Required.Values.Aggregate(1, (a, x) => a * x) } )
.Dump()
.Sum(x=>x.Power)

.Dump();
