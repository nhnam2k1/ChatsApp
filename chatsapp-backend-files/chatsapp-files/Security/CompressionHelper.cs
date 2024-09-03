using System.IO.Compression;
using System.Text;

public static class CompressionHelper
{
    public static byte[] Compress(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }
        return mso.ToArray();
    }

    public static byte[] Decompress(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }
        return mso.ToArray();
    }
}
