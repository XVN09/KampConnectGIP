using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Identity.Business.Contracts.Dto;

public class TenantDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantSlug { get; set; }
}
