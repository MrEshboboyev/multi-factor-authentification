using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Users.Constants;
using System.Collections.Immutable;
using System.Text.Json;

namespace Persistence.Users.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Map to the Users table
        builder.ToTable(UserTableNames.Users);

        // Configure the primary key
        builder.HasKey(x => x.Id);

        // Configure property conversions and constraints
        builder
            .Property(x => x.Email)
            .HasConversion(
                x => x.Value,
                v => Email.Create(v).Value);
        
        builder
            .Property(x => x.FullName)
            .HasConversion(
                x => x.Value,
                v => FullName.Create(v).Value)
            .HasMaxLength(FullName.MaxLength);
        
        #region MFA related
        
        // Configure MFA-related properties
        builder
            .Property(x => x.IsMfaEnabled)
            .IsRequired()
            .HasDefaultValue(false); // Default value for IsMfaEnabled is false

        builder
            .Property(x => x.RecoveryCode)
            .HasConversion(
                x => x != null ? x.Value : null, // Convert RecoveryCode to string for storage
                v => v != null ? RecoveryCode.Create(v).Value : null); // Convert string to RecoveryCode when reading
        
        // Configure new MFA properties
        builder
            .Property(x => x.TotpSecret)
            .HasConversion(
                x => x != null ? x.Value : null,
                v => v != null ? TotpSecret.Create(v).Value : null)
            .HasColumnName("TotpSecret");
            
        // Configure BackupCodes as JSON
        builder
            .Property(x => x.BackupCodes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => string.IsNullOrEmpty(v) ? ImmutableList<BackupCode>.Empty : JsonSerializer.Deserialize<ImmutableList<BackupCode>>(v, null as JsonSerializerOptions)!)
            .HasColumnName("BackupCodes");
            
        // Configure TrustedDevices as JSON
        builder
            .Property(x => x.TrustedDevices)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => string.IsNullOrEmpty(v) ? ImmutableList<Device>.Empty : JsonSerializer.Deserialize<ImmutableList<Device>>(v, null as JsonSerializerOptions)!)
            .HasColumnName("TrustedDevices");
            
        // Configure FailedMfaAttempts
        builder
            .Property(x => x.FailedMfaAttempts)
            .HasDefaultValue(0);
            
        // Configure MfaLockedUntil
        builder
            .Property(x => x.MfaLockedUntil)
            .IsRequired(false);

        #endregion

        // Configure unique constraint on Email
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
