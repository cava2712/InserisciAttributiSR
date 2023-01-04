using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace InserisciAttributiSR
{
    internal class Program
    {
        static void Main()
        {
            var entitiesPath = @"C:\Temp\Entities";
            if (!Directory.Exists(entitiesPath))
            {
                throw new Exception("Directory path does not exist in file system");
            }

            var files = Directory.GetFiles(entitiesPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!IsEnumFile(file))
                    continue;
                InserisciAttributiNelFileERitornaStringResources(file);
            }
        }

        private static void InserisciAttributiNelFileERitornaStringResources(string file)
        {
            var lines = File.ReadAllLines(file);
            StringBuilder sbSourceCode = new StringBuilder();
            StringBuilder sbResources = new StringBuilder();

            bool inEnum = false;
            bool descriptionAttributeFound = false;
            foreach (string line in lines)
            {

                if (!inEnum)
                {
                    inEnum = IsEnumDeclare(line);
                    sbSourceCode.AppendLine(line);
                    continue;
                }

                if (IsDescriptionAttribute(line))
                {
                    descriptionAttributeFound = true;
                }
                else if (IsEnumEntry(line))
                {
                    if (!descriptionAttributeFound)
                    {
                        sbSourceCode.AppendLine(GetIndentation(line) + string.Format("[SRDescription(\"{0}\")]", GetEnumEntryName(line)));
                        sbResources.AppendLine(string.Format("{0}\t{1}", GetEnumEntryName(line), Uncamel(GetEnumEntryName(line))));
                    }

                    descriptionAttributeFound = false;
                }
                else if (IsEnumDeclareEnd(line))
                {
                    inEnum = false;
                }


                sbSourceCode.AppendLine(line);
            }

            File.AppendAllText("C:\\TEMP\\EnumRes.txt", sbResources.ToString());
            File.WriteAllText(file + ".result.txt", sbSourceCode.ToString());
        }

        private static string Uncamel(string line)
        {
            string result = line[0].ToString();
            for (int i = 1; i < line.Length; i++)
            {
                if (line[i] < 'a')
                    result += " " + line[i].ToString().ToLower();
                else
                    result += line[i];
            }

            return result;
        }

        private static string GetEnumEntryName(string line)
        {
            Regex regex = new Regex(@"^\s*(?<enumName>[a-zA-Z][a-zA-Z0-9]*)");
            var match = regex.Match(line);
            return match.Groups["enumName"].Value;
        }

        private static string GetIndentation(string line)
        {
            string result = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] > ' ')
                    return result;

                result += line[i];
            }

            return result;
        }

        private static bool IsEnumDeclareEnd(string line)
        {
            return line.Contains("}");
        }

        private static bool IsEnumEntry(string line)
        {
            return Regex.IsMatch(line, @"^\s*[a-zA-Z]");
        }

        private static bool IsDescriptionAttribute(string line)
        {
            return line.Contains("[SRDescription") || line.Contains("[Description");
        }

        private static bool IsEnumDeclare(string line)
        {
            return line.Contains("enum");
        }

        private static bool IsEnumFile(string file)
        {
            return File.ReadAllText(file).Contains("public enum");
        }
    }
}
