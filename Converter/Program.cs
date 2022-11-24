// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Presentation;
using Text = DocumentFormat.OpenXml.Drawing.Text;

Directory.SetCurrentDirectory("C:\\Users\\Renat\\RiderProjects\\SongList\\Converter\\");
var files = Directory.GetFiles("sources").Select(x => new FileInfo(x));

var songs = new List<object>();

foreach (var file in files)
{
    using var pres = PresentationDocument.Open(file.FullName, false);
    var presPart = pres.PresentationPart;
    var slideIds = presPart.Presentation.SlideIdList.ChildElements.Select(x => ((SlideId)x).RelationshipId);
    var slides = slideIds.Select(x=> presPart.GetPartById(x) as SlidePart);
    var songTexts = new List<string>();
    var regex = new Regex(@"\w");
    int? number = null;
    string note = null;
    foreach (var slide in slides)
    {
        var shapes = slide.Slide.Descendants<DocumentFormat.OpenXml.Presentation.Shape>()
            .Where(x=> x.Descendants<Text>().Any())
            .ToArray();
        var textShapes = shapes.Length switch
        {
            0 => shapes,
            1 => shapes,
            2 => shapes.Take(1).ToArray(),
            _ => throw new Exception()
        };
        var texts = textShapes.Select(x => x.Descendants<Text>().Select(x => x.Text.Trim()).Where(x=> x != "")).SelectMany(x=> x).ToArray();
        var sb = new StringBuilder();
        foreach (var s in texts)
        {
            if (regex.IsMatch(s))
            {
                sb.Append(" ");
            }
            sb.Append(s);
        }

        var numberShape = shapes.Skip(1).FirstOrDefault();
        if (numberShape != null)
        {
            var shapeText = string.Join(" ", numberShape.Descendants<Text>().Select(x => x.Text.Trim()).Where(x => x != ""));
            var match = Regex.Match(shapeText, @"№\s*(\d[\d|\s]*)");
            if (match.Success)
            {
                number = int.Parse(match.Groups[1].Value.Replace(" ", ""));
            }

            match = Regex.Match(shapeText, @"([^\w]|^)(до|ре|ми|фа|соль|ля|си)\s*([♭|#])?$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var rawNote = match.Groups[2].Value;
                var modifier = match.Groups[3].Value;
                note = rawNote[..1].ToUpper() + rawNote[1..].ToLower() + modifier;
                Console.WriteLine(shapeText + "    |||" + note);
            }
        }


        var text = sb.ToString().Trim();

        if (text.Length > 0)
        {
            songTexts.Add(text);
        }
        //var a = shapes[0].ShapeProperties.Transform2D;
        var paragraphs = slide.Slide.Descendants<Paragraph>();
        //var texts = paragraphs.SelectMany(x => x.Descendants<Text>()).Select(x=> x.Text).ToArray();
    }
    
    
    songs.Add(new
    {
        Text = string.Join("\n", songTexts),
        Title = file.Name.Replace(".pptx", ""),
        Number = number,
        Note = note
    });
}

var cl = new HttpClient();
await cl.PostAsJsonAsync("http://localhost:5241/song", songs);


Console.WriteLine("Hello, World!");