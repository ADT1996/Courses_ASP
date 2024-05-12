using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace webapi.Utils;

public static class Utility
{
     public static T getValue<T>(T value, T @default)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (value != null && value.ToString()
            .Trim() != "") return @value;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return @default;
    }

    public static void SetBlankStringToNull(this object value) {
        if (value == null) return;

        var typeValue = value.GetType();
        var members = typeValue.GetMembers();

        foreach (var member in members) {
            if(member.MemberType == MemberTypes.Field ||
                member.MemberType == MemberTypes.Property) {
                    if(member.MemberType == MemberTypes.Field) {
                        var fieldMember = member as FieldInfo;
                        var fieldType = fieldMember?.FieldType;
                        if(fieldType?.FullName == typeof(string).FullName) {
                            var fieldValue = fieldMember?.GetValue(value) as string;
                            if(string.IsNullOrWhiteSpace(fieldValue)) {
                                fieldMember?.SetValue(value, null);
                            } else {
                                fieldMember?.SetValue(value, fieldValue?.Trim());
                            }
                        }
                    } else {
                        var propertyInfo = member as PropertyInfo;
                        var propertyType = propertyInfo?.PropertyType;
                        if(Nullable.GetUnderlyingType(propertyType) != null) {
                            // propertyType.DeclaringType
                            // var fieldValue = propertyInfo?.GetValue(value) as string;
                            // if(string.IsNullOrWhiteSpace(fieldValue)) {
                                 
                            // }
                        }
                    }
                }
                
            }
        }
    public static string GenerateKey(int len) {
        var keyChars = "12klzx345UIO67890qwertpasdfBNMghjcvbnmQWERTYPASDFGyuioHJKLZXCV".ToArray();
        int keyCharsLength = keyChars.Length;
        var random = new Random();
        var key = "";
        for(int i = 0; i < len; i++) {
            key += keyChars[random.Next(0, keyCharsLength)];
        }
        return key;
    }

    public static Aes GetAESCipher(string key, string IV) {
        Aes aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(IV);
        return aes;
    }
    public static string Encrypt(SymmetricAlgorithm algorithm, string data, CipherFormat cipherFormat = default, Encoding encoding = default) {
        if(algorithm == null) {
            throw new ArgumentNullException($"{nameof(algorithm)} must have not been null.");
        }
        if(string.IsNullOrEmpty(data)) {
            return "";
        }
        encoding = encoding ?? Encoding.UTF8;
        var encryptor = algorithm.CreateEncryptor();
        var plaintBytes = encoding.GetBytes(data);
        var cipherBytes = encryptor.TransformFinalBlock(plaintBytes, 0, plaintBytes.Length);
        switch(cipherFormat) {
            case CipherFormat.HEX:
                return Convert.ToHexString(cipherBytes);
            case CipherFormat.BASE64_URL:
                return Base64UrlTextEncoder.Encode(cipherBytes);
            case CipherFormat.BASE64:
                return Convert.ToBase64String(cipherBytes);
            default:
                return Convert.ToHexString(cipherBytes);
        }
    }

    public static string Decrypt(SymmetricAlgorithm algorithm, string data, CipherFormat cipherFormat = default, Encoding encoding = default) {
        var cipherBytes = new byte[0];
        switch(cipherFormat) {
            case CipherFormat.HEX:
                cipherBytes = Convert.FromHexString(data);
                break;
            case CipherFormat.BASE64_URL:
                cipherBytes = Base64UrlTextEncoder.Decode(data);
                break;
            case CipherFormat.BASE64:
                cipherBytes = Convert.FromBase64String(data);
                break;
            default:
                cipherBytes = Convert.FromHexString(data);
                break;
        }
        encoding = encoding ?? Encoding.UTF8;
        var decryptor = algorithm.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return encoding.GetString(plainBytes);
    }
}
