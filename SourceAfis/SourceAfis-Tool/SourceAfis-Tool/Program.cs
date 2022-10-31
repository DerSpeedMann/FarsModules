using SourceAFIS;

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
                "-e",
                @"matching.png",
                "template.cbor"
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
                    res = matching(args[2], args[3], out double score);
                    break;

                default:
                    printUsageInfo(args);
                    return -1;
            };

           

            if(res != 0)
            {
                return res;
            }


            //TODO: save score

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

        static int matching(string imagePath, string templatePath, out double score)
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
            return 0;
        }

        static void printUsageInfo(string[] args)
        {
            Console.WriteLine(
                "Fingerprint Extraction\n" +
                $"Usage: {args[0]} -e <in_image.png> <out_tmp.cbor>\n" +
                "where <in_image.png> is the grayscale figerprint image\n" +
                "<out_tmp.cbor> is the serialized template file (RFC 8949 Concise Binary Object Representation)\n\n" +

                "Fingerprint Matching\n" +
                $"Usage: {args[0]} -m <in_image.png> <in_tmp.cbor>\n" +
                "where <image.png> is the grayscale figerprint image\n" +
                "<tmp.cbor> is the serialized template file (RFC 8949 Concise Binary Object Representation)\n");
        }
    }
}