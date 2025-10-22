namespace Application.Users.Commands.MfaSetupWizard;

public class MfaSetupWizardResult
{
    public string? RecoveryCode { get; set; }
    public string? TotpQrCodeUrl { get; set; }
    public IEnumerable<string>? BackupCodes { get; set; }
    public string? Message { get; set; }
}
