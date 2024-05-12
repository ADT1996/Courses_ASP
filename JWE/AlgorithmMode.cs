using System.ComponentModel;

namespace JWE;

public enum AlgorithmMode: byte
{
     /// <summary>
     /// Encrypt and decrypt the key encrypt/decrypt content using RSA algorithm and PKCS#1 padding
     /// </summary>
     /// <remarks>Default value</remarks>
     RSA1_5 = 0,
     /// <summary>
     /// Encrypt and decrypt the key encrypt/decrypt content using RSA algorithm and OAEP padding
     /// </summary>
     RSA_OAEP = 1,
     /// <summary>
     /// Encrypt and decrypt the key encrypt/decrypt using Elliptic-curve Diffie–Hellman algorithm
     /// </summary>
     ECDH_ES = 2,
     /// <summary>
     /// Encrypt and decrypt the key encrypt/decrypt using AES algorithm wrapped key using key length 128 bits
     /// </summary>
     A128KW = 3,
     // <summary>
     /// Encrypt and decrypt the key encrypt/decrypt using AES algorithm wrapped key using key length 256 bits
     /// </summary>
     A256KW = 4,
     /// <summary>
     /// Encrypt content with AES algorithm and Glossary/Counter mode use key length 128 bits
     /// </summary>
     A128GCM = 5,
     /// <summary>
     /// Encrypt content with AES algorithm and Glossary/Counter mode use key length 256 bits
     /// </summary>
     A256GCM = 6
}
public enum EncryptionMode: byte {
     /// <summary>
     /// Encrypt content with AES algorithm and CBC mode use key length 128 bits
     /// </summary>
     /// <remarks>Default value</remarks>
     A128CBC = 0,
     /// <summary>
     /// Encrypt content with AES algorithm and CBC mode use key length 256 bits
     /// </summary>
     A256CBC = 1,
     /// <summary>
     /// Encrypt content with AES algorithm and Glossary/Counter mode use key length 128 bits
     /// </summary>
     A128GCM =2,
     /// <summary>
     /// Encrypt content with AES algorithm and Glossary/Counter mode use key length 256 bits
     /// </summary>
     A256GCM = 3,
}