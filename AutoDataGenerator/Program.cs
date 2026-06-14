
namespace AutoDataGenerator;

class Program
{
    static void Main(string[] args)
    {

        // The input dir is the current directory
        string inputDir = Directory.GetCurrentDirectory();
        string outputDir;
        if (args.Length < 1)
        {
            Console.WriteLine("Enter output directory:");
            outputDir = Console.ReadLine();
        }
        else
        {
            outputDir = args[0];
        }

        if (inputDir == null || !Directory.Exists(inputDir))
        {
            Console.WriteLine("Invalid input directory.");
            return;
        }

        if (outputDir == null || !Directory.Exists(outputDir))
        {
            Console.WriteLine("Invalid output directory.");
            return;
        }

        // Ask user which data to generate
        int option = -1;
        while (option < 0 || option > 2)
        {
            Console.WriteLine("Which data to generate? (0 = FF12, 1 = LR, 2=FF132, 3 = All)");
            string input = Console.ReadLine();
            if (!int.TryParse(input, out option) || option < 0 || option > 3)
            {
                Console.WriteLine("Invalid option. Please enter 0, 1, 2 or 3.");
            }
        }

        if (option == 0 || option == 3)
        {
            GenerateFF12Data(inputDir, Path.Combine(outputDir, "ff12_open_world"));
        }
        if (option == 1 || option == 3)
        {
            GenerateLRData(inputDir, Path.Combine(outputDir, "lrff13"));
        }
        if (option == 2 || option == 3)
        {
            GenerateFF132Data(inputDir, Path.Combine(outputDir, "ff132"));
        }
    }

    private static void GenerateFF12Data(string inputDir, string outputDir)
    {
        inputDir = inputDir.Replace("AutoDataGenerator", "FF12Rando");
        if (!Directory.Exists(outputDir))
        {
            Console.WriteLine("No output directory for FF12.");
            return;
        }

        Console.WriteLine("Generating FF12 data...");
        FF12MultiworldGenerator generator = new(inputDir, outputDir);
        generator.Generate();
    }

    private static void GenerateLRData(string inputDir, string outputDir)
    {
        inputDir = inputDir.Replace("AutoDataGenerator", "LRRando");
        if (!Directory.Exists(outputDir))
        {
            Console.WriteLine("No output directory for LR.");
            return;
        }

        Console.WriteLine("Generating LR data...");
        LRMultiworldGenerator generator = new(inputDir, outputDir);
        generator.Generate();
    }

    private static void GenerateFF132Data(string inputDir, string outputDir)
    {
        inputDir = inputDir.Replace("AutoDataGenerator", "FF13_2Rando");
        if (!Directory.Exists(outputDir))
        {
            Console.WriteLine("No output directory for FF13-2.");
            return;
        }

        Console.WriteLine("Generating FF13-2 data...");
        FF13_2MultiworldGenerator generator = new(inputDir, outputDir);
        generator.Generate();
    }
}
