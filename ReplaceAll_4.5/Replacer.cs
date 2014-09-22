using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Abstracta.ReplaceAll
{
    public class Replacer
    {
        public static bool ReeplaceInFile(string inputFile, string outputFile, string textToBeReplaced, string textToReplace, bool matchByRegex, bool textToReplaceIsTemplate, long fromLine = 0, long toLine = long.MaxValue)
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
                    long i = 0;
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (fromLine <= i && i <= toLine)
                        {
                            var newLine = matchByRegex
                                ? ReplaceUsingRegularExpression(line, textToBeReplaced, textToReplace, textToReplaceIsTemplate)
                                : ReplaceUsingEqualsOperator(line, textToBeReplaced, textToReplace);

                            newFile.WriteLine(newLine);

                            fileModified = fileModified || newLine != line;
                        }
                        else
                        {
                            newFile.WriteLine(line);
                        }

                        i++;
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
