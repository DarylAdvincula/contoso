using ContosoUniversity.Configs;
using ContosoUniversity.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add interceptors as services
builder.Services.AddSingleton<SchoolInterceptorLogging>();
builder.Services.AddSingleton<SchoolInterceptorTransientErrors>();

builder.Services.AddDbContext<SchoolContext>((serviceProvider, options) =>
{
    // create interceptor services
    var transientErrorInterceptor = serviceProvider.GetRequiredService<SchoolInterceptorTransientErrors>();
    var loggingInterceptor = serviceProvider.GetRequiredService<SchoolInterceptorLogging>();

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // Connection resiliency
        // maximum of 6 retries
        options => options.EnableRetryOnFailure()
    )
        .AddInterceptors(
            transientErrorInterceptor,
            loggingInterceptor
        );
});

var app = builder.Build();

// initialize database
using (var scope = app.Services.CreateScope())
{
    // get school context
    var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();

    DbInitializer.Initialize(context);
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
