using SourceAFIS;
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

           

            if(args.Length < 4)
            {
                printUsageInfo(args);
                return -1;
            }

            int res = 0;
            switch (args[1])
            {
                case "-e":
                    res = extraction(args[2], args[3]);
                    break;

                case "-m":
                    if(args.Length < 5)
                    {
                        goto default;
                    }
                    res = matching(args[2], args[3], args[4], out double score);
                    break;

                default:
                    printUsageInfo(args);
                    return -1;
            };

            return res;
        }

        static int extraction(string imagePath, string templatePath)
        {
            FingerprintTemplate probe;

            try
            {
                probe = new FingerprintTemplate(
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
                File.WriteAllBytes(templatePath, probe.ToByteArray());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 2: Could not write Template! ({e})");
                return 2;
            }

            return 0;
        }

        static int matching(string imagePath, string templatePath, string outputPath, out double score)
        {
            FingerprintTemplate candidate, probe;
            score = 0;

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
                probe = new FingerprintTemplate(File.ReadAllBytes(templatePath));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error 3: Could not load Template! ({e.GetType()})");
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
            string path;
            if (args.Length > 0)
            {
                path = args[0];
            }
            else
            {
                path = System.Reflection.Assembly.GetEntryAssembly().Location;
            }
            Console.WriteLine(
                "Fingerprint Extraction\n" +
                $"Usage: {path} -e <in_image.png> <out_tmp.cbor>\n" +
                "where <in_image.png> is the grayscale figerprint image\n" +
                "<out_tmp.cbor> is the serialized template file (RFC 8949 Concise Binary Object Representation)\n\n" +

                "Fingerprint Matching\n" +
                $"Usage: {path} -m <in_image.png> <in_tmp.cbor> <out_score.json>\n" +
                "where <in_image.png> is the grayscale figerprint image\n" +
                "<in_tmp.cbor> is the serialized template file (RFC 8949 Concise Binary Object Representation)\n" +
                "<out_score.json> is the json serialized score file");
        }
    }
}