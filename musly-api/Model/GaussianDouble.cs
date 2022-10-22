using System;
using System.Buffers;
using System.Drawing;

namespace musly_api.Model
{
    public class GaussianDouble : IDisposable
    {
        private static ArrayPool<double> _arrayPool = ArrayPool<double>.Create();

        public double mu { get; set; }
        public double covar { get; set; }
        public double covar_logdet { get; set; }

        public double[] muPtr { get; set; }
        public double[] covarPtr { get; set; }
        public double[] covarLogdetPtr { get; set; }

        public GaussianDouble(int size)
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
        }
    }
}

