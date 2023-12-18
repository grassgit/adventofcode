<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Windows</Namespace>
</Query>


void Main()
{
	var input =
		File.ReadAllText(Util.CurrentQueryPath + ".input")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample1")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample2")
		//File.ReadAllText(Util.CurrentQueryPath + ".sample3")
		;

	var map = input.Split("\r\n")
		//.SelectMany((l, y) => l.Select((c, x) => new { X = x, Y = y, Value = c - '0' }))
		//.ToDictionary(x => new Vector(x.X, x.Y));
		.SelectMany((l, y) => l.Select((c, x) => new Vertex(x, y, c - '0')))
		.ToDictionary(x => x.Position);

	var start = map[(0, 0)];

	var queue = new List<Path>() { new Path(start, new Vertex[] { start }, 0, Direction.None) };

	var destination = map[(map.Keys.Max(x => x.X), map.Keys.Max(x => x.Y))];
	int min = 4;
	int max = 10;
	Dictionary<int, int[]> stepOver = Ruler(max)
		.Select(x => Ruler(x + 1, 1).Select(x => x).Reverse().ToArray())
		.SelectMany((x, i) => new[] { (i, x), (i + max, x.Select(y => -y).ToArray()) })
		.ToDictionary(x => x.Item1, x => x.Item2);
	int step = 0;
	int spotSize = 3;
//#define GIF
#if GIF
	using var fs = new FileStream(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Util.CurrentQueryPath), "out2.gif"), FileMode.Create);
	using var gif = new GifWriter(fs, 20, -1);
	using var image = new Bitmap((destination.Position.Y + 1) * spotSize, (destination.Position.X + 1) * spotSize);
	bool init = false;
	using (var g = Graphics.FromImage(image))
	{
		g.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);
#endif
		while (queue.Count > 0)
		{
			step++;
			//if (step == 2) break;
			var item = queue.First();
			queue.RemoveAt(0);

			if (step % 1000 == 0)
				$"{step} - {item.PathCost}".Dump();

			bool log(Coordinate x) => false; //x == (2, 1) || x == (2, 0);
			if (log(item))
				$"Processing {item}".Dump();

#if GIF
			using (var b = new SolidBrush(Color.FromArgb((int)Math.Min(255, item.PathCost * 0.255), (int)Math.Max(0, (500 - item.PathCost) * 0.255), 0)))
				g.FillEllipse(b, item.Node.Position.X * spotSize, item.Node.Position.Y * spotSize, spotSize, spotSize);

			if (step % 30 == 0)
				gif.WriteFrame(image);
#endif
			item.Node.SetVisited(item.Direction);

			if (item.Node == destination)
				break;
			var next = new[] {
					new int[stepOver.Count]
						.Where(_ => item.Direction != Direction.Horizontal)
						.Select((_,x) => new { Skip = stepOver[x].Where(d=>Math.Abs(d)<min).Select(x=>item.Node.Position+(x,0)).ToArray(), Path = stepOver[x].Select(x=>item.Node.Position+(x,0)).ToArray(), Direction = Direction.Horizontal }),
				 	new int[stepOver.Count]
						.Where(_ =>item.Direction != Direction.Vertial)
						.Select((_,y) => new { Skip = stepOver[y].Where(d=>Math.Abs(d)<min).Select(x=>item.Node.Position+(x,0)).ToArray(), Path = stepOver[y].Select(y=>item.Node.Position+(0,y)).ToArray(), Direction = Direction.Vertial }),
				}
				.SelectMany(x => x)
				.Where(x => x.Path.Length > x.Skip.Length)
				//.Dump()
				.Select(x => new { Path = x.Path.Select(y => map.Get(y)).ToArray(), x.Direction })
				.Where(x => x.Path.All(x => x != null))
				.Select(x => new Path(x.Path[0], x.Path.Concat(item.History).ToArray(), item.PathCost + x.Path.Sum(c => c.Value), x.Direction))
				;
			//break;
			if (log(item.Node.Position))
				next.Dump();
				
			var improves = next
				.Do(x => log(item.Node.Position) ? x.Select(y => new { y.Node.Position, y.Node.Visited, y.Node.Tentative[(int)y.Direction]?.PathId, y.Node.Tentative[(int)y.Direction]?.PathCost, NewCost = y.PathCost }).Dump() : null)
				.Where(x => !x.Node.IsVisited(x.Direction))
				.Where(x => x.Node.Tentative[(int)x.Direction] == null || x.Node.Tentative[(int)x.Direction].PathCost > x.PathCost)
				//.Dump()
				;

			if (log(item.Node.Position))
				improves.Dump();
			foreach (var target in improves)
			{
				if (log(target)) $"Potential {target.Node} from {item}".Dump();
				if (target.Node.Tentative != null)
				{
					if (log(target) || log(target.Node.Tentative[(int)target.Direction])) $"Replace {target.Node} for {target.Direction} {target.Node.Tentative[(int)target.Direction]} with {target}".Dump();
					queue.Remove(target.Node.Tentative[(int)target.Direction]);
				}

				target.Node.Tentative[(int)target.Direction] = target;
				var before = queue.FirstOrDefault(x => x.PathCost >= target.PathCost);
				if (before == null)
				{
					queue.Add(target);
				}
				else
				{
					var index = queue.IndexOf(before);
					queue.Insert(index, target);
				}
			}
			if (log(item))
				queue.Dump("Queue");
		}

		var route = destination.Tentative.OrderBy(x => x?.PathCost ?? Int32.MaxValue).First();
		if (route == null)
		{
			destination.Dump("No Result");
			return;
		}
		// Part 1: 843
		// Part 2: 1017
		$"Got to {destination} after {step} at cost {string.Join(",", destination.Tentative.Where((x, i) => destination.Visited[i]).Select(x => x.PathCost).Single())}".Dump("Result");
		var pathGraph = map.Values
			.GroupBy(x => x.Position.Y)
			.OrderBy(x => x.Key)
			.Select(x => string.Join("", x.OrderBy(x => x.Position.X).Select(c => route.History.Any(h => h.Position == c.Position) ? "." : c.Value.ToString())));
			
		#if VIDEO
		foreach (var point in route.History)
		{
			g.FillEllipse(Brushes.CadetBlue, point.Position.X * spotSize, point.Position.Y * spotSize, spotSize+1, spotSize+1);
			gif.WriteFrame(image);
		}
		#endif

		Clipboard.SetText(
			string.Join("\r\n", pathGraph.Select(x => string.Join("\t", x.ToCharArray()))));
		map.Values
			.GroupBy(x => x.Position.Y)
			.OrderBy(x => x.Key)
			.Select(x => string.Join("", x.OrderBy(x => x.Position.X).Select(c => c.Visited[1] ? "|" : c.Visited[2] ? "-" : c.Value.ToString())))
			.Dump();
#if GIF
	}
#endif
}

public enum Direction { None = 0, Horizontal = 1, Vertial = 2 };

public record Coordinate(int X, int Y)
{
	public static implicit operator Coordinate((int X, int Y) v) => new Coordinate(v.X, v.Y);

	public static Coordinate operator -(Coordinate a, Coordinate b) => new Coordinate(a.X - b.X, a.Y - b.Y);
	public static Coordinate operator +(Coordinate a, Coordinate b) => new Coordinate(a.X + b.X, a.Y + b.Y);
	object ToDump() => ToString();
	public override string ToString() => $"{(X, Y)}";
}

public record Path(Vertex Node, Vertex[] History, int PathCost, Direction Direction)
{
	public int PathId { get; } = _pathId++;
	object ToDump() => new { Node, PathCost };
	public static implicit operator Coordinate(Path v) => v?.Node.Position;
}
private static int _pathId = 0;
public class Vertex
{
	public Vertex(int x, int y, int value)
	{
		Position = new Coordinate(x, y);
		Value = value;
	}
	public int Value { get; }
	public Coordinate Position { get; }

	public bool[] Visited { get; set; } = new bool[3];
	public bool IsVisited(Direction direction) => Visited[(int)direction];
	public bool SetVisited(Direction direction) => Visited[(int)direction] = true;

	public Path[] Tentative { get; } = new Path[3];

	object ToDump() => ToString();
	public override string ToString() => $"{Position} {Value}";
	public static implicit operator Coordinate(Vertex v) => v.Position;
}

static IEnumerable<int> Ruler(int count, int start = 0, int step = 1) => new int[count].Select((_, i) => i * step + start);

public static class Ext
{
	public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue @default = default)
	{
		return dict.TryGetValue(key, out var v) ? v : @default;
	}
	public static T[] Add<T>(this T[] source, T item)
	{
		return source.Concat(new[] { item }).ToArray();
	}
	public static T[] AddFirst<T>(this T[] source, T item)
	{
		return new[] { item }.Concat(source).ToArray();
	}
	public static T Do<T>(this T item, Action<T> action)
	{
		action(item);
		return item;
	}
	public static T Do<T>(this T item, Func<T, object> action)
	{
		action(item);
		return item;
	}
	public static IEnumerable<(T Item, Func<IEnumerable<T>, IEnumerable<T>> More)> Recurse<T>(this IEnumerable<T> init)
	{
		var list = init.ToList();
		var index = 0;
		IEnumerable<T> More(IEnumerable<T> items)
		{
			list.AddRange(items);
			return items;
		}
		while (list.Count > index && index < 5000000)
			yield return (list[index++], More);

		//index.Dump("Loop exit");
	}
}


/// <summary>
/// Creates a GIF using .Net GIF encoding and additional animation headers.
/// </summary>
public class GifWriter : IDisposable
{
	#region Fields
	const long SourceGlobalColorInfoPosition = 10,
		SourceImageBlockPosition = 789;

	readonly BinaryWriter _writer;
	bool _firstFrame = true;
	readonly object _syncLock = new object();
	#endregion

	/// <summary>
	/// Creates a new instance of GifWriter.
	/// </summary>
	/// <param name="OutStream">The <see cref="Stream"/> to output the Gif to.</param>
	/// <param name="DefaultFrameDelay">Default Delay between consecutive frames... FrameRate = 1000 / DefaultFrameDelay.</param>
	/// <param name="Repeat">No of times the Gif should repeat... -1 not to repeat, 0 to repeat indefinitely.</param>
	public GifWriter(Stream OutStream, int DefaultFrameDelay = 500, int Repeat = -1)
	{
		if (OutStream == null)
			throw new ArgumentNullException(nameof(OutStream));

		if (DefaultFrameDelay <= 0)
			throw new ArgumentOutOfRangeException(nameof(DefaultFrameDelay));

		if (Repeat < -1)
			throw new ArgumentOutOfRangeException(nameof(Repeat));

		_writer = new BinaryWriter(OutStream);
		this.DefaultFrameDelay = DefaultFrameDelay;
		this.Repeat = Repeat;
	}

	/// <summary>
	/// Creates a new instance of GifWriter.
	/// </summary>
	/// <param name="FileName">The path to the file to output the Gif to.</param>
	/// <param name="DefaultFrameDelay">Default Delay between consecutive frames... FrameRate = 1000 / DefaultFrameDelay.</param>
	/// <param name="Repeat">No of times the Gif should repeat... -1 not to repeat, 0 to repeat indefinitely.</param>
	public GifWriter(string FileName, int DefaultFrameDelay = 500, int Repeat = -1)
		: this(new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read), DefaultFrameDelay, Repeat) { }

	#region Properties
	/// <summary>
	/// Gets or Sets the Default Width of a Frame. Used when unspecified.
	/// </summary>
	public int DefaultWidth { get; set; }

	/// <summary>
	/// Gets or Sets the Default Height of a Frame. Used when unspecified.
	/// </summary>
	public int DefaultHeight { get; set; }

	/// <summary>
	/// Gets or Sets the Default Delay in Milliseconds.
	/// </summary>
	public int DefaultFrameDelay { get; set; }

	/// <summary>
	/// The Number of Times the Animation must repeat.
	/// -1 indicates no repeat. 0 indicates repeat indefinitely
	/// </summary>
	public int Repeat { get; }
	#endregion

	/// <summary>
	/// Adds a frame to this animation.
	/// </summary>
	/// <param name="Image">The image to add</param>
	/// <param name="Delay">Delay in Milliseconds between this and last frame... 0 = <see cref="DefaultFrameDelay"/></param>
	public void WriteFrame(Image Image, int Delay = 0)
	{
		lock (_syncLock)
			using (var gifStream = new MemoryStream())
			{
				Image.Save(gifStream, ImageFormat.Gif);

				// Steal the global color table info
				if (_firstFrame)
					InitHeader(gifStream, _writer, Image.Width, Image.Height);

				WriteGraphicControlBlock(gifStream, _writer, Delay == 0 ? DefaultFrameDelay : Delay);
				WriteImageBlock(gifStream, _writer, !_firstFrame, 0, 0, Image.Width, Image.Height);
			}

		if (_firstFrame)
			_firstFrame = false;
	}

	#region Write
	void InitHeader(Stream SourceGif, BinaryWriter Writer, int Width, int Height)
	{
		// File Header
		Writer.Write("GIF".ToCharArray()); // File type
		Writer.Write("89a".ToCharArray()); // File Version

		Writer.Write((short)(DefaultWidth == 0 ? Width : DefaultWidth)); // Initial Logical Width
		Writer.Write((short)(DefaultHeight == 0 ? Height : DefaultHeight)); // Initial Logical Height

		SourceGif.Position = SourceGlobalColorInfoPosition;
		Writer.Write((byte)SourceGif.ReadByte()); // Global Color Table Info
		Writer.Write((byte)0); // Background Color Index
		Writer.Write((byte)0); // Pixel aspect ratio
		WriteColorTable(SourceGif, Writer);

		// App Extension Header for Repeating
		if (Repeat == -1)
			return;

		Writer.Write(unchecked((short)0xff21)); // Application Extension Block Identifier
		Writer.Write((byte)0x0b); // Application Block Size
		Writer.Write("NETSCAPE2.0".ToCharArray()); // Application Identifier
		Writer.Write((byte)3); // Application block length
		Writer.Write((byte)1);
		Writer.Write((short)Repeat); // Repeat count for images.
		Writer.Write((byte)0); // terminator
	}

	static void WriteColorTable(Stream SourceGif, BinaryWriter Writer)
	{
		SourceGif.Position = 13; // Locating the image color table
		var colorTable = new byte[768];
		SourceGif.Read(colorTable, 0, colorTable.Length);
		Writer.Write(colorTable, 0, colorTable.Length);
	}

	static void WriteGraphicControlBlock(Stream SourceGif, BinaryWriter Writer, int FrameDelay)
	{
		SourceGif.Position = 781; // Locating the source GCE
		var blockhead = new byte[8];
		SourceGif.Read(blockhead, 0, blockhead.Length); // Reading source GCE

		Writer.Write(unchecked((short)0xf921)); // Identifier
		Writer.Write((byte)0x04); // Block Size
		Writer.Write((byte)(blockhead[3] & 0xf7 | 0x08)); // Setting disposal flag
		Writer.Write((short)(FrameDelay / 10)); // Setting frame delay
		Writer.Write(blockhead[6]); // Transparent color index
		Writer.Write((byte)0); // Terminator
	}

	static void WriteImageBlock(Stream SourceGif, BinaryWriter Writer, bool IncludeColorTable, int X, int Y, int Width, int Height)
	{
		SourceGif.Position = SourceImageBlockPosition; // Locating the image block
		var header = new byte[11];
		SourceGif.Read(header, 0, header.Length);
		Writer.Write(header[0]); // Separator
		Writer.Write((short)X); // Position X
		Writer.Write((short)Y); // Position Y
		Writer.Write((short)Width); // Width
		Writer.Write((short)Height); // Height

		if (IncludeColorTable) // If first frame, use global color table - else use local
		{
			SourceGif.Position = SourceGlobalColorInfoPosition;
			Writer.Write((byte)(SourceGif.ReadByte() & 0x3f | 0x80)); // Enabling local color table
			WriteColorTable(SourceGif, Writer);
		}
		else Writer.Write((byte)(header[9] & 0x07 | 0x07)); // Disabling local color table

		Writer.Write(header[10]); // LZW Min Code Size

		// Read/Write image data
		SourceGif.Position = SourceImageBlockPosition + header.Length;

		var dataLength = SourceGif.ReadByte();
		while (dataLength > 0)
		{
			var imgData = new byte[dataLength];
			SourceGif.Read(imgData, 0, dataLength);

			Writer.Write((byte)dataLength);
			Writer.Write(imgData, 0, dataLength);
			dataLength = SourceGif.ReadByte();
		}

		Writer.Write((byte)0); // Terminator
	}
	#endregion

	/// <summary>
	/// Frees all resources used by this object.
	/// </summary>
	public void Dispose()
	{
		// Complete File
		_writer.Write((byte)0x3b); // File Trailer

		_writer.BaseStream.Dispose();
		_writer.Dispose();
	}
}