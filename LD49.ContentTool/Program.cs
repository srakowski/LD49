using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LD49.ContentTool
{
	class Program
	{
		static void Main(string[] args)
		{
			var root = @"./";

			var contentFileName = Directory.GetFiles(root, "*.mgcb")
				.FirstOrDefault();

			if (contentFileName == null)
			{
				Console.WriteLine("No content file in directory");
				return;
			}

			var fileLines = File.ReadAllLines(contentFileName);

			var contentFile = new ContentFile();
			contentFile.Load(fileLines);

			var contentDirectories = Directory.GetDirectories(root)
				.Where(d => contentDirectoryNames.Contains(Path.GetFileName(d)))
				.ToArray();

			var codeBuilder = BeginContentCode();
			foreach (var directory in contentDirectories)
			{
				var groupName = Path.GetFileName(directory);
				var createEntry = ContentFileEntry.GetEntryConstructor(groupName);
				StartContentClass(codeBuilder, groupName);
				var files = Directory.GetFiles(directory);
				var props = new List<string>();
				var i = 0;
				foreach (var file in files)
				{
					var propertyName = Path.GetFileNameWithoutExtension(file);
					var assetName = $"{groupName}/{propertyName}";
					props.Add(assetName);

					var sanitizedPropertyName = GetPropertyName(propertyName);

					WriteIdx(codeBuilder, sanitizedPropertyName, assetName, i);
					i++;

					var entryFileName = $"{groupName}/{Path.GetFileName(file)}";
					var entry = contentFile.Entries.FirstOrDefault(e => e.Name == entryFileName);
					if (entry == null)
					{
						contentFile.Entries.Add(createEntry(entryFileName));
					}
					else
					{
						entry.Found = true;
					}
				}

				WriteLoadAll(codeBuilder, groupName, props);


				EndContentClass(codeBuilder);
			}
			var code = EndContentCode(codeBuilder);
			var output = Path.Combine(root, "ContentIndex.cs");
			File.WriteAllText(output, code);

			var data = contentFile.ToString();
			File.WriteAllLines($"{contentFileName}.old", fileLines);
			File.WriteAllText($"{contentFileName}", data);
		}

		private static string[] contentDirectoryNames = new string[]
		{
			"Effect",
			"SpriteFont",
			"Texture2D",
			"Song",
			"SoundEffect"
		};

		class ContentFile
		{
			public List<string> HeaderLines { get; } = new List<string>();

			public List<ContentFileEntry> Entries { get; } = new List<ContentFileEntry>();

			internal void Load(string[] fileLines)
			{
				var lineIdx = 0;
				while (!fileLines[lineIdx].Contains("Content"))
				{
					HeaderLines.Add(fileLines[lineIdx]);
					lineIdx++;
				}
				HeaderLines.Add(fileLines[lineIdx]);
				lineIdx++;

				while (lineIdx < fileLines.Length)
				{
					while (lineIdx < fileLines.Length && string.IsNullOrWhiteSpace(fileLines[lineIdx]))
						lineIdx++;

					if (lineIdx >= fileLines.Length)
						break;

					var entry = new ContentFileEntry
					{
						Name = fileLines[lineIdx].Replace("#begin ", "").Trim(),
						Found = false,
					};
					while (lineIdx < fileLines.Length && !string.IsNullOrWhiteSpace(fileLines[lineIdx]))
					{
						entry.Lines.Add(fileLines[lineIdx]);
						lineIdx++;
					}
					Entries.Add(entry);
				}
			}

			public override string ToString()
			{
				var sb = new StringBuilder(string.Join("\r\n", HeaderLines));
				sb.AppendLine();
				sb.AppendLine();
				foreach (var entry in Entries.Where(e => e.Found))
				{
					sb.AppendLine(string.Join("\r\n", entry.Lines));
					sb.AppendLine();
				}
				return sb.ToString();
			}
		}

		class ContentFileEntry
		{
			public string Name { get; set; }

			public bool Found { get; set; } = true;

			public List<string> Lines { get; } = new List<string>();

			public static Func<string, ContentFileEntry> GetEntryConstructor(string type)
			{
				switch (type)
				{
					case "Effect": return CreateEffect;
					case "SpriteFont": return CreateSpriteFont;
					case "Texture2D": return CreateTexture2D;
					case "Song": return CreateSong;
					case "SoundEffect": return CreateSoundEffect;
					default: return null;
				}
			}

			private static ContentFileEntry CreateEffect(string name)
			{
				var entry = new ContentFileEntry { Name = name };
				entry.Lines.Add($"#begin {name}");
				entry.Lines.Add($"/importer:EffectImporter");
				entry.Lines.Add($"/processor:EffectProcessor");
				entry.Lines.Add($"/processorParam:DebugMode=Auto");
				entry.Lines.Add($"/build:{name}");
				return entry;
			}

			private static ContentFileEntry CreateSpriteFont(string name)
			{
				var entry = new ContentFileEntry { Name = name };
				entry.Lines.Add($"#begin {name}");
				entry.Lines.Add($"/importer:FontDescriptionImporter");
				entry.Lines.Add($"/processor:FontDescriptionProcessor");
				entry.Lines.Add($"/processorParam:PremultiplyAlpha=True");
				entry.Lines.Add($"/processorParam:TextureFormat=Compressed");
				entry.Lines.Add($"/build:{name}");
				return entry;
			}

			private static ContentFileEntry CreateTexture2D(string name)
			{
				var entry = new ContentFileEntry { Name = name };
				entry.Lines.Add($"#begin {name}");
				entry.Lines.Add($"/importer:TextureImporter");
				entry.Lines.Add($"/processor:TextureProcessor");
				entry.Lines.Add($"/processorParam:ColorKeyColor=255,0,255,255");
				entry.Lines.Add($"/processorParam:ColorKeyEnabled=True");
				entry.Lines.Add($"/processorParam:GenerateMipmaps=False");
				entry.Lines.Add($"/processorParam:PremultiplyAlpha=True");
				entry.Lines.Add($"/processorParam:ResizeToPowerOfTwo=False");
				entry.Lines.Add($"/processorParam:MakeSquare=False");
				entry.Lines.Add($"/processorParam:TextureFormat=Color");
				entry.Lines.Add($"/build:{name}");
				return entry;
			}

			private static ContentFileEntry CreateSong(string name)
			{
				var entry = new ContentFileEntry { Name = name };
				entry.Lines.Add($"#begin {name}");
				entry.Lines.Add($"/importer:Mp3Importer");
				entry.Lines.Add($"/processor:SongProcessor");
				entry.Lines.Add($"/processorParam:Quality=Best");
				entry.Lines.Add($"/build:{name}");
				return entry;
			}

			private static ContentFileEntry CreateSoundEffect(string name)
			{
				var entry = new ContentFileEntry { Name = name };
				entry.Lines.Add($"#begin {name}");
				entry.Lines.Add($"/importer:WavImporter");
				entry.Lines.Add($"/processor:SoundEffectProcessor");
				entry.Lines.Add($"/processorParam:Quality=Best");
				entry.Lines.Add($"/build:{name}");
				return entry;
			}
		}

		private static string GetPropertyName(string propertyName)
		{
			return new string(propertyName.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
		}

		private static StringBuilder BeginContentCode()
		{
			var codeBuilder = new StringBuilder();
			codeBuilder.AppendLine("using Microsoft.Xna.Framework.Audio;");
			codeBuilder.AppendLine("using Microsoft.Xna.Framework.Content;");
			codeBuilder.AppendLine("using Microsoft.Xna.Framework.Graphics;");
			codeBuilder.AppendLine("using Microsoft.Xna.Framework.Media;");
			codeBuilder.AppendLine("using System.Collections.Generic;");
			codeBuilder.AppendLine();
			codeBuilder.AppendLine("namespace LD49");
			codeBuilder.AppendLine("{");
			return codeBuilder;
		}

		private static void StartContentClass(StringBuilder codeBuilder, string name)
		{
			codeBuilder.AppendLine($"\tpublic static class {name}s");
			codeBuilder.AppendLine("\t{");
		}

		private static void WriteIdx(StringBuilder codeBuilder, string propertyName, string name, int idx)
		{
			codeBuilder.AppendLine($"\t\tpublic const string {propertyName} = \"{name}\";");
		}

		private static void WriteLoadAll(StringBuilder codeBuilder, string type, List<string> props)
		{
			codeBuilder.AppendLine($"\t\tpublic static Dictionary<string, {type}> Load{type}s(this ContentManager content)");
			codeBuilder.AppendLine($"\t\t{{");
			codeBuilder.AppendLine($"\t\t\treturn new Dictionary<string, {type}>");
			codeBuilder.AppendLine($"\t\t\t{{");
			foreach (var prop in props)
			{
				codeBuilder.AppendLine($"\t\t\t\t[\"{prop}\"] = content.Load<{type}>(\"{prop}\"),");
			}
			codeBuilder.AppendLine($"\t\t\t}};");
			codeBuilder.AppendLine($"\t\t}}");
		}

		private static void EndContentClass(StringBuilder codeBuilder)
		{
			codeBuilder.AppendLine("\t}");
		}

		private static string EndContentCode(StringBuilder codeBuilder)
		{
			codeBuilder.AppendLine("}");
			return codeBuilder.ToString();
		}
	}
}
