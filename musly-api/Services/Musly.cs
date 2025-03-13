using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class Musly
{
    const string MUSLY_LIB = "/usr/local/bin/musly";

    static Musly()
    {

    }

    private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        IntPtr libHandle = IntPtr.Zero;
        if (libraryName == MUSLY_LIB)
        {
            // Try using the system library 'libmylibrary.so.5'
            NativeLibrary.TryLoad("libmylibrary.so.5", assembly, DllImportSearchPath.System32, out libHandle);
        }
        return libHandle;
    }


    [DllImport(MUSLY_LIB)]
    public static extern IntPtr musly_jukebox_poweron([MarshalAs(UnmanagedType.LPStr)] string method, [MarshalAs(UnmanagedType.LPStr)] string decoder);

    [DllImport(MUSLY_LIB)]
    public static extern int musly_track_binsize(IntPtr jukebox);

    [DllImport(MUSLY_LIB)]
    public static extern IntPtr musly_track_alloc(IntPtr jukebox);

    [DllImport(MUSLY_LIB)]
    public static extern int musly_track_frombin(IntPtr jukebox, byte[] from_buffer, IntPtr to_track);


    [DllImport(MUSLY_LIB)]
    public static extern IntPtr musly_jukebox_fromfile([MarshalAs(UnmanagedType.LPStr)] string jukebox);

    [DllImport(MUSLY_LIB)]
    public static extern int musly_jukebox_trackcount(IntPtr jukebox);

    [DllImport(MUSLY_LIB)]
    public static extern int musly_jukebox_guessneighbors(IntPtr jukebox, IntPtr seed, IntPtr[] neighbors, int numofNeighbors);

    [DllImport(MUSLY_LIB)]
    public static extern int musly_jukebox_similarity(IntPtr jukebox, IntPtr seedTrack, int seedTrackId, IntPtr[] tracks,
        int[] trackIds, int numOfTracks, float[] similarites);

}