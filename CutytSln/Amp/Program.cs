using WebEssentials.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddOutputCaching(options =>
{
    // Disabling cache by setting duration to 1 - adds with IsMobile usage // TO DO

    var cacheDurationDefault = TimeSpan.FromHours(2).TotalSeconds;
    var cacheDurationShort = TimeSpan.FromMinutes(10).TotalSeconds;

    if (Environment.MachineName.Equals("YTODOROV-NB", StringComparison.InvariantCultureIgnoreCase))
    {
        cacheDurationDefault = 1;
        cacheDurationShort = 1;
    }

    options.Profiles["default"] = new OutputCacheProfile
    {
        Duration = cacheDurationDefault,
        VaryByParam = "c",
        VaryByHeader = "user-agent"
    };

    options.Profiles["short"] = new OutputCacheProfile
    {
        Duration = cacheDurationShort,
        UseAbsoluteExpiration = true,
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseOutputCaching();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
