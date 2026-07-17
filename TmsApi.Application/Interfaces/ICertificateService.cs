using TmsApi.Dtos;

namespace TmsApi.Services;

public interface ICertificateService
{
    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<CertificateResponseDto?> GetCertificateByIdAsync(
        int id,
        CancellationToken ct);

    Task<CertificateResponseDto> CreateCertificateAsync(
        CreateCertificateRequest request,
        CancellationToken ct);
}