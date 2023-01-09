import com.machinezoo.fingerprintio.TemplateFormat;
import com.machinezoo.sourceafis.FingerprintCompatibility;
import com.machinezoo.sourceafis.FingerprintImage;
import com.machinezoo.sourceafis.FingerprintTemplate;
import com.machinezoo.sourceafis.transparency.TransparencyArchive;
import com.machinezoo.sourceafis.transparency.TransparencyBuffer;
import com.machinezoo.sourceafis.transparency.TransparencyKey;
import com.machinezoo.sourceafis.visualization.*;
import com.machinezoo.sourceafis.visualization.keys.ShuffledMinutiaeVisualizer;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

public class Main {

    static FingerprintTemplate decoded;
    public static void main(String[] args) {

        int result = -1;
        if(args.length < 4){
            printUsageInfo();
            System.exit(result);
        }

        switch(args[0]){
            case "-c":
                result = importTemplate(args[2]);

                if(result != 0) break;

                result = exportTemplate(args[3], args[1], decoded);
                break;
            case "-v":
                var buffer = new TransparencyBuffer()
                        .capture(() -> importTemplate(args[2]))
                        .toArchive();
                result = visualize(buffer);
                break;
            default:
                printUsageInfo();
        }
        System.exit(result);
    }
    private static int visualize(TransparencyArchive archive){
        var visualizer = new ShuffledMinutiaeVisualizer();
        var image = visualizer.visualize(archive);
        createSvgFile(image.tree().toString(), "image.svg");
        return 0;
    }
    private static int createSvgFile(String svgText, String filePath)
    {
        try{
            File file = new File(filePath);
            FileWriter fileWriter = new FileWriter(file);
            BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);
            bufferedWriter.write(svgText);
            bufferedWriter.close();
        }catch (IOException e){
            System.out.println("Error 3: Could not create SVG Image" + filePath);
            return 4;
        }
        return 0;
    }
    private static int importTemplate(String in_template){
        try {
            byte[] encoded = Files.readAllBytes(Paths.get(in_template));
            decoded = FingerprintCompatibility.importTemplate(encoded);

        } catch (IllegalArgumentException e) {
            System.out.println("Error 2: Unknown Template format" + e.getClass());
            return 2;
        } catch (Exception e) {
            System.out.println("Error 1: Could not load Template" + e.getClass());
            return 1;
        }
        return 0;
    }
    private static int exportTemplate(String outTemplate, String formatString, FingerprintTemplate template){
        Path outTemplatePath = Path.of(outTemplate);

        if(formatString.equals("SourceAFIS")) {
            try{
                Files.write(outTemplatePath, template.toByteArray());
            } catch (Exception e){
                System.out.println("Error 3: Could not save SourceAfis Template" + e.getClass());
                return 3;
            }
            return 0;
        }

        TemplateFormat format = parseTemplateFormat(formatString);
        if(format == null){
            System.out.println("Error 4: Could not parse TemplateFormat");
            return 4;
        }

        byte[] encoded = FingerprintCompatibility.exportTemplates(format, template);
        try{
            Files.write(outTemplatePath, encoded);
        } catch (Exception e){
            System.out.println("Error 3: Could not save " + format + " Template" + e.getClass());
            return 3;
        }
        return 0;
    }

    private static TemplateFormat parseTemplateFormat(String formatString)
    {
        for (TemplateFormat format : TemplateFormat.values()) {
            if (format.name().equals(formatString)) {
                return format;
            }
        }
        return null;
    }
    static void printUsageInfo()
    {
        String name = getJarName();

        System.out.println(
                        "Template Conversion\n" +
                        "Usage: java -jar " + name + " -c <output_format> <in_tmp.*> <out_tmp.*>\n" +
                        "where <output_format> is the format of the output template\n" +
                        "<in_tmp.*> is the fingerprint template to convert\n" +
                        "<out_tmp.*> is the fingerprint template to convert to\n" +
                        "supported formats:\n" +
                                "SourceAFIS\n" +
                                "ANSI_378_2004\n" +
                                "ANSI_378_2009\n" +
                                "ANSI_378_2009_AM1\n" +
                                "ISO_19794_2_2005\n" +
                                "ISO_19794_2_2011");
    }

    static String getJarName(){
        try {

            // Get path of the JAR file
            String jarPath = Main.class
                    .getProtectionDomain()
                    .getCodeSource()
                    .getLocation()
                    .toURI()
                    .getPath();

            // Get name of the JAR file
           return jarPath.substring(jarPath.lastIndexOf("/") + 1);

        } catch (Exception e) {
            e.printStackTrace();
        }
        return "unknown.jar";
    }
}
