﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Udap.CA.DbContexts;
using Udap.CA.Mappers;
using Udap.Common.Extensions;

namespace Udap.CA.Services;

public class RootCertificateService
{
    private IUdapCaContext _dbContext;
    private IMapper _autoMapper;
    private ILogger<CommunityService> _logger;

    public RootCertificateService(IUdapCaContext dbContext, IMapper autoMapper, ILogger<CommunityService> logger)
    {
        _dbContext = dbContext;
        _autoMapper = autoMapper;
        _logger = logger;
    }

    public async Task<ICollection<ViewModel.RootCertificate>> Get(CancellationToken token = default)
    {
        var rootCertificates = await _dbContext.RootCertificates
            .ToListAsync(cancellationToken: token);

        return _autoMapper.Map<ICollection<ViewModel.RootCertificate>>(rootCertificates);
    }

    public async Task<ViewModel.RootCertificate> Create(ViewModel.RootCertificate rootCertificate, CancellationToken token = default)
    {
        var entity = rootCertificate.ToEntity();
        await _dbContext.RootCertificates.AddAsync(entity, token);
        await _dbContext.SaveChangesAsync(token);

        return entity.ToViewModel();
    }

    public async Task Update(ViewModel.RootCertificate rootCertificate, CancellationToken token = default)
    {
        var entity = await _dbContext.RootCertificates
            .Where(c => c.Id == rootCertificate.Id)
            .SingleOrDefaultAsync(cancellationToken: token);

        if (entity == null)
        {
            _logger.LogDebug($"No Community Id {rootCertificate.Id} found in database. Update failed.");

            return;
        }

        entity.Enabled = rootCertificate.Enabled;
        entity.Name = rootCertificate.Name;
        entity.Certificate = rootCertificate.Certificate.Export(X509ContentType.Pkcs12);
        entity.Thumbprint = rootCertificate.Certificate.Thumbprint;
        entity.BeginDate = rootCertificate.Certificate.NotBefore;
        entity.EndDate = rootCertificate.Certificate.NotAfter;

        await _dbContext.SaveChangesAsync(token);
    }

    public async Task<bool> Delete(int id, CancellationToken token = default)
    {
        var community = await _dbContext.Communities
            .SingleOrDefaultAsync(d => d.Id == id, token);

        if (community == null)
        {
            return false;
        }

        _dbContext.Communities.Remove(community);

        await _dbContext.SaveChangesAsync(token);

        return true;
    }
}