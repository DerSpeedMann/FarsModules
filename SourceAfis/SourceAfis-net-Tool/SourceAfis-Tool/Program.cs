using SourceAFIS;
using System.Diagnostics;
using System.Text.Json;

namespace MyNamespace
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            #if DEBUG
            args = new string[] {
                System.Reflection.Assembly.GetEntryAssembly().Location,
                "-m",
                "matching.png",
                "template.cbor",
                "score.json"
            };
            #endif

           

            if(args.Length < 3)
            {
                printUsageInfo(args);
                return -1;
            }

            int res = 0;
            switch (args[0])
            {
                case "-e":
                    res = extraction(args[1], args[2]);
                    break;

                case "-m":
                    if(args.Length < 4)
                    {
                        goto default;
                    }
                    res = matching(args[1], args[2], args[3], out double score);
                    break;

                default:
                    printUsageInfo(args);
                    return -1;
            };

            return res;
        }

        static int extraction(string imagePath, string templatePath)
        {
            FingerprintTemplate candidate;

            try
            {
                candidate = new FingerprintTemplate(
                    new FingerprintImage(File.ReadAllBytes(imagePath))
                );

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 1: Could not load Image! ({e.GetType()})");
                return 1;
            }
            try
            {
                File.WriteAllBytes(templatePath, candidate.ToByteArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 2: Could not write Template! ({e})");
                return 2;
            }

            return 0;
        }

        static int matching(string probeTemplatePath, string candidateTemplatePath, string outputPath, out double score)
        {
            FingerprintTemplate candidate, probe;
            score = 0;

            try
            {
                probe = new FingerprintTemplate(File.ReadAllBytes(probeTemplatePath));

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 1: Could not load Template to match! ({e.GetType()})");
                return 1;
            }
            try
            {
                candidate = new FingerprintTemplate(File.ReadAllBytes(candidateTemplatePath));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 3: Could not load Template to match against! ({e.GetType()})");
                return 3;
            }

            score = new FingerprintMatcher(probe).Match(candidate);
            
            
            Console.WriteLine($"score: {score}");
            try
            {
                string jsonString = JsonSerializer.Serialize(score);
                File.WriteAllText(outputPath, jsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 4: Could not serialize or save result! ({e.GetType()})");
                return 4;
            }
           

            
            return 0;
        }

        static void printUsageInfo(string[] args)
        {
            string name = Process.GetCurrentProcess().ProcessName+".exe";

            Console.WriteLine(
                "Fingerprint Extraction\n" +
                $"Usage: {name} -e <in_image.png> <out_tmp.cbor>\n" +
                "where <in_image.png> is the grayscale fingerprint image\n" +
                "<out_tmp.cbor> is the serialized template file (RFC 8949 Concise Binary Object Representation)\n\n" +

                "Fingerprint Matching\n" +
                $"Usage: {name} -m <in_probeTmp.cbor> <in_candidateTmp.cbor> <out_score.json>\n" +
                "where <in_probeTmp.cbor> is the serialized fingerprint template to check (RFC 8949 Concise Binary Object Representation)\n" +
                "<in_candidateTmp.cbor> is the serialized fingerprint template file to check against\n" +
                "<out_score.json> is the json serialized score file");
        }
    }
}