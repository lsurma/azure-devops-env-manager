using AzureDevOpsEnvManager.Models;
using AzureDevOpsEnvManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Azure DevOps settings
builder.Services.Configure<AzureDevOpsConfig>(
    builder.Configuration.GetSection("AzureDevOps"));

// Register Azure DevOps Service
builder.Services.AddScoped<AzureDevOpsService>(sp =>
{
    var config = builder.Configuration.GetSection("AzureDevOps").Get<AzureDevOpsConfig>();
    if (config == null)
    {
        throw new InvalidOperationException("Azure DevOps configuration is missing");
    }
    return new AzureDevOpsService(
        config.OrganizationUrl,
        config.PersonalAccessToken,
        config.ProjectName
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
