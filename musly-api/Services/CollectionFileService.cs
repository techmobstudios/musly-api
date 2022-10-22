using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace musly_api.Services
{
    public class CollectionFileService
    {
        private readonly IConfiguration _config;

        public CollectionFileService(IConfiguration config)
        {
            _config = config;
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ImportResolver);
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

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == MUSLY_LIB)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            //List<MuslyTrack> mTracks = new List<MuslyTrack>();


            using (var stream = File.Open(cfile, FileMode.Open))
            {
                /*using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    //byte[] buffer = reader.ReadBytes(bufferSize);
                    //string headerString = Encoding.Default.GetString(buffer);
                    //readAscii(reader);

                }*/
                
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
                        //this.muslyTracksList = mTracks;
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
                    //Console.WriteLine(file);

                    var currentMt = musly_track_alloc(jukebox);
                    int byteRead = musly_track_frombin(jukebox, buffer, currentMt);
                    var muslyTrack = Marshal.PtrToStructure<MuslyTrack>(currentMt);

                    if (byteRead > 0)
                    {
                        track.Add(file);
                        muslyTracks.Add(currentMt);
                        //mTracks.Add(muslyTrack);
                    }

                    /*while ((bytesRead = stream.Read(buffer,0, buffer.Length)) > 0)
                    {
                        string headerString = Encoding.Default.GetString(buffer);

                    }*/
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
        public MuslyTrack(float value)
        {
            this.realValue = value;
        }

        private float realValue;
        public static implicit operator float(MuslyTrack value)
        {
            return value.realValue;
        }

        public static implicit operator MuslyTrack(float value)
        {
            return new MuslyTrack(value);
        }

        public float getFloat()
        {
            return realValue;
        }
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
