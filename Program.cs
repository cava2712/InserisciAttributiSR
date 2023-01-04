using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace InserisciAttributiSR
{
    internal class Program
    {
        static void Main()
        {
            var entitiesPath = @"C:\Temp\Entities";
            var resourcesPath = @"C:\Temp\AttributeResources.txt";
            if (!Directory.Exists(entitiesPath))
            {
                throw new Exception("Directory path does not exist in file system");
            }

            var files = Directory.GetFiles(entitiesPath, "*.cs", SearchOption.AllDirectories);
            string resources = string.Empty;
            foreach (var file in files)
            {
                if (!IsEnumFile(file))
                    continue;
                resources += "FILE:" + file + "\r\n";
                resources += InseriesciAttributiNelFileERitornaStringResources(file);
            }
            File.WriteAllText(resourcesPath, string.Empty);
            File.WriteAllText(resourcesPath, resources);
        }

        private static string InseriesciAttributiNelFileERitornaStringResources(string file)
        {
            var f = File.ReadAllText(file);
            string resources = string.Empty;
            f = RimuoviCommentiDaStringa(f);
            int i = f.IndexOf("enum", StringComparison.Ordinal);
            while (f[i] != '{')
            {
                i++;
            }
            i++; // Entro dentro le parentesi graffe

            var enumCompleti = LeggiTuttiGliEnumCompleti(f, i);

            f = File.ReadAllText(file); // Ripristino i file con anche i commenti
            foreach (var enumCorrenteCompleto in enumCompleti)
            {
                var enumCorrenteNoTrim = enumCorrenteCompleto.TrimEnd().TrimStart();
                if (enumCorrenteNoTrim.Contains("[SRDescription") || enumCorrenteNoTrim.Contains("[Description"))
                    continue;
                var nomeEnumCorrente = TrovaNomeEnum(enumCorrenteNoTrim);
                resources += ScriviInFileResources(nomeEnumCorrente);
                string stringSpazi = StringaSpaziIniziali(enumCorrenteCompleto);
                f = f.Replace(
                    enumCorrenteNoTrim,
                     "\r\n" + stringSpazi + $"[SRDescription(AttributeResources.{nomeEnumCorrente})]" + "\r\n" + stringSpazi + enumCorrenteNoTrim);
            }

            File.WriteAllText(file, string.Empty);
            File.WriteAllText(file, f);

            return resources;
        }

        private static string ScriviInFileResources(string nomeResources)
        {
            var s = nomeResources + '\t';
            int i = 1;
            s += nomeResources[0];
            while (i < nomeResources.Length)
            {
                if (char.IsLower(nomeResources[i]))
                {
                    s += nomeResources[i];
                }
                else
                {
                    s += " " + char.ToLower(nomeResources[i]);
                }
                i++;
            }

            return s + "\r\n";
        }

        private static string StringaSpaziIniziali(string enumCorrenteCompleto)
        {
            string s = "";
            enumCorrenteCompleto = enumCorrenteCompleto.Replace("\r\n", String.Empty);
            int i = 0;
            while (i < enumCorrenteCompleto.Length && enumCorrenteCompleto[i] == ' ')
            {
                s += " ";
                i++;
            }

            return s;
        }

        private static string TrovaNomeEnum(string enumCorrenteCompleto)
        {
            string nomeEnumCorrente = string.Empty;
            var enumCorrenteCompletoNoSpaziIniziali = enumCorrenteCompleto.Replace("\r\n", String.Empty).TrimStart();

            for (int j = 0; j < enumCorrenteCompletoNoSpaziIniziali.Length; j++)
            {
                if (enumCorrenteCompletoNoSpaziIniziali[j] != ' ')
                    nomeEnumCorrente += enumCorrenteCompletoNoSpaziIniziali[j];
                else
                {
                    break;
                }
            }

            return nomeEnumCorrente;
        }

        private static string[] LeggiTuttiGliEnumCompleti(string s, int index)
        {
            List<string> enumCompleti = new List<string>();
            while (index < s.Length)
            {
                var stringaEnumCorrenteCompleta = string.Empty;
                while (s[index] != ',' && s[index] != '}')
                {
                    stringaEnumCorrenteCompleta += s[index];
                    index++;
                }
                stringaEnumCorrenteCompleta =
                    stringaEnumCorrenteCompleta.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                if (!string.IsNullOrEmpty(stringaEnumCorrenteCompleta.Replace(" ", "")))
                {
                    enumCompleti.Add(stringaEnumCorrenteCompleta);
                }
                if (s[index] == '}')
                    break;
                index++;
            }
            return enumCompleti.ToArray();
        }

        private static string RimuoviCommentiDaStringa(string stringaEnumCorrenteCompleta)
        {
            stringaEnumCorrenteCompleta = Regex.Replace(stringaEnumCorrenteCompleta, "//(.*?)\r?\n", string.Empty);
            return Regex.Replace(stringaEnumCorrenteCompleta, "(/\\*([^*]|[\\r\\n]|(\\*+([^*/]|[\\r\\n])))*\\*+/)|(//.*)", string.Empty);
        }

        private static bool IsEnumFile(string file)
        {
            return File.ReadAllText(file).Contains("public enum");
        }
    }
}
