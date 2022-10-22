using System;
using System.Buffers;

namespace musly_api.Model
{

    public class Gaussian : IDisposable
    {
        private static ArrayPool<float> _arrayPool = ArrayPool<float>.Create();

        public float mu { get; set; }
        public float covar { get; set; }
        public float covar_logdet { get; set; }

        public  float[] muPtr { get; set; }
        public float[] covarPtr { get; set; }
        public float[] covarLogdetPtr { get; set; }


        public Gaussian(int size)
        {
            muPtr = _arrayPool.Rent(size);
            covarPtr = _arrayPool.Rent(size);
            covarLogdetPtr = _arrayPool.Rent(size);
        }

        public void Dispose()
        {
            _arrayPool.Return(muPtr);
            _arrayPool.Return(covarPtr);
            _arrayPool.Return(covarLogdetPtr);
            //Console.WriteLine("Disposed");
        }

    }
}

