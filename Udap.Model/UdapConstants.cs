﻿#region (c) 2022 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion

namespace Udap.Model;

/// <summary>
/// <a href="https://build.fhir.org/ig/HL7/fhir-udap-security-ig/branches/main/discovery.html#required-udap-metadata">2.2 Required UDAP Metadata</a>
/// </summary>
public static class UdapConstants
{
    public const string UdapVersionsSupportedValue = "1";
    public const string Community = "community";

    public static class UdapProfilesSupportedValues
    {
        /// <summary>
        /// UDAP Dynamic Client Registration
        /// </summary>
        public const string UdapDcr = "udap_dcr";
        /// <summary>
        /// UDAP JWT-Based Client Authentication
        /// </summary>
        public const string UdapAuthn = "udap_authn";
        /// <summary>
        /// UDAP Client Authorization
        /// </summary>
        public const string UdapAuthz = "udap_authz";
        /// <summary>
        /// UDAP Tiered OAuth
        /// </summary>
        public const string UdapTo = "udap_to";
    }

    public static class RegistrationRequestBody
    {
        public const string SoftwareStatement = "software_statement";

        public const string Certifications = "certifications";

        public const string Udap = "udap";
    }

    public static class RegistrationDocumentValues
    {
        public const string TokenEndpointAuthMethodValue = "private_key_jwt";

        // Fields
        public const string ClientId = "client_id"; // IdentityModel.JwtClaimTypes.ClientId;
        public const string SoftwareStatement = "software_statement";
        public const string Issuer = "iss"; //IdentityModel.JwtClaimTypes.Issuer;
        public const string Subject = "sub"; // IdentityModel.JwtClaimTypes.Subject;
        public const string Audience = "aud"; // IdentityModel.JwtClaimTypes.Audience;
        public const string Expiration = "exp"; // IdentityModel.JwtClaimTypes.Expiration;
        public const string IssuedAt = "iat"; //IdentityModel.JwtClaimTypes.IssuedAt;
        public const string JwtId = "jti"; // IdentityModel.JwtClaimTypes.JwtId;
        public const string ClientName = "client_name";
        public const string ClientUri = "client_uri";
        public const string RedirectUris = "redirect_uris";
        public const string LogoUri = "logo_uri";
        public const string Contacts = "contacts";
        public const string GrantTypes = "grant_types";
        public const string ResponseTypes = "response_types";
        public const string TokenEndpointAuthMethod = "token_endpoint_auth_method";
        public const string Scope = "scope"; // IdentityModel.JwtClaimTypes.Scope;
    }

    /// <summary>
    /// See the <a href="https://www.udap.org/udap-certifications-and-endorsements-stu1.html">
    /// "UDAP CERTIFICATIONS AND ENDORSEMENTS FOR CLIENT APPLICATIONS"</a> profile.
    /// The certification is signed and assembled using JWS compact serialization as per RFC 7515.
    /// </summary>
    public static class CertificationAndEndorsementDocumentValues
    {
        public const string Issuer = "iss"; //IdentityModel.JwtClaimTypes.Issuer;
        public const string Subject = "sub"; // IdentityModel.JwtClaimTypes.Subject;
        public const string Audience = "aud"; // IdentityModel.JwtClaimTypes.Audience;
        public const string Expiration = "exp"; // IdentityModel.JwtClaimTypes.Expiration;
        public const string IssuedAt = "iat"; //IdentityModel.JwtClaimTypes.IssuedAt;
        public const string JwtId = "jti"; // IdentityModel.JwtClaimTypes.JwtId;
        public const string CertificateIssuer = "certification_issuer";
        public const string CertificationName = "certification_name";
        public const string CertificationLogo = "certification_logo";
        public const string CertificationDescription = "certification_description";
        public const string CertificationUris = "certification_uris";
        public const string CertificationStatusEndpoint = "certification_status_endpoint";
        public const string IsEndorsement = "is_endorsement";
        public const string DeveloperName = "developer_name";
        public const string DeveloperAddress = "developer_address";
        public const string ClientName = "client_name";
        public const string SoftwareId = "software_id";
        public const string SoftwareVersion = "software_version";
        public const string ClientUri = "client_uri";
        public const string LogoUri = "logo_uri";
        public const string TosUri = "tos_uri";
        public const string PolicyUri = "policy_uri";
        public const string Contacts = "contacts";
        public const string LaunchUri = "launch_uri";
        public const string RedirectUris = "redirect_uris";
        public const string IpAllowed = "ip_allowed";
        public const string GrantTypes = "grant_types";
        public const string ResponseTypes = "response_types";
        public const string Scope = "scope";
        public const string TokenEndpointAuthMethod = "token_endpoint_auth_method";
        public const string Jwks = "jwks";
    }

;
    public static class SupportedAlgorithm
    {
        public const string RS256 = "RS256";
    }

    public static class UdapAuthorizationExtensions
    {
        public const string Hl7B2B = "hl7-b2b";
    }


    /// <summary>
    /// Huge list.  Would be nice to find this coded somewhere.
    /// </summary>
    public static class FhirScopes
    {
        public const string SystemPatientRead = "system/Patient.read";
        public const string SystemAllergyIntoleranceRead = "system/AllergyIntolerance.read";
        public const string SystemProcedureRead = "system/Procedures.read";
    }

    public static class Discovery
    {
        public const string DiscoveryEndpoint = "/.well-known/udap";
        public const string Issuer = "issuer";
        public const string UdapVersionsSupported = "udap_versions_supported";
        public const string UdapProfilesSupported = "udap_profiles_supported";
        public const string UdapAuthorizationExtensionsSupported = "udap_authorization_extensions_supported";
        public const string UdapAuthorizationExtensionsRequired = "udap_authorization_extensions_required";
        public const string UdapCertificationsSupported = "udap_certifications_supported";
        public const string UdapCertificationsRequired = "udap_certifications_required";
        public const string GrantTypesSupported = "grant_types_supported";
        public const string ScopesSupported = "scopes_supported";
        public const string TokenEndpointAuthMethodsSupported = "token_endpoint_auth_methods_supported";
        public const string TokenEndpointAuthSigningAlgValuesSupported = "token_endpoint_auth_signing_alg_values_supported";
        public const string RegistrationEndpointJwtSigningAlgValuesSupported = "registration_endpoint_jwt_signing_alg_values_supported";
        public const string SignedMetadata = "signed_metadata";


        // endpoints
        public const string AuthorizationEndpoint = "authorization_endpoint";
        public const string TokenEndpoint = "token_endpoint";
        public const string RegistrationEndpoint = "registration_endpoint";
        public const string JwksUri = "jwks_uri";
    }

    public static class JwtClaimTypes
    {
        public static string Extensions = "extensions";
    }

    public static class TokenRequest
    {
        public const string Udap = "udap";
    }

    public static class TokenRequestTypes
    {
        /// <summary>
        /// RFC 6750 1.1 and 2.1.  Bearer name in the header is case sensitive
        /// </summary>
        public const string Bearer = "Bearer";
    }
}

