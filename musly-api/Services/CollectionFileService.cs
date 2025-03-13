using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using CMTZ.Entities;
using Microsoft.Extensions.Configuration;
using musly_api.Model;

namespace musly_api.Services
{
    public class CollectionFileService
    {
        private readonly IConfiguration _config;

        public CollectionFileService(IConfiguration config)
        {
            _config = config;
            //NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ImportResolver);
        }

        const string MUSLY_LIB = "/usr/local/bin/musly";

        const string MUSLY_LIB_WIN = "C:\\Program Files (x86)\\musly\\bin\\libmusly.dll";


        [DllImport(MUSLY_LIB)]
        public static extern IntPtr musly_jukebox_poweron([MarshalAs(UnmanagedType.LPStr)] string method, [MarshalAs(UnmanagedType.LPStr)] string decoder);


        [DllImport(MUSLY_LIB)]
        public static extern int musly_track_binsize(IntPtr jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern IntPtr musly_track_alloc(IntPtr jukebox);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_track_frombin(IntPtr jukebox, byte[] from_buffer, IntPtr to_track);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_track_analyze_audiofile(IntPtr jukebox, [MarshalAs(UnmanagedType.LPStr)] string audioData, float length, float start, IntPtr features);

        [DllImport(MUSLY_LIB)]
        public static extern int musly_track_size(IntPtr jukebox);


        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == MUSLY_LIB)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.WriteLine("Loading Windows Musly Library");
                    NativeLibrary.TryLoad(MUSLY_LIB_WIN, assembly, DllImportSearchPath.System32, out libHandle);
                }
            }
            return libHandle;
        }

        string VERSION = "0";
        string HEADER = "MUSLY";

        public String method { get; set; }

        public MuslyJukeBox jukebox { get; set; }

        public List<String> trackFiles { get; set; }

        public List<IntPtr> tracks { get; set; }

        private IntPtr jukeboxPtr { get; set; }



        public void InitializeJukebox()
        {

             this.jukeboxPtr = musly_jukebox_poweron(null, null);

        }

        public int readCollectionFile(String cfile, char mode, string[] tracks, string[] tracksFiles)
        {

            if (!File.Exists(cfile))
            {
                throw new Exception("Collection File Doesn't Exist");
            }

            if (!readHeader(cfile))
            {
                throw new Exception("Collection File Invalid");
            }

            var method = this.method;

            var jukebox = musly_jukebox_poweron(method, null);

            this.jukebox = Marshal.PtrToStructure<MuslyJukeBox>(jukebox);

            int buffersize = musly_track_binsize(jukebox);

             var mt = musly_track_alloc(jukebox);
             var muslyTrack = Marshal.PtrToStructure<MuslyTrack>(jukebox);

            int trackCount = readTracks(cfile, buffersize, jukebox);
            Console.WriteLine("Loaded " + trackCount + " Tracks");

            return 0;
        }

        private bool readHeader(String cfile)
        {
            using (var stream = File.Open(cfile, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    byte[] header = reader.ReadBytes(255);
                    string headerString = Encoding.Default.GetString(header);
                    headerString = headerString.Split("\0")[0];

                    string[] headerSplit = headerString.Split("-");

                    if (headerSplit.Length != 3)
                    {
                        return false;
                    }

                    if ((headerSplit[0] != HEADER) || (headerSplit[1] != VERSION))
                    {
                        return false;

                    }

                    this.method = headerSplit[2];

                    return true;
                }
            }

        }

        private int readTracks(String cfile, int bufferSize, IntPtr jukebox)
        {
            int count = 0;
            List<String> track = new List<string>();
            List<IntPtr> muslyTracks = new List<IntPtr>();


            using (var stream = File.Open(cfile, FileMode.Open))
            {
                
                SeekOrigin seekOrigin = new SeekOrigin();
                stream.Seek(15, seekOrigin);
                int bytesRead = 0;

                while (bytesRead >= 0)
                {
                    long pos = stream.Position;
                    byte[] buffer = new byte[bufferSize];
                    byte[] filebuffer = new byte[4096];

                    int fileBytes = stream.Read(filebuffer, 0, filebuffer.Length);

                    if(fileBytes == 0)
                    {
                        this.trackFiles = track;
                        this.tracks = muslyTracks;
                        //musly_track_free(mt);

                        return count;
                    }

                    string file = readAscii(filebuffer);
                    stream.Position = pos + file.Length + 1;
                    int dataFieldSize;

                    using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                    {
                        uint udataFieldSize = reader.ReadUInt32();
                        dataFieldSize = unchecked((int)udataFieldSize);

                    }

                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    var currentMt = musly_track_alloc(jukebox);
                    int byteRead = musly_track_frombin(jukebox, buffer, currentMt);
                    var muslyTrack = Marshal.PtrToStructure<MuslyTrack>(currentMt);

                    if (byteRead > 0)
                    {
                        track.Add(file);
                        muslyTracks.Add(currentMt);
                    }

                    count++;
                }


            }
            return count;

        }


        string readAscii(byte[] bufferBytes)
        {
            List<byte> strBytes = new List<byte>();
            byte b;
            int i = 0;
            while ((b = bufferBytes[i]) != 0x00)
            {
                strBytes.Add((byte)b);
                i++;
            }
            return Encoding.ASCII.GetString(strBytes.ToArray());

        }

        /*public async Task<TrackInfo[]> ProcessTrackInfo(List<Track> tracks)
        {
            InitializeJukebox();
            List<Task<TrackInfo>> trackinfoList = new List<Task<TrackInfo>>();

            //for (int i = 0; i < tracks.Count; i++)
            foreach(var track in tracks)
            {
                //var track = tracks[i];


                trackinfoList.Add(PopulateTrack(track));
            }

            return await Task.WhenAll<TrackInfo>(trackinfoList);
        }*/

        /*public async Task<TrackInfo> PopulateTrack(Track track)
        {
            //float[] audioFeatures = ExtractFeatures(track.TrackURL);
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.Url = track.TrackURL;
            //trackInfo.Id = track.Id.ToString();
            trackInfo.Title = track.TrackTitle;
            trackInfo.Artist = track.Album.Artists;
            trackInfo.Description = track.Album.Description;
            trackInfo.AlbumTitle = track.Album.Title;

            //trackInfo.AudioFeatures = audioFeatures;
            return trackInfo;
        }*/

        public float[] ExtractFeatures(string filePath)
        {
            IntPtr trackPtr = musly_track_alloc(this.jukeboxPtr);
            int bytes = musly_track_binsize(this.jukeboxPtr);
            int trackSize = musly_track_size(this.jukeboxPtr) / sizeof(float);
            float[] track = new float[trackSize];
            Console.WriteLine("$\"Extracting features from " + filePath);



            try
            {

                int result = musly_track_analyze_audiofile(
                        this.jukeboxPtr,
                        filePath,
                        30f,
                        0f,
                        trackPtr
                    );

                if (result != 0)
                {
                    Console.WriteLine("$\"Failed to extract features from " + filePath);
                    return track;
                }

                Marshal.Copy(trackPtr, track, 0, trackSize);


                return track;
            }
            finally
            {
                // Always free unmanaged memory
                Marshal.FreeHGlobal(trackPtr);
            }
        }
    }

    public struct MuslyJukeBox
    {
        public IntPtr method;

        /** Method name as null terminated string
         */
                [MarshalAs(UnmanagedType.LPStr)]
        public string methodName;

        /** A reference to the initialized audio file decoder. Hides a C++
         * musly::decoder object.
         */
        public IntPtr decoder;

        /** Decoder name as null terminated string
         */
        [MarshalAs(UnmanagedType.LPStr)]
        public string decoderName;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MuslyTrack
    {
        /*public MuslyTrack(float[] value)
        {
            this.realValue = value;
        }*/

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 351)]
        public float[] realValue;
        /*public static implicit operator float[](MuslyTrack value)
        {
            return value.realValue;
        }

        public static implicit operator MuslyTrack(float[] value)
        {
            return new MuslyTrack(value);
        }

        public float[] getFloat()
        {
            return realValue;
        }*/
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MuslyTrackId
    {
        public MuslyTrackId(int value)
        {
            this.realValue = value;
        }

        private int realValue;
        public static implicit operator int(MuslyTrackId value)
        {
            return value.realValue;
        }

        public static implicit operator MuslyTrackId(int value)
        {
            return new MuslyTrackId(value);
        }
    }


    }
