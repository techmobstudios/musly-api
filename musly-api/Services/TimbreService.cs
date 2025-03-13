using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using musly_api.Model;
using System.Threading.Tasks;

namespace musly_api.Services
{
    public class TimbreService
    {
        MuslyService muslyService { get; set; }
        public TimbreService(MuslyService _muslyService)
        {
            muslyService = _muslyService;
        }

        public List<Song> timbreSimilarity(String seedFile, int k)
        {
            MuslyTrackId trackId = muslyService.getSeed(seedFile, muslyService.cf.trackFiles);
            int tId = trackId;
            IntPtr seedTrack = muslyService.cf.tracks[tId];
            var muslyTrack = Marshal.PtrToStructure<MuslyTrack>(seedTrack);
            float[] similarities = new float[1];
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                similarities = similarity_raw(seedTrack, muslyService.cf.tracks, k);
                stopwatch.Stop();
                ;
                Console.WriteLine("similarity_raw - Time: " + stopwatch.Elapsed);
                stopwatch.Reset();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Innerxception: " + ex.InnerException);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }

            // Need to normalize data
            Dictionary<int, float> knn_sim = new Dictionary<int, float>();
            for (int i = 0; i < muslyService.trackIds.Length; i++)
            {
                MuslyTrackId currentId = muslyService.trackIds[i];
                if (trackId == currentId)
                {
                    continue;
                }

                knn_sim.Add(i, similarities[i]);

            }

            List<KeyValuePair<int, float>> knnList = knn_sim.ToList();
            knnList.Sort(
                delegate (KeyValuePair<int, float> pair1, KeyValuePair<int, float> pair2)
                {
                    if (double.IsNaN(pair1.Value))
                    {
                        if (double.IsNaN(pair2.Value)) // Throws an argument exception if we don't handle both being NaN
                            return 0;
                        else
                            return 1;
                    }
                    if (double.IsNaN(pair2.Value))
                    {
                        return -1;
                    }

                    return pair1.Value.CompareTo(pair2.Value);
                }
            );

            return muslyService.getSongs(knnList.Take(k).ToList());

        }

        /*
         * 
         *  sample_rate(22050),
            window_size(1024),
            hop(0.5f),
            max_pcmlength(60*sample_rate),
            ps_bins(window_size/2+1),
            mel_bins(36),
            mfcc_bins(25),
         * 
         * 
         */
        float[] similarity_raw(IntPtr seedTrack, List<IntPtr> mTracks, int count)
        {

            int totalArraySize = 675;
            int arraySize = 325;

            float[] similarities = new float[mTracks.Count];

            Gaussian g0 = new Gaussian(arraySize);

            float[] trackFloat = new float[totalArraySize];
            Marshal.Copy(seedTrack, trackFloat, 0, totalArraySize);

            Array.Copy(trackFloat, 0, g0.muPtr, 0, arraySize);
            Array.Copy(trackFloat, 25, g0.covarPtr, 0, arraySize);
            Array.Copy(trackFloat, 350, g0.covarLogdetPtr, 0, arraySize);


            float mu = trackFloat[0];
            float covar = trackFloat[25];
            float logdet = trackFloat[350];


            g0.mu = mu;
            g0.covar = covar;
            g0.covar_logdet = logdet;


            //for (int t = 0; t < mTracks.Count; t++)
            Parallel.For(0, mTracks.Count, t =>
            {
                Gaussian g1 = new Gaussian(arraySize);
                float[] track1Float = new float[totalArraySize];


                IntPtr track1 = mTracks[t];
                Marshal.Copy(track1, track1Float, 0, totalArraySize);


                Array.Copy(track1Float, 0, g1.muPtr, 0, arraySize);
                Array.Copy(track1Float, 25, g1.covarPtr, 0, arraySize);
                Array.Copy(track1Float, 350, g1.covarLogdetPtr, 0, arraySize);


                float t_mu = track1Float[0];
                g1.mu = t_mu;
                float t_covar = track1Float[25];
                g1.covar = t_covar;
                float t_logdet = track1Float[350];
                g1.covar_logdet = t_logdet;

                similarities[t] = jensenShannon(g0, g1);

                track1Float = null;
                g1.Dispose();
            });


            return similarities;
        }

        private float jensenShannon(Gaussian g0, Gaussian g1)
        {
            /*using (Gaussian tmp = new Gaussian(351))
            {*/
            //tmp.muPtr = new float[351];
            //tmp.covarPtr = new float[351];
            //tmp.covarLogdetPtr = new float[351];
            Gaussian tmp = new Gaussian(351);
            int d = 25;

            // return 0 if the models to compare are the same
            if ((g0.covar == g1.covar) && (g0.mu == g1.mu))
            {
                return 0;
            }
            float jsd = -0.25f * (g0.covar_logdet + g1.covar_logdet);

            // merge the mean and covariance matrices to get the merged Gaussian
            for (int i = 0; i < d; i++)
            {
                tmp.muPtr[i] = (float)(0.5 * (g0.muPtr[i] - g1.muPtr[i]));
            }

            int idx_covar = 0;
            for (int i = 0; i < d; i++)
            {
                for (int j = i; j < d; j++)
                {
                    tmp.covarPtr[idx_covar] = 0.5f *
                            (g0.covarPtr[idx_covar] + g1.covarPtr[idx_covar]) +
                            tmp.muPtr[i] * tmp.muPtr[j];
                    idx_covar++;
                }
            }

            // Do an inplace cholesky decompositon and compute logdet of the merged
            // Gaussian.
            int idx_ii = 0;
            for (int i = 0; i < d; i++)
            {
                int idx_k = i;
                for (int k = 0; k < i; k++)
                {
                    tmp.covarPtr[idx_ii] -=
                            tmp.covarPtr[idx_k] * tmp.covarPtr[idx_k];
                    idx_k += d - k - 1;
                }

                if (tmp.covarPtr[idx_ii] <= 0)
                {
                    return -1;
                }
                tmp.covarPtr[idx_ii] = (float)MathF.Sqrt(tmp.covarPtr[idx_ii]);
                jsd += (float)MathF.Log(tmp.covarPtr[idx_ii]);

                int idx_ij = idx_ii;
                for (int j = i + 1; j < d; j++)
                {
                    idx_ij++;

                    int indx_k = 0;
                    for (int k = 0; k < i; k++)
                    {
                        tmp.covarPtr[idx_ij] -=
                                tmp.covarPtr[indx_k + i] * tmp.covarPtr[indx_k + j];
                        indx_k += d - k - 1;
                    }
                    tmp.covarPtr[idx_ij] /= tmp.covarPtr[idx_ii];
                }

                idx_ii += d - i;
            }

            if (Double.IsNaN(jsd) || Double.IsInfinity(jsd))
            {
                return float.MaxValue;
            }

            // We may need to Round to the 7 digits to match the musly c++ lib
            // currently rounding to the nearest 8 digits
            return MathF.Sqrt(MathF.Max(0.0f, jsd));
            //}
        }
    }
}

