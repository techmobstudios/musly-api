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


            if (enableLocal)
            {
                path = Path.Combine(_environment.WebRootPath, "musly") + Path.DirectorySeparatorChar + "collection.musly";
                jukeBoxPath = Path.Combine(_environment.WebRootPath, "musly") + Path.DirectorySeparatorChar + "jukebox";
            }
            else
            {
                path = _config.GetValue<string>("musly:collection");
                jukeBoxPath = _config.GetValue<string>("musly:jukebox");
            }

            Console.WriteLine("Using Collection " + path);
            Console.WriteLine("Using Jukebox " + jukeBoxPath);

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

            int size = Marshal.SizeOf(typeof(MuslyTrackId));

            for (int i = 0; i < trackIds.Length; i++)
            {
                trackIds[i] = i;
            }

            this.trackIds = trackIds;
        }


        public List<Song> getTrackRecommends(String seedFile, int k)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            MuslyTrackId trackId = getSeed(seedFile, this.cf.trackFiles);
            stopwatch.Stop();
            Console.WriteLine("Part 1 {0}", stopwatch.Elapsed);
            stopwatch.Reset();


            stopwatch.Start();
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
            Console.WriteLine("Part 2 - Simialrity {0}", stopwatch.Elapsed);
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
            Console.WriteLine("Part 3 - Dictionary Add {0}", stopwatch.Elapsed);
            stopwatch.Reset();



            stopwatch.Start();
            List<KeyValuePair<int, float>> knnList = knn_sim.ToList();
            knnList.Sort(
                delegate (KeyValuePair<int, float> pair1, KeyValuePair<int, float> pair2)
                {
                    if(pair1.Value == 0 )//|| pair1.Value < .00005)
                    {
                        return 1;
                    }

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
            stopwatch.Stop();
            Console.WriteLine("Part 4 - List Sort {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            return getSongs(knnList.Take(k).ToList());

        }


        // Mac
        //const string MUSLY_LIB = "/usr/local/bin/musly";

        // Windows
        const string MUSLY_LIB = "C:\\Program Files (x86)\\musly\\bin\\libmusly.dll";


        [DllImport(MUSLY_LIB)]
        public static extern IntPtr musly_jukebox_fromfile([MarshalAs(UnmanagedType.LPStr)] string jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_trackcount(IntPtr jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_guessneighbors(IntPtr jukebox, IntPtr seed, IntPtr[] neighbors, int numofNeighbors);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_jukebox_similarity(IntPtr jukebox, IntPtr seedTrack, int seedTrackId, IntPtr[] tracks,
            int[] trackIds, int numOfTracks, float[] similarites);


        public List<Song> getSongs(List<KeyValuePair<int, float>> knnList)
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
            Console.WriteLine("Part 5 - GetSongs {0}", stopwatch.Elapsed);
            stopwatch.Reset();

            return songs;
        }

        private IntPtr readJukeBox(String jukeBoxFile)
        {
            return musly_jukebox_fromfile(jukeBoxFile);
        }

        public MuslyTrackId getSeed(String seedPath, List<string> trackFiles)
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
