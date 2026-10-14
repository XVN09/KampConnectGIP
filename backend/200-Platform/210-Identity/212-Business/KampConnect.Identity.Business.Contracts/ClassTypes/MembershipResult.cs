using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Identity.Business.Contracts.ClassTypes;

public enum MembershipError
{
    None = 0,
    AlreadyMember = 1,
    UserUnavailable = 2,
    RoleMissing = 3
}

public record MembershipResult(bool Succeeded,
    MembershipError Error = MembershipError.None,
    string? RoleCode = null,
    IDictionary<string, string[] >? Errors = null)

{
    public static MembershipResult Success(string? roleCode = null) => new(true, RoleCode: roleCode);
    public static MembershipResult Fail(MembershipError error) => new(false, error);
    public static MembershipResult Invalid(IDictionary<string, string[]> errors) => new(false, MembershipError.RoleMissing, Errors: errors);
}
