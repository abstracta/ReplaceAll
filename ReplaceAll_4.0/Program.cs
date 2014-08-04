using System.Linq;

namespace ReplaceAll
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using CommandLine;
    using CommandLine.Text;

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

            var fileModified = ReeplaceInFile(options.InputFile, options.OutputFile, options.TextToBeReplaced, options.TextToReplace, options.IsRegex, options.TextToReplaceIsTemplate);

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

        private static bool ReeplaceInFile(string inputFile, string outputFile, string textToBeReplaced, string textToReplace, bool matchByRegex, bool textToReplaceIsTemplate)
        {
            // TODO : add support for other 'special caracters' 
            textToReplace = textToReplace.Replace("\\t", "\t");
            textToReplace = textToReplace.Replace("\\n", "\n");
            textToReplace = textToReplace.Replace("\\r", "\r");

            textToBeReplaced = textToBeReplaced.Replace("\\t", "\t");
            textToBeReplaced = textToBeReplaced.Replace("\\n", "\n");
            textToBeReplaced = textToBeReplaced.Replace("\\r", "\r");

            var replaceOriginalFile = outputFile == inputFile;
            if (replaceOriginalFile)
            {
                var tmpName = DateTime.Now - DateTime.MinValue;
                outputFile = tmpName.TotalMilliseconds + "_tmpFile.log";
            }

            var fileModified = false;
            using (var newFile = new StreamWriter(outputFile))
            {
                using (var file = new StreamReader(inputFile))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        var newLine = matchByRegex
                                ? ReplaceUsingRegularExpression(line, textToBeReplaced, textToReplace, textToReplaceIsTemplate)
                                : ReplaceUsingEqualsOperator(line, textToBeReplaced, textToReplace);

                        newFile.WriteLine(newLine);

                        fileModified = fileModified || newLine != line;
                    }
                }
            }

            if (replaceOriginalFile && fileModified)
            {
                File.Delete(inputFile);
                File.Move(outputFile, inputFile);
            }
            else if (!fileModified)
            {
                File.Delete(outputFile);
            }

            return fileModified;
        }

        private static string ReplaceUsingEqualsOperator(string line, string textToBeReplaced, string textToReplace)
        {
            return line.Replace(textToBeReplaced, textToReplace);
        }

        private static string ReplaceUsingRegularExpression(string line, string textToBeReplaced, string textToReplace, bool textToReplaceIsTemplate)
        {
            var pattern = textToBeReplaced; //  @"\p{Sc}*(\s?\d+[.,]?\d*)\p{Sc}*";
            var replacement = (textToReplaceIsTemplate) ? textToReplace : CreateTemplate(pattern, textToReplace);
            var input = line; // "$16.32 12.19 £16.29 €18.29  €18,29";

            return Regex.Replace(input, pattern, replacement);
        }

        private static string CreateTemplate(string pattern, string textToReplace)
        {
            var result = string.Empty;

            var startsWithGroup = pattern[0] == '(';
            var groupsCount = pattern.Count(t => t == '(');
            var endsWithGroup = pattern[pattern.Length - 1] == ')';

            if (groupsCount > 0)
            {
                var i = 1;
                if (startsWithGroup)
                {
                    result = "$1";
                    i++;
                }

                for (; i <= groupsCount; i++)
                {
                    result += textToReplace + "$" + i;
                }

                if (!endsWithGroup)
                {
                    result += textToReplace;
                }
            }
            else
            {
                result = textToReplace;
            }

            return result;
        }
    }
}
