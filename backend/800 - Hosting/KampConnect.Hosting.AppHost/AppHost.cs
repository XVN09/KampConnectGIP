var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddConnectionString("messaging");
var identityService = builder.AddProject<Projects.KampConnect_Identity_Service>("identity-service").WithReference(messaging);
var tenantService = builder.AddProject<Projects.KampConnect_Tenant_Service>("tenant-service").WithReference(messaging);
var userService = builder.AddProject<Projects.KampConnect_User_Service>("user-service");
var complianceService = builder.AddProject<Projects.KampConnect_Compliance_Service>("compliance-service");
var campService = builder.AddProject<Projects.KampConnect_Camp_Service>("camp-service");
var messagingService = builder.AddProject<Projects.KampConnect_Messaging_Service>("messaging-service");
var callingService = builder.AddProject<Projects.KampConnect_Calling_Service>("calling-service");
var mediaService = builder.AddProject<Projects.KampConnect_Media_Service>("media-service");
var notificationService = builder.AddProject<Projects.KampConnect_Notification_Service>("notification-service");
var billingService = builder.AddProject<Projects.KampConnect_Billing_Service>("billing-service");
var paymentService = builder.AddProject<Projects.KampConnect_Payment_Service>("payment-service");


builder.AddProject<Projects.KampConnect_Gateway>("gateway")
    .WithReference(identityService)
    .WithReference(tenantService)
    .WithReference(userService)
    .WithReference(complianceService)
    .WithReference(campService)
    .WithReference(messagingService)
    .WithReference(callingService)
    .WithReference(mediaService)
    .WithReference(notificationService)
    .WithReference(billingService)
    .WithReference(paymentService);

   

builder.Build().Run();
