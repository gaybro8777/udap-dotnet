﻿using System.Security.Cryptography.X509Certificates;

namespace Udap.Idp.Admin.ViewModel
{
    public class Community
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public bool Enabled { get; set; }

        public bool Default { get; set; }

        public ICollection<Anchor> Anchors { get; set; } = new HashSet<Anchor>();

        public ICollection<RootCertificate> RootCertificates { get; set; } = new HashSet<RootCertificate>();
        
        public ICollection<Certification> Certifications { get; set; } = new HashSet<Certification>();
        
        public bool ShowAnchors { get; set; }

        public bool ShowRootCertificates { get; set; }

        public bool ShowCertifications { get; set; }
    }

    public class Anchor
    {
        public long Id { get; set; }
        public bool Enabled { get; set; }
        public string? Name { get; set; }
        public string? Community { get; set; }
        public long CommunityId { get; set; }
        public X509Certificate2? Certificate { get; set; }
        public string? Thumbprint { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class RootCertificate
    {
        public long Id { get; set; }
        public bool Enabled { get; set; }
        public string? Name { get; set; }
        public X509Certificate2? Certificate { get; set; }
        public string? Thumbprint { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Certification
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class IssuedCertificate
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string? Name { get; set; }
        public string? Community { get; set; }

        public X509Certificate2? Certificate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
