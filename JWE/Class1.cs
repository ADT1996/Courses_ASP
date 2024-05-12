using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JsonWeb;
using JWE;

namespace adt.JsonWeb.JWE;

public sealed class JWEBuilder
{
     private AlgorithmMode algorithmMode = default;
     private EncryptionMode encryptionMode = default;
     private string? epk = null;
     private List<AlgKey> algKeys = new List<AlgKey>();
     private ZipMode zipMode = default;
     private bool includeEPK = default;

    public JWEBuilder UseEPK(bool isUse) {
          this.includeEPK = isUse;
          return this;
     }

     public JWEBuilder SetAlgorithm(AlgorithmMode algorithm) {
          this.algorithmMode = algorithm;
          return this;
     }

     public JWEBuilder SetEncryptionMode(EncryptionMode mode) {
          this.encryptionMode = mode;
          return this;
     }

     public JWEBuilder SetZipMode(ZipMode mode) {
          this.zipMode = mode;
          return this;
     }

     public JWEBuilder AddAlgorithmKey(AlgKey algKey) {
          if(algKeys.FirstOrDefault(_algKey => algKey.KeyID == _algKey.KeyID) != null) {
               throw new ArgumentException($"The algorithm KeyID {algKey.KeyID} already exists");
          }
          this.algKeys.Add(algKey);
          return this;
     }

     public JWEBuilder AddAlgorithmKey(AlgKey[] algKeys) {
          var errorIndex = new StringBuilder();
          foreach(var algKey in this.algKeys) {
               if(algKeys.FirstOrDefault(_algKey => _algKey.KeyID == algKey.KeyID)!= null) {
                    errorIndex.AppendJoin(',',algKey.KeyID);
               }
          }
          if(errorIndex.Length > 0) {
               throw new ArgumentException($"The algorithm KeyID(s) [{errorIndex}] already exists");
          }
          this.algKeys.AddRange(algKeys);
          return this;
     }

     public JWEBuilder ClearAllAlgorithmKeys() {
          this.algKeys.Clear();
          return this;
     }

     public JWEBuilder RemoveAlgorithmKey(AlgKey algKey) {
          algKeys = algKeys.FindAll(_algKey => _algKey.KeyID != algKey.KeyID);
          return this;
     }
     public JWEBuilder RemoveAlgorithmKeys(AlgKey[] algKeys) {
          this.algKeys = this.algKeys.FindAll(algKey => algKeys.FirstOrDefault(_algKey => _algKey.KeyID == algKey.KeyID) == null);
          return this;
     }
     public JWEBuilder RemoveAlgorithmKey(int KeyID) {
          algKeys = algKeys.FindAll((algKey) => algKey.KeyID != KeyID);
          return this;
     }
     public JWEBuilder RemoveAlgorithmKeys(int[] KeyIDs) {
          algKeys = algKeys.FindAll((algKey) => !KeyIDs.Contains(algKey.KeyID));
          return this;
     }

     public JWEBuilder Clone() {
          var newBuilder = new JWEBuilder();
          newBuilder.algKeys = algKeys.ToArray().ToList();
          newBuilder.epk = epk;
          newBuilder.algorithmMode = algorithmMode;
          newBuilder.encryptionMode = encryptionMode;
          newBuilder.includeEPK = includeEPK;
          newBuilder.zipMode = zipMode;
          return newBuilder;
     }
}
