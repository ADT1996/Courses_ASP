using System.Collections;
using System.IO.Compression;
using System.Text;
using Jose;
using JWE;

namespace JsonWeb;

public sealed class JWE
{
     public JWE(AlgorithmMode algorithmMode, EncryptionMode encryptionMode, Uri? x5u, byte[]? x5t, List<AlgKey> algKeys, ZipMode zipMode, bool includeEPK) {
        AlgorithmMode = algorithmMode;
        EncryptionMode = encryptionMode;
        X5U = x5u;
        X5T = x5t;
        AlgKeys = algKeys;
        ZipMode = zipMode;
        IncludeEPK = includeEPK;
    }

    private AlgorithmMode AlgorithmMode { get; }
    private EncryptionMode EncryptionMode { get; }
    private Uri? X5U { get; }
    private byte[]? X5T { get; }
    private List<AlgKey> AlgKeys { get; }
    private ZipMode ZipMode { get; }
    private bool IncludeEPK { get; }

    public string EncryptObject(object data) {
          if(AlgKeys == null ||AlgKeys.Count == 0) throw new ArgumentException("AlgKeys is null or empty");
          AlgKey algKey = AlgKeys[new Random().Next() % AlgKeys.Count];
          Jose.JweAlgorithm algorithm = default;
          Jose.JweEncryption jweEncryption = default;
          List<Jose.JweRecipient> recipients = new List<Jose.JweRecipient>();
          object? key = new object();
          var dataPlaintext = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data));
          Dictionary<string, object> headers = new Dictionary<string, object>();
          switch (AlgorithmMode) {
               case AlgorithmMode.RSA1_5:
                    algorithm = Jose.JweAlgorithm.RSA1_5;
                    key = (algKey as RSAAlgKey)?.Key;
               break;
               case AlgorithmMode.RSA_OAEP:
                    algorithm = Jose.JweAlgorithm.RSA_OAEP;
                    key = (algKey as RSAAlgKey)?.Key;
               break;
               case AlgorithmMode.A256GCM:
               case AlgorithmMode.A256KW:
                    algorithm = Jose.JweAlgorithm.A256GCMKW;
                    key = new byte[32];
               break;
               case AlgorithmMode.A128GCM:
               case AlgorithmMode.A128KW:
                    algorithm = Jose.JweAlgorithm.A128GCMKW;
                    key = new byte[16];
               break;
          }

          switch (EncryptionMode)
          {
               case EncryptionMode.A128CBC:
                    jweEncryption = Jose.JweEncryption.A128CBC_HS256;
               break;
               case EncryptionMode.A256GCM:
                    jweEncryption = Jose.JweEncryption.A256GCM;
               break;
               case EncryptionMode.A256CBC:
                    jweEncryption = Jose.JweEncryption.A256CBC_HS512;
               break;
               case EncryptionMode.A128GCM:
                    jweEncryption = Jose.JweEncryption.A128GCM;
               break;
          }
          
          switch (ZipMode)
          {
               case ZipMode.Gzip:
                    headers.Add("zip", "gzip");
                    using(var stream = new MemoryStream()) {
                         var gzip = new GZipStream(stream, CompressionMode.Compress);
                         gzip.Write(dataPlaintext);
                         stream.Position = 0;
                         dataPlaintext = stream.ToArray();
                         gzip.Close();
                    }
               break;
               case ZipMode.Zip:
                    headers.Add("zip", "zip");
                    using(var stream = new MemoryStream()) {
                         stream.Write(dataPlaintext);
                         stream.Position = 0;
                         var zip = new ZipArchive(stream, ZipArchiveMode.Create);
                         dataPlaintext = new Jose.DeflateCompression().Compress(dataPlaintext);
                    }
               break;
               default:
               break;
          }
          headers.Add("kid", algKey.KeyID.ToString());
          recipients.Add(new Jose.JweRecipient(algorithm, key, headers));
          return Jose.JWE.EncryptBytes(
               dataPlaintext ?? new byte[0],
               recipients, jweEncryption
          );
    }
}
