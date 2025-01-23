
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

var sourceData = SourceData.Load("data/masterParts.txt", "data/parts.txt");
var processor = new Processor5(sourceData);
Benchmark.RunFor(processor, sourceData, true);
