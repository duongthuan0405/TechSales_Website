using MailModule.Domain.Enums;

namespace MailModule.Domain.Exceptions;

public class DomainException : Exception
{
    private readonly DomainExceptionCode _domainExceptionCode;

    public DomainExceptionCode DOMAIN_EXCEPTION_CODE => _domainExceptionCode;

    public DomainException(DomainExceptionCode code, string message)
        : this(code, message, null)
    {
    }

    public DomainException(DomainExceptionCode code, string message, Exception? innerException)
        : base(message, innerException)
    {
        if (!Enum.IsDefined(code))
            throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown domain exception code.");

        _domainExceptionCode = code;
    }
}
