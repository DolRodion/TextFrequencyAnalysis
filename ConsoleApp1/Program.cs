using TextFrequencyAnalysis;

string path = @"C:\text.txt";
//string path = Console.ReadLine();

StringProcessingFactory factoryBuilder = new StringProcessingFactory(path);
factoryBuilder.InitAsync();