using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using musly_api.Model;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;

namespace musly_api.Services
{
    public class MuslyService
    {
        public IntPtr jukebox { get; set; }

        public int[] trackIds { get; set; }

        public CollectionFileService cf { get; set; }

        private readonly IConfiguration _config;

        public string library { get; set; }

        public MuslyService(CollectionFileService cf, IConfiguration config, IHostingEnvironment _environment)
        {
            _config = config;
            string path = "";
            string jukeBoxPath = "";
            bool enableLocal = _config.GetValue<bool>("musly:enableLocalCollection");
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ImportResolver);



            if (enableLocal){
              path = Path.Combine(_environment.WebRootPath, "musly") + Path.DirectorySeparatorChar + "collection.musly";
              jukeBoxPath = Path.Combine(_environment.WebRootPath, "musly") + Path.DirectorySeparatorChar + "jukebox";
            }
            else{
             path = _config.GetValue<string>("musly:collection");
             jukeBoxPath = _config.GetValue<string>("musly:jukebox");
            }

            char[] t = "t".ToCharArray();
            cf.readCollectionFile(path, t[0], null, null);

            // read_jukebox(jukebox_file, &mj2, &last_reinit) - Ln 762
            this.jukebox = readJukeBox(jukeBoxPath);
            var marshalledJukeBox = Marshal.PtrToStructure<MuslyJukeBox>(jukebox);
            int trackCount = musly_jukebox_trackcount(jukebox);
            Console.WriteLine("Jukebox Track Count " + trackCount);

            //tracks_initialize - ln 813
            this.cf = cf;
            List<string> trackFiles = cf.trackFiles;
            List<IntPtr> tracks = cf.tracks;

            int[] trackIds = new int[tracks.Count];
            IntPtr[] track_ids = new IntPtr[tracks.Count];

            int size = Marshal.SizeOf(typeof(MuslyTrackId));

            for (int i = 0; i < trackIds.Length; i++)
            {
                trackIds[i] = i;
            }

            //this.track_ids = track_ids;
            this.trackIds = trackIds;

        }


        /*
         *        
         *      musly_track* track - seed Track
                musly_trackid seed_trackid - seed TrackId
                musly_track** tracks,
                musly_trackid* trackids,

         * 
         */

        public List<Song> timbreSimilarity(String seedFile, int k)
        {
            MuslyTrackId trackId = getSeed(seedFile, this.cf.trackFiles);
            int tId = trackId;
            IntPtr seedTrack = cf.tracks[tId];
            var muslyTrack = Marshal.PtrToStructure<MuslyTrack>(seedTrack);
            float[] similarities = new float[1];
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                similarities = similarity_raw(seedTrack, cf.tracks, k);
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
            for (int i = 0; i < this.trackIds.Length; i++)
            {
                MuslyTrackId currentId = this.trackIds[i];
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

            return getSongs(knnList.Take(k).ToList());

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

        public List<Song> getTrackRecommends(String seedFile, int k)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            MuslyTrackId trackId = getSeed(seedFile, this.cf.trackFiles);

            //int[] seedInt = new int[2];
            //Marshal.Copy(seed, seedInt, 0, 2);


            stopwatch.Stop();
            //Console.WriteLine("Part 1 {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            //k = Math.Min(k, cf.tracks.Count);

            //int guessLen = Math.Max(k, (int)(cf.tracks.Count * 0.1));
            //IntPtr[] guess_ids = new IntPtr[guessLen + 1];

            //MuslyTrackId[] guessIds = new MuslyTrackId[guessLen];
            /*for (int i = 0; i < guessIds.Length; i++)
            {
                guess_ids[i] = Marshal.AllocHGlobal(size + sizeof(int));
                Marshal.StructureToPtr(guessIds[i], guess_ids[i], false);
            }

            int guessLength = musly_jukebox_guessneighbors(this.jukebox, seed, guess_ids, guessLen);*/
            // stopwatch.Start();
            float[] similaritiesArr = new float[cf.tracks.Count];
            Dictionary<int, float> knn_sim = new Dictionary<int, float>();

            int guessLength = -1;

            if (guessLength <= 0)
            {
                int tId = trackId;
                IntPtr seedTrack = cf.tracks[tId];
                IntPtr[] allTracks = cf.tracks.ToArray();
                int count = cf.tracks.Count;
                stopwatch.Start();

                int ret = musly_jukebox_similarity(this.jukebox, seedTrack, tId, allTracks, this.trackIds, count, similaritiesArr);


            }
            stopwatch.Stop();
            //Console.WriteLine("Part 2 - Simialrity {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            stopwatch.Start();

            for (int i = 0; i < this.trackIds.Length; i++)
            {
                MuslyTrackId currentId = this.trackIds[i];
                if (trackId == currentId)
                {
                    continue;
                }

                knn_sim.Add(i, similaritiesArr[i]);

            }

            stopwatch.Stop();
            //Console.WriteLine("Part 3 - Dictionary Add {0}", stopwatch.Elapsed);
            stopwatch.Reset();


            stopwatch.Start();

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

                    //return pair1.Value < pair2.Value ? 1 : 0;
                    return pair1.Value.CompareTo(pair2.Value);
                }
            );
            stopwatch.Stop();
            //Console.WriteLine("Part 4 - List Sort {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            return getSongs(knnList.Take(k).ToList());

        }




        const string MUSLY_LIB = "/usr/local/bin/musly";

        const string MUSLY_LIB_WIN = "C:\\Program Files (x86)\\musly\\bin\\libmusly.dll";


        [DllImport(MUSLY_LIB)]
        public static extern IntPtr musly_jukebox_fromfile([MarshalAs(UnmanagedType.LPStr)] string jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_trackcount(IntPtr jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_guessneighbors(IntPtr jukebox, IntPtr seed, IntPtr[] neighbors, int numofNeighbors);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_similarity(IntPtr jukebox, IntPtr seedTrack, int seedTrackId, IntPtr[] tracks,
            int[] trackIds, int numOfTracks, float[] similarites);


        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == MUSLY_LIB)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Console.WriteLine("Loading Musly Library for Windows");
                    NativeLibrary.TryLoad(MUSLY_LIB_WIN, assembly, DllImportSearchPath.System32, out libHandle);
                }
            }
            return libHandle;
        }

        private List<Song> getSongs(List<KeyValuePair<int, float>> knnList)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Song> songs = new List<Song>();
            for (int j = 0; j < knnList.Count; j++)
            {
                Song song = new Song();
                int id = knnList[j].Key;
                float floatVal = knnList[j].Value;
                song.similarity = floatVal;
                song.key = cf.trackFiles[id];
                song.audio = song.key.Replace("/home/music/", "https://certifiedmixtapez.com/UploadedFiles/");
                if (song.audio.Contains(" "))
                {
                    song.audio = song.audio.Replace(" ", "%20");
                }

                songs.Add(song);
            }

            stopwatch.Stop();
            //Console.WriteLine("Part 5 - GetSongs {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            return songs;
        }

        private IntPtr readJukeBox(String jukeBoxFile)
        {
            return musly_jukebox_fromfile(jukeBoxFile);
        }

        private MuslyTrackId getSeed(String seedPath, List<string> trackFiles)
        {
            int seedId = 0;
            for (int i = 0; i < trackFiles.Count; i++)
            {
                if (trackFiles[i].Equals(seedPath))
                {
                    seedId = i;
                    break;
                }
            }

            return new MuslyTrackId(seedId);
        }

        private int getSeedInt(String seedPath, List<string> trackFiles)
        {
            int seedId = 0;
            for (int i = 0; i < trackFiles.Count; i++)
            {
                if (trackFiles[i].Equals(seedPath))
                {
                    seedId = i;
                    break;
                }
            }

            return seedId;
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

    public class Song
    {
        public Song()
        {

        }

        public string key { get; set; }

        public string audio { get; set; }

        public float similarity { get; set; }

    }
}
