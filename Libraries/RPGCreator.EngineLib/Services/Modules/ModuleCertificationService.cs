// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RPGCreator.EngineLib.Common;
using RPGCreator.SDK.Common.Logging;

namespace RPGCreator.EngineLib.Services.Modules;

public class ModuleCertificationService
{
    private readonly ScopedLogger _logger = SDK.Common.Logging.Logger.ForContext<ModuleCertificationService>();
    
    private const string PublicKey =
        """
        -----BEGIN PUBLIC KEY-----
        MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEWGEyQWgNEd+7DULoGRowlXjSR7/I
        ljfzdojH6bX/gw80ZczAyxv18+kko/XbNi5XPM2vxaA9eNj5XrVKN7Co/g==
        -----END PUBLIC KEY-----
        """;

    public (SDK.Modules.Module.CertificationState cert, string fileHash) CheckCertification(
        string originalDllFilePath)
    {
        _logger.Debug("Checking certification for module at path: {Path}", args: originalDllFilePath);
        
        string actualHash = ShaUtil.ComputeSha256(originalDllFilePath);
        
        try
        {
            string certPath = Path.ChangeExtension(originalDllFilePath, ".RCert");
            if (!File.Exists(certPath)) return (SDK.Modules.Module.CertificationState.NotCertified, actualHash);

            var certContent = File.ReadAllText(certPath);
            using var doc = JsonDocument.Parse(certContent);
            var root = doc.RootElement;

            string signature64 = root.GetProperty("Signature").GetString()!;
            string rawData = root.GetProperty("Data").GetString()!;

            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(PublicKey);

            byte[] signature = Convert.FromBase64String(signature64);
            byte[] rawDataBytes = Encoding.UTF8.GetBytes(rawData);

            if (!ecdsa.VerifyData(rawDataBytes, signature, HashAlgorithmName.SHA256))
                return (SDK.Modules.Module.CertificationState.ModifiedCertification, actualHash);

            using var certDataDoc = JsonDocument.Parse(rawData);
            var certDataRoot = certDataDoc.RootElement;
            var dllHash = certDataRoot.GetProperty("DllHash").GetString()!;

            if (actualHash.Equals(dllHash, StringComparison.OrdinalIgnoreCase))
            {
                return (SDK.Modules.Module.CertificationState.Certified, actualHash);
            }

            return (SDK.Modules.Module.CertificationState.DllModified, actualHash);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to check module certification. Exception: {Exception}", args: ex);
            return (SDK.Modules.Module.CertificationState.NotCertified, actualHash);
        }
    }

}