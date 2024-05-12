using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace JsonWeb;

public abstract class AlgKey
{
     private int keyID;

     protected AlgKey(int keyID) {
          this.keyID = keyID;
     }
     public int KeyID => keyID;
}
public sealed class RSAAlgKey : AlgKey {
     private RSA? key;
     private X509Certificate2? certificate;
     private bool hasThumbPrint = default;
     public RSAAlgKey(int keyID, RSA? key, X509Certificate2? cert = null, bool hasThumbPrint = default): base(keyID) {
          this.key = key ?? throw new ArgumentNullException("key is null");
          this.certificate = cert;
        this.hasThumbPrint = hasThumbPrint;
     }

     public RSAAlgKey(int keyID, X509Certificate2? cert, bool hasThumbPrint = default): base(keyID) {
        this.certificate = cert ?? throw new ArgumentNullException("cert is null");
        this.hasThumbPrint = hasThumbPrint;
     }

     public RSA? Key => this.key;
     public X509Certificate2? Certificate => this.certificate;
     public bool HasThumbPrint => this.hasThumbPrint;
}

public enum RSAKeyFormat {
     PEM = 0,
     DER = 1
}

public enum ZipMode {
     None = 0,
     Gzip = 1,
     Zip = 2
}

