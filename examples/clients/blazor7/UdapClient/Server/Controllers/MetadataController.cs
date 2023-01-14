﻿#region (c) 2023 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion

using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Udap.Model;
using Udap.Model.Registration;
using UdapClient.Shared.Model;

namespace UdapClient.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class MetadataController : ControllerBase
{
    [HttpPost("UploadClientCert")]
    public IActionResult UploadClientCert([FromBody] string base64String)
    {
        HttpContext.Session.SetString("clientCert", base64String);
        
        return Ok();
    }

    [HttpPost("BuildSoftwareStatement")]
    public IActionResult BuildSoftwareStatement([FromBody] BuildSoftwareStatementRequest request)
    {
        var now = DateTime.UtcNow;
        var jwtId = CryptoRandom.CreateUniqueId();
        var certBytes = Convert.FromBase64String(HttpContext.Session.GetString("clientCert"));
        var clientCert = new X509Certificate2(certBytes, request.Password);
        var securityKey = new X509SecurityKey(clientCert);
        var signingCredentials = new SigningCredentials(securityKey, UdapConstants.SupportedAlgorithm.RS256);

        var certBase64 = Convert.ToBase64String(clientCert.Export(X509ContentType.Cert));
        var jwtHeader = new JwtHeader
            {
                { "alg", signingCredentials.Algorithm },
                { "x5c", new[] { certBase64 } }
            };

        var document = new UdapDynamicClientRegistrationDocument
        {
            Issuer = "https://fhirlabs.net:7016/fhir/r4",
            Subject = "https://fhirlabs.net:7016/fhir/r4",
            Audience = request.MetadataUrl,
            Expiration = EpochTime.GetIntDate(now.AddMinutes(5).ToUniversalTime()),
            IssuedAt = EpochTime.GetIntDate(now.ToUniversalTime()),
            JwtId = jwtId,
            ClientName = "udapTestClient",
            Contacts = new HashSet<string> { "mailto:Joseph.Shook@Surescripts.com", "mailto:JoeShook@gmail.com" },
            GrantTypes = new HashSet<string> { "client_credentials" },
            TokenEndpointAuthMethod = UdapConstants.RegistrationDocumentValues.TokenEndpointAuthMethodValue,
            Scope = "system/Patient.* system/Practitioner.read"
        };

        var encodedHeader = jwtHeader.Base64UrlEncode();
        var encodedPayload = document.Base64UrlEncode();
        var encodedSignature =
            JwtTokenUtilities.CreateEncodedSignature(string.Concat(encodedHeader, ".", encodedPayload),
                signingCredentials);
        var signedSoftwareStatement = string.Concat(encodedHeader, ".", encodedPayload, ".", encodedSignature);

        var tokenHandler = new JsonWebTokenHandler();
        var jsonToken = tokenHandler.ReadToken(signedSoftwareStatement);
        var requestToken = jsonToken as JsonWebToken;

        var sb = new StringBuilder();
        sb.Append("[");
        sb.Append(Base64UrlEncoder.Decode(requestToken.EncodedHeader));
        sb.Append(",");
        sb.Append(Base64UrlEncoder.Decode(requestToken.EncodedPayload));
        sb.Append("]");

        var softwareStatementBeforeEncoding = JsonObject.Parse(sb?.ToString())
            .ToJsonString(new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        return Ok(softwareStatementBeforeEncoding);
    }

    [HttpPost("BuildRequestBody")]
    public IActionResult BuildRequestBody([FromBody] BuildSoftwareStatementRequest request)
    {
        var now = DateTime.UtcNow;
        var jwtId = CryptoRandom.CreateUniqueId();
        var certBytes = Convert.FromBase64String(HttpContext.Session.GetString("clientCert"));
        var clientCert = new X509Certificate2(certBytes, request.Password);
        var securityKey = new X509SecurityKey(clientCert);
        var signingCredentials = new SigningCredentials(securityKey, UdapConstants.SupportedAlgorithm.RS256);

        var certBase64 = Convert.ToBase64String(clientCert.Export(X509ContentType.Cert));
        var jwtHeader = new JwtHeader
            {
                { "alg", signingCredentials.Algorithm },
                { "x5c", new[] { certBase64 } }
            };

        var document = new UdapDynamicClientRegistrationDocument
        {
            Issuer = "https://fhirlabs.net:7016/fhir/r4",
            Subject = "https://fhirlabs.net:7016/fhir/r4",
            Audience = request.MetadataUrl,
            Expiration = EpochTime.GetIntDate(now.AddMinutes(5).ToUniversalTime()),
            IssuedAt = EpochTime.GetIntDate(now.ToUniversalTime()),
            JwtId = jwtId,
            ClientName = "udapTestClient",
            Contacts = new HashSet<string> { "mailto:Joseph.Shook@Surescripts.com", "mailto:JoeShook@gmail.com" },
            GrantTypes = new HashSet<string> { "client_credentials" },
            TokenEndpointAuthMethod = UdapConstants.RegistrationDocumentValues.TokenEndpointAuthMethodValue,
            Scope = "system/Patient.* system/Practitioner.read"
        };

        var encodedHeader = jwtHeader.Base64UrlEncode();
        var encodedPayload = document.Base64UrlEncode();
        var encodedSignature =
            JwtTokenUtilities.CreateEncodedSignature(string.Concat(encodedHeader, ".", encodedPayload),
                signingCredentials);
        var signedSoftwareStatement = string.Concat(encodedHeader, ".", encodedPayload, ".", encodedSignature);

        var requestBody = new UdapRegisterRequest
        {
            SoftwareStatement = signedSoftwareStatement,
            // Certifications = new string[0],
            Udap = UdapConstants.UdapVersionsSupportedValue
        };

        var formattedJsonString = JsonSerializer.Serialize(
            requestBody,
            new JsonSerializerOptions() { WriteIndented = true });

        return Ok(formattedJsonString);
    }
}
