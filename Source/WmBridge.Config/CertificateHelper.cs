//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using CERTENROLLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WmBridge.Config
{
    public static class CertificateHelper
    {
        public static string CertificateName(X509Certificate2 cert)
        {
            string str = cert.Subject;

            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            bool inKey = true, inValue = false, inQuote = false;
            string key = "", value = "";
            int start = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (inKey)
                {
                    i = str.IndexOf('=', start);
                    key = str.Substring(start, i - start).Trim();
                    inValue = true;
                    inKey = false;

                    i++;
                    inQuote = str[i] == '\"';
                    if (inQuote) i++;

                    start = i;
                }

                bool valueEnd = false;
                if (inValue)
                {
                    if (inQuote)
                    {
                        if (str[i] == '"' && str[i - 1] != '\\') // skip escaped backslash
                            valueEnd = true;
                    }
                    else
                    {
                        if (str[i] == ',')
                            valueEnd = true;
                    }

                    if (!valueEnd && i >= str.Length - 1)
                    {
                        valueEnd = true;
                        i++;
                    }
                }

                if (valueEnd)
                {
                    value = str.Substring(start, i - start);

                    if (inQuote)
                        value = value.Replace("\\\"", "\"");

                    list.Add(new KeyValuePair<string, string>(key, value));

                    inValue = false;
                    inKey = true;
                    start = i + 1;
                }
            }

            var dict = list.ToLookup(x => x.Key, x => x.Value);

            if (dict.Contains("CN"))
                return dict["CN"].First();

            if (dict.Contains("OU"))
                return dict["OU"].First();

            if (dict.Contains("O"))
                return dict["O"].First();

            return str;
        }

        public static X509Certificate2 CreateSelfSignedCA(string subjectName, int days)
        {
            // create DN for subject and issuer
            var dn = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            // create a new private key for the certificate
            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft Base Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
            privateKey.Create();

            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

            CX509ExtensionKeyUsage keyUsage = new CX509ExtensionKeyUsage();
            keyUsage.InitializeEncode(
                CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE |
                CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_CERT_SIGN_KEY_USAGE |
                CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_CRL_SIGN_KEY_USAGE |
                CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_OFFLINE_CRL_SIGN_KEY_USAGE);

            CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints();
            bc.InitializeEncode(true, 0);
            bc.Critical = false;

            // add extended key usage if you want - look at MSDN for a list of possible OIDs
            var oid = new CObjectId();
            oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server
            var oidlist = new CObjectIds();
            oidlist.Add(oid);
            var eku = new CX509ExtensionEnhancedKeyUsage();
            eku.InitializeEncode(oidlist);

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = dn; // the issuer and the subject are the same
            cert.NotBefore = DateTime.UtcNow.Date.AddDays(-1);
            
            cert.NotAfter = DateTime.UtcNow.Date.AddDays(days);
            cert.X509Extensions.Add((CX509Extension)keyUsage);
            cert.X509Extensions.Add((CX509Extension)eku); // add the EKU
            cert.X509Extensions.Add((CX509Extension)bc);
            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            // Do the final enrollment process
            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert); // load the certificate
            enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name
            string csr = enroll.CreateRequest(); // Output the request in base64
            // and install it back as the response

            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, ""); // no password

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX("", // no password, this is for internal consumption
                PFXExportOptions.PFXExportChainWithRoot);

            // instantiate the target class with the PKCS#12 data (and the empty password)
            var x509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                System.Convert.FromBase64String(base64encoded), "",
                // mark the private key as exportable (this is usually what you want to do)
                System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable
            );

            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            // install to CA store
            store.Add(new X509Certificate2(x509Certificate2) { PrivateKey = null });
            store.Close();

            return x509Certificate2;
        }

        public static X509Certificate2 CreateCertificate(string subjectName, int days, X509Certificate2 issuer)
        {
            CSignerCertificate signerCertificate = new CSignerCertificate();
            signerCertificate.Initialize(true, X509PrivateKeyVerify.VerifyNone, EncodingType.XCN_CRYPT_STRING_HEX, issuer.GetRawCertDataString());

            // create DN for subject and issuer
            var dn = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            // create a new private key for the certificate
            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft Base Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
            privateKey.Create();

            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                AlgorithmFlags.AlgorithmFlagsNone, "SHA256");

            CX509ExtensionKeyUsage keyUsage = new CX509ExtensionKeyUsage();
            keyUsage.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DATA_ENCIPHERMENT_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE);

            CX509ExtensionBasicConstraints bc = new CX509ExtensionBasicConstraints();
            bc.InitializeEncode(false, 0);
            bc.Critical = false;

            // add extended key usage if you want - look at MSDN for a list of possible OIDs
            var oid = new CObjectId();
            oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server
            var oidlist = new CObjectIds();
            oidlist.Add(oid);
            var eku = new CX509ExtensionEnhancedKeyUsage();
            eku.InitializeEncode(oidlist);

            var dnIssuer = new CX500DistinguishedName();
            dnIssuer.Encode(issuer.Subject, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = dnIssuer;
            cert.SignerCertificate = signerCertificate;
            cert.NotBefore = DateTime.UtcNow.Date.AddDays(-1);

            cert.NotAfter = DateTime.UtcNow.Date.AddDays(days);
            cert.X509Extensions.Add((CX509Extension)keyUsage);
            cert.X509Extensions.Add((CX509Extension)eku); // add the EKU
            cert.X509Extensions.Add((CX509Extension)bc);

            /*
            var ski = new CX509ExtensionAuthorityKeyIdentifier();
            ski.InitializeEncode(EncodingType.XCN_CRYPT_STRING_BINARY, cert.PublicKey.ComputeKeyIdentifier(KeyIdentifierHashAlgorithm.SKIHashSha1, EncodingType.XCN_CRYPT_STRING_BINARY));
            cert.X509Extensions.Add((CX509Extension)ski);
            */

            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            // Do the final enrollment process
            var enroll = new CX509Enrollment();
            enroll.InitializeFromRequest(cert); // load the certificate
            enroll.CertificateFriendlyName = subjectName; // Optional: add a friendly name
            string csr = enroll.CreateRequest(); // Output the request in base64
            // and install it back as the response

            enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, csr, EncodingType.XCN_CRYPT_STRING_BASE64, ""); // no password

            // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
            var base64encoded = enroll.CreatePFX("", // no password, this is for internal consumption
                PFXExportOptions.PFXExportChainWithRoot);

            // instantiate the target class with the PKCS#12 data (and the empty password)
            var x509Certificate2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                System.Convert.FromBase64String(base64encoded), "",
                // mark the private key as exportable (this is usually what you want to do)
                System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);

            return x509Certificate2;
        }

    }
}
