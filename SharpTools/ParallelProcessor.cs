using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTools
{
    public class ParallelProcessor<TInput, TOutput>
    {
        struct Chunk
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }
        private readonly int _threads = Environment.ProcessorCount * 2;
        
        private readonly TInput[] _input;
        private readonly TOutput[] _output;
        private readonly Func<TInput, TOutput> _processor;
 
        public ParallelProcessor(int threadsNumber, IEnumerable<TInput> input, Func<TInput, TOutput> processor):this(input, processor)
        {
            _threads = threadsNumber;            
           
        }
 
        public ParallelProcessor(IEnumerable<TInput> input, Func<TInput, TOutput> processor)
        {
            _input = input.ToArray();
            _output = new TOutput[_input.Length];
            _processor = processor;
        }

 
        private void Spawn(int min, int max)
        {
            for (var i = max - 1; i >= min; i--)
            {
                _output[i] = _processor(_input[i]);
            }
        }
 
        private IEnumerable<Chunk> GetRanges(int collectionSize)
        {
            var chunkSize = collectionSize / _threads;
            if (collectionSize < _threads || chunkSize < 2)
            {
                yield return new Chunk {Min = 0, Max = collectionSize};
            }
            var pos = 0;
            for (var i = 0; i < _threads - 1; i++)
            {
                var next = pos + chunkSize;
                yield return new Chunk{Min= pos, Max = next };
                pos = next;
            }
            yield return new Chunk{Min = pos, Max = collectionSize};
        }
 
        public async Task<IEnumerable<TOutput>> Process()
        {
            var arr = _input.ToArray();
            await Task.WhenAll(GetRanges(arr.Length)
                               .Select(r => Task.Run(() => Spawn(r.Min, r.Max))));
            return _output;
        }
    }

}