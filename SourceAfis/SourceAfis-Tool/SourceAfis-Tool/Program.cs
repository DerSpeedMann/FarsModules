using SourceAFIS;

Console.WriteLine(Directory.GetCurrentDirectory());

var options = new FingerprintImageOptions { Dpi = 500 };
FingerprintTemplate candidate, probe;

var imagePath = @"G:\Users\Michaelwa\Desktop\privat\FH\SIM\PRO_02\SourceAfis\SourceAfis-Tool\SourceAfis-Tool\bin\Debug\net6.0\matching.png";
try
{
    probe = new FingerprintTemplate(
        new FingerprintImage(File.ReadAllBytes(imagePath))
    );
}
catch (Exception e)
{
    Console.WriteLine($"Could not load Image! ({e.GetType()})");
    return 1;
}
try
{
    candidate = new FingerprintTemplate(
        new FingerprintImage(320, 407, File.ReadAllBytes("candidate.dat"), options)
    );
}
catch (Exception e)
{
    Console.WriteLine($"Could not load Template! ({e.GetType()})");
    return 2;
}
double score = new FingerprintMatcher(probe).Match(candidate);
Console.WriteLine("score");

return 0;
