using Serilog;

namespace RPGCreator.Core.Common;

public static class ShaUtil
{
    public static string ComputeSha256(string filePath)
    {
        try
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return hashString;
        }
        catch (Exception ex)
        {
            Log.Error("Error computing SHA256 for file '{filePath}': {ex.Message}", filePath, ex.Message);
            return string.Empty;
        }
    }
    
    public static bool VerifySha256(string filePath, string expectedHash)
    {
        try
        {
            var computedHash = ComputeSha256(filePath);
            return string.Equals(computedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Log.Error("Error verifying SHA256 for file '{filePath}': {ex.Message}", filePath, ex.Message);
            return false;
        }
    }
}