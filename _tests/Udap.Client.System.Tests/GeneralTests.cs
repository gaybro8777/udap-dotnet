#region (c) 2022 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Udap.Client.Client.Extensions;
using Udap.Client.Client.Messages;
using Udap.Common;
using Udap.Common.Certificates;
using Udap.Metadata.Server;
using Udap.Model;
using Udap.Util.Extensions;
using Xunit.Abstractions;
using DiscoveryPolicy = Udap.Client.Client.DiscoveryPolicy;

namespace Udap.Client.System.Tests
{
    public class GeneralTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private FakeChainValidatorDiagnostics _diagnosticsChainValidator = new FakeChainValidatorDiagnostics();

        public GeneralTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test1()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://test.udap.org/fhir/r4/stage/metadata");
            var metadata = await response.Content.ReadAsStringAsync();

            // _testOutputHelper.WriteLine(metadata);
            //
            // Example
            //

            #region example metadata for security.extensions

            /*
            {
                "resourceType": "CapabilityStatement",
                "version": "1636389333424",
                "status": "active",
                "date": "2021-11-08T08:35:33-08:00",
                "kind": "instance",
                "instantiates": [
                "http://hl7.org/fhir/us/core/CapabilityStatement/us-core-server|3.1.1",
                "http://hl7.org/fhir/uv/bulkdata/CapabilityStatement/bulk-data|1.0.0"
                    ],
                "implementation": {
                    "description": "PROD"
                },
                "fhirVersion": "4.0.1",
                "format": [
                "application/fhir+xml",
                "application/fhir+json"
                    ],
                "rest": [
                {
                    "mode": "server",
                    "security": {
                        "extension": [
                        {
                            "url": "http://fhir-registry.smarthealthit.org/StructureDefinition/oauth-uris",
                            "extension": [
                            {
                                "url": "token",
                                "valueUri": "https://test.udap.org/oauth/stage/token"
                            },
                            {
                                "url": "authorize",
                                "valueUri": "https://test.udap.org/oauth/stage/authz"
                            },
                            {
                                "url": "register",
                                "valueUri": "https://test.udap.org/oauth/stage/register"
                            }
                            ]
                        }
                        ],
                        "service": [
                        {
                            "coding": [
                            {
                                "system": "http://hl7.org/fhir/restful-security-service",
                                "code": "SMART-on-FHIR"
                            }
                            ],
                            "text": "OAuth2 using SMART-on-FHIR profile (see http://docs.smarthealthit.org)"
                        },
                        {
                            "coding": [
                            {
                                "system": "http://fhir.udap.org/CodeSystem/capability-rest-security-service",
                                "code": "UDAP"
                            }
                            ],
                            "text": "OAuth 2 using UDAP profile (see http://www.udap.org)"
                        }
                        ]
                    }
                }
            }
            */

            #endregion
        }

        [Fact]
        public async Task UdapClientDiscoveryForIdentityProvider()
        {
            var client = new HttpClient();
            var disco = await client.GetUdapDiscoveryDocument(new UdapDiscoveryDocumentRequest()
            {
                Address = "https://securedcontrols.net:5001",
                Policy = new DiscoveryPolicy()
                {
                    DiscoveryDocumentPath = ".well-known/udap"
                }
            });
            if (disco.IsError)
            {
                _testOutputHelper.WriteLine(disco.Error);
            }
            _testOutputHelper.WriteLine(disco.Json.ToString());
            var discoJsonFormatted = JsonSerializer.Serialize(disco.Json, new JsonSerializerOptions { WriteIndented = true });
            _testOutputHelper.WriteLine(discoJsonFormatted);
        }

        [Fact]
        public async Task RegistrationEndpointExpected()
        {
            var client = new HttpClient();
            var disco = await client.GetUdapDiscoveryDocument(new UdapDiscoveryDocumentRequest()
            {
                Address = "https://securedcontrols.net:5001",
                Policy = new DiscoveryPolicy()
                {
                    DiscoveryDocumentPath = ".well-known/udap"
                }
            });
            if (disco.IsError)
            {
                _testOutputHelper.WriteLine(disco.Error);
            }

            var registrationEndpoint = disco.TryGetString(UdapConstants.Discovery.RegistrationEndpoint);
            registrationEndpoint.Should().BeEquivalentTo("https://securedcontrols.net:5001/connect/register");
        }

        [Fact]
        public async Task Register()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://securedcontrols.net:5001/connect/register");

            var response = await client.RegisterClientAsync(new DynamicClientRegistrationRequest
            {
                Address = "https://securedcontrols.net:5001/connect/register",
                Document = new DynamicClientRegistrationDocument()
            });
        }


        [Fact]
        public async Task UdapClientDiscoveryForFhirServer()
        {
            var client = new HttpClient();
            var disco = await client.GetUdapDiscoveryDocument(new UdapDiscoveryDocumentRequest()
            {
                Address = "https://fhirlabs.net:7016/fhir/r4/.well-known/udap", 
                Policy = new DiscoveryPolicy { 
                    ValidateIssuerName = false, // No issuer name in UDAP Metadata of FHIR Server.
                    ValidateEndpoints = false   // Authority endpoints are not hosted on same domain as Identity Provider.
                }
            });

            //_testOutputHelper.WriteLine(disco.Json.ToString());
            var discoJsonFormatted = JsonSerializer.Serialize(disco.Json, new JsonSerializerOptions { WriteIndented = true });
            //_testOutputHelper.WriteLine(discoJsonFormatted);

            var metadata = JsonSerializer.Deserialize<UdapMetadata>(disco.Json);
            var jwt = new JwtSecurityToken(metadata.SignedMetadata);
            var tokenHeader = jwt.Header;
            // _testOutputHelper.WriteLine(tokenHeader.X5c);
            var x5CArray = JsonNode.Parse(tokenHeader.X5c).AsArray();

            var cert = new X509Certificate2(Convert.FromBase64String(x5CArray.First().ToString()));
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(metadata.SignedMetadata, new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://fhirlabs.net:7016/fhir/r4" }, //With ValidateIssuer = true issuer is validated against this list.  Docs are not clear on this, thus this example.
                ValidateAudience = false, // No aud for UDAP metadata
                ValidateLifetime = true,
                IssuerSigningKey = new X509SecurityKey(cert),
                ValidAlgorithms = new[] { tokenHeader.Alg }, //must match signing algorithm

            }, out SecurityToken valdatedToken);

            var problemFlags = X509ChainStatusFlags.NotTimeValid |
                               X509ChainStatusFlags.Revoked |
                               X509ChainStatusFlags.NotSignatureValid |
                               X509ChainStatusFlags.InvalidBasicConstraints |
                               X509ChainStatusFlags.CtlNotTimeValid |
                               X509ChainStatusFlags.OfflineRevocation |
                               X509ChainStatusFlags.CtlNotSignatureValid;

            (await ValidateCertificateChain(cert, problemFlags, "udap://surefhir.labs")).Should().BeTrue();
            _diagnosticsChainValidator.Called.Should().BeFalse();
        }


        [Fact]
        public async Task UdapClientDiscoveryForHealthToGo()
        {
            var client = new HttpClient();
            var disco = await client.GetUdapDiscoveryDocument(new UdapDiscoveryDocumentRequest()
            {
                Address = "https://stage.healthtogo.me:8181/fhir/r4/stage",
                Policy = new DiscoveryPolicy
                {
                    ValidateIssuerName = false, // No issuer name in UDAP Metadata of FHIR Server.
                    ValidateEndpoints = false   // Authority endpoints are not hosted on same domain as Identity Provider.
                }
            });

            var discoJsonFormatted = JsonSerializer.Serialize(disco.Json, new JsonSerializerOptions { WriteIndented = true });
            // _testOutputHelper.WriteLine(discoJsonFormatted);

            var metadata = JsonSerializer.Deserialize<UdapMetadata>(disco.Json);

            var jwt = new JwtSecurityToken(metadata.SignedMetadata);
            var tokenHeader = jwt.Header;
            // _testOutputHelper.WriteLine(tokenHeader.X5c);
            var x5CArray = JsonNode.Parse(tokenHeader.X5c).AsArray();
            
            var cert = new X509Certificate2(Convert.FromBase64String(x5CArray.First().ToString()));
            var tokenHandler = new JwtSecurityTokenHandler();
            
            tokenHandler.ValidateToken(metadata.SignedMetadata, new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://stage.healthtogo.me:8181/fhir/r4/stage" }, //With ValidateIssuer = true issuer is validated against this list.  Docs are not clear on this, thus this example.
                ValidateAudience = false, // No aud for UDAP metadata
                ValidateLifetime = true,
                IssuerSigningKey = new X509SecurityKey(cert),
                ValidAlgorithms = new[] { tokenHeader.Alg }, //must match signing algorithm
            
            }, out SecurityToken valdatedToken);
            
            var problemFlags = X509ChainStatusFlags.NotTimeValid |
                                      X509ChainStatusFlags.Revoked |
                                      X509ChainStatusFlags.NotSignatureValid |
                                      X509ChainStatusFlags.InvalidBasicConstraints |
                                      X509ChainStatusFlags.CtlNotTimeValid |
                                      X509ChainStatusFlags.OfflineRevocation |
                                      X509ChainStatusFlags.CtlNotSignatureValid;
            
            (await ValidateCertificateChain(cert, problemFlags, "https://stage.healthtogo.me:8181")).Should().BeTrue();
            _diagnosticsChainValidator.Called.Should().BeFalse();
        }

        
        public async Task<bool> ValidateCertificateChain(
            X509Certificate2 issuedCertificate2, 
            X509ChainStatusFlags problemFlags,
            string communityName)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<GeneralTests>()
            .Build();

            var services = new ServiceCollection();
            
            // UDAP CertStore
            services.Configure<UdapFileCertStoreManifest>(configuration.GetSection("UdapFileCertStoreManifest"));
            services.AddSingleton<ICertificateStore>(sp =>
                new FileCertificateStore(
                    sp.GetRequiredService<IOptionsMonitor<UdapFileCertStoreManifest>>(), 
                    new Mock<ILogger<FileCertificateStore>>().Object,
                    "FhirLabsApi"));
            

            var sp = services.BuildServiceProvider();
            var certStore = sp.GetRequiredService<ICertificateStore>();
            var certificateStore = await certStore.Resolve();
            var roots = certificateStore.RootCAs;

            var anchors = certificateStore.Anchors
                .Where(c => c.Community == communityName)
                .OrderBy(c => X509Certificate2.CreateFromPem(c.Certificate).NotBefore)
                .Select(c => c.Certificate);
            
            var validator = new TrustChainValidator(new X509ChainPolicy(), problemFlags, _testOutputHelper.ToLogger<TrustChainValidator>());
            validator.Problem += _diagnosticsChainValidator.OnChainProblem;
            
            // Help while writing tests to see problems summarized.
            validator.Error += (certificate2, exception) => _testOutputHelper.WriteLine("Error: " + exception.Message);
            validator.Problem += element => _testOutputHelper.WriteLine("Problem: " + element.ChainElementStatus.Summarize(problemFlags));
            validator.Untrusted += certificate2 => _testOutputHelper.WriteLine("Untrusted: " + certificate2.Subject);
            
            return validator.IsTrustedCertificate(
                "client_name",
                issuedCertificate2,
                anchors.Select(a => X509Certificate2.CreateFromPem(a)).ToArray().ToX509Collection(),
                out X509ChainElementCollection? chainElements,
                roots.ToArray().ToX509Collection());
        }

        public class FakeChainValidatorDiagnostics
        {
            public bool Called;

            private readonly List<string> _actualErrorMessages = new List<string>();
            public List<string> ActualErrorMessages
            {
                get { return _actualErrorMessages; }
            }

            public void OnChainProblem(X509ChainElement chainElement)
            {
                foreach (var chainElementStatus in chainElement.ChainElementStatus
                             .Where(s => (s.Status & TrustChainValidator.DefaultProblemFlags) != 0))
                {
                    var problem = $"Trust ERROR ({chainElementStatus.Status}){chainElementStatus.StatusInformation}, {chainElement.Certificate}";
                    _actualErrorMessages.Add(problem);
                    Called = true;
                }
            }
        }
    }
}