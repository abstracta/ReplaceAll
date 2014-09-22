using System;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace Abstracta.ReplaceAll
{
    public class Program
    {
        // Define a class to receive parsed values
        internal class Options
        {
            [Option('i', "inputFile", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('o', "outputFile", HelpText = "Output file name")]
            public string OutputFile { get; set; }

            public string DefaultOutputFile
            {
                get { return InputFile; }
            }

            [Option('x', "isRegex", DefaultValue = false, HelpText = "Match text to replace using regular expressions. By default uses 'equals' operator. Use groups to match and preserve text.")]
            public Boolean IsRegex { get; set; }

            [Option('t', "isTemplate", DefaultValue = false, HelpText = "When using regExp, the default template is '$1TextToReplace$2TextToReplace..$n'. With this option you define the template.")]
            public Boolean TextToReplaceIsTemplate { get; set; }

            [Option('r', "textToReplace", Required = true, HelpText = "Text that will be replaced")]
            public string TextToBeReplaced { get; set; }

            [Option('w', "replaceWith", Required = true, HelpText = "Text that will replace the other text")]
            public string TextToReplace { get; set; }

            [Option("fromLine", HelpText = "Replaces the lines starting at this line index. Default value = 0")]
            public long FromLine { get; set; }

            [Option("toLine", HelpText = "Replaces the lines until this line index. Default value = MAXLONG")]
            public long ToLine { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine("File doesn't exists: {0}", options.InputFile);
                return;
            }

            if (options.TextToReplaceIsTemplate && !options.IsRegex)
            {
                Console.WriteLine("Option 'isTemplate' only can used when using also option 'isRegularExpresion'");
                return;
            }

            if (options.OutputFile == null || options.OutputFile.Trim() == string.Empty)
            {
                options.OutputFile = options.DefaultOutputFile;
            }

            long fromLine = 0;
            if (args.Any(a => a == "--fromLine"))
            {
                fromLine = options.FromLine;
            }

            var toLine = long.MaxValue;
            if (args.Any(a => a == "--toLine"))
            {
                toLine = options.ToLine;
            }

            var fileModified = Replacer.ReeplaceInFile(options.InputFile, options.OutputFile, options.TextToBeReplaced,
                                                       options.TextToReplace, options.IsRegex,
                                                       options.TextToReplaceIsTemplate, fromLine, toLine);

            if (fileModified)
            {
                Console.WriteLine(options.InputFile == options.OutputFile ? "File modified: {0}" : "File created: {0}",
                                  options.OutputFile);
            }
            else
            {
                Console.WriteLine("File not modified, no matching text found.");
            }
        }
    }
}
