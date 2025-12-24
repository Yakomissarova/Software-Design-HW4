using System.Security.Cryptography;
using System.Text;

namespace Orders.UseCases.Utils;
public static class DeterministicGuid
{
    private static readonly Guid Namespace = Guid.Parse("6b0b1f27-46d8-4b8c-8bb6-93f6c1e9c1d4");

    public static Guid FromLogin(string login)
    {
        login = (login).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException("Login is required");

        // UUIDv5 = SHA1(namespace + name)
        var nsBytes = Namespace.ToByteArray();
        SwapGuidByteOrder(nsBytes);

        var nameBytes = Encoding.UTF8.GetBytes(login);

        byte[] hash;
        using (var sha1 = SHA1.Create())
        {
            sha1.TransformBlock(nsBytes, 0, nsBytes.Length, null, 0);
            sha1.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
            hash = sha1.Hash!;
        }

        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);

        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        SwapGuidByteOrder(newGuid);
        return new Guid(newGuid);
    }

    private static void SwapGuidByteOrder(byte[] guid)
    {
        void Swap(int a, int b) { (guid[a], guid[b]) = (guid[b], guid[a]); }
        Swap(0, 3); Swap(1, 2);
        Swap(4, 5);
        Swap(6, 7);
    }
}