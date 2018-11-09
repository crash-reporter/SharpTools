using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpTools;
using Xunit;

namespace SharpToolsTests
{
    public class ParallelProcessorTests
    {
        private string IndexOfSomeString(string input)
        {
            if (input.IndexOf("This is the pattern I want to find!", StringComparison.Ordinal) > -1)
            {
                return input;
            }

            return null;
        }

        [Fact]
        public void LoopProcessing()
        {
            var count = 500_000;
            var shouldFind = count / 2;
            var strings = new string[count];
            for (var i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    strings[i] = "Some text here and This is the pattern I want to find! and some text here";
                    continue;
                }

                strings[i] = "Some text here but not the pattern I am looking for!";
            }

            var loopStopwatch = new Stopwatch();
            loopStopwatch.Start();
            var loopResult = new List<string>();

            for (int i = 0; i < strings.Length; i++)
            {
                loopResult.Add(IndexOfSomeString(strings[i]));
            }

            var loopFoundEntries = loopResult.Count(s => !string.IsNullOrWhiteSpace(s));
            loopStopwatch.Stop();
            Debug.WriteLine($"Loop time ms: {loopStopwatch.ElapsedMilliseconds}");


            Assert.Equal(shouldFind, loopFoundEntries);
        }

        [Fact]
        public async Task ParallelProcessing()
        {
            var count = 500_000;
            var shouldFind = count / 2;
            var strings = new string[count];
            for (var i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    strings[i] = "Some text here and This is the pattern I want to find! and some text here";
                    continue;
                }

                strings[i] = "Some text here but not the pattern I am looking for!";
            }


            var parallelStopwatch = new Stopwatch();
            parallelStopwatch.Start();

            var processor = new ParallelProcessor<string, string>(strings, IndexOfSomeString);
            var parallelResult = await processor.Process();
            var parallelFoundEntries = parallelResult.Count(s => !string.IsNullOrWhiteSpace(s));

            parallelStopwatch.Stop();
            Debug.WriteLine($"Parallel time ms: {parallelStopwatch.ElapsedMilliseconds}");

            Assert.Equal(shouldFind, parallelFoundEntries);
        }
        
    }
}