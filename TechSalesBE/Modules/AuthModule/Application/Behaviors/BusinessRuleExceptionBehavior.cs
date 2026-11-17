using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using MediatR;
using AuthModule.Application.Exceptions;
using AuthModule.Domain.Exceptions;

namespace AuthModule.Application.Behaviors;

internal sealed class BusinessRuleExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DomainException exception)
        {
            switch (exception.Code)
            {
                case DomainExceptionCode.RULE_CANNOT_CREATE_TECHNICAL_ADMIN:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.CannotCreateTechnicalAdmin);
                case DomainExceptionCode.RULE_CANNOT_MANAGE_ADMIN:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.CannotManageAdmin);
                case DomainExceptionCode.RULE_STAFF_CAN_ONLY_MANAGE_CUSTOMERS:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.StaffCanOnlyManageCustomers);
                case DomainExceptionCode.RULE_CANNOT_MANAGE_ACCOUNT:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.CannotManageAccount);
                case DomainExceptionCode.RULE_TECHNICAL_ADMIN_REQUIRED:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.TechnicalAdminRequired);
                case DomainExceptionCode.RULE_CANNOT_MANAGE_SELF:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.CannotManageSelf);
                case DomainExceptionCode.RULE_ACTIVE_ACTOR_REQUIRED:
                    throw new BusinessException(BusinessExceptionCode.FORBIDDEN, BusinessExceptionMessage.ActiveActorRequired);
                case DomainExceptionCode.RULE_EMAIL_ALREADY_REGISTERED:
                    throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.EmailAlreadyRegistered);
                case DomainExceptionCode.RULE_CURRENT_PASSWORD_INCORRECT:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.CurrentPasswordIncorrect);
                case DomainExceptionCode.RULE_PASSWORD_UNCHANGED:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PasswordUnchanged);
                case DomainExceptionCode.RULE_ROLE_UNKNOWN:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.RoleUnknown);
                case DomainExceptionCode.RULE_TECHNICAL_ADMIN_PERMISSIONS_PROTECTED:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.TechnicalAdminPermissionsProtected);
                case DomainExceptionCode.RULE_PERMISSION_UNKNOWN:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PermissionUnknown);
                case DomainExceptionCode.RULE_OTP_INVALID:
                    throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.OtpInvalid);
                default:
                    throw;
            }
        }
    }
}
