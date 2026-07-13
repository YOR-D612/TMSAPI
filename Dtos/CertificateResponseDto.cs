public record CertificateResponseDto(
    int Id,
    int StudentId,
    string CourseCode,
    DateOnly IssuedOn
);