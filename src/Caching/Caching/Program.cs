using Caching;
using Caching.Data;
using Marvin.Cache.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Enable Cache Control
builder.Services.AddResponseCaching();

// Using  Marvin.Cache.Headers\
// This library supports HTTP cache headers like Cache-Control,
// Expires, Etag, and Last-Modified and also implements validation and expiration models.
builder.Services.AddHttpCacheHeaders((expirationOpt) =>
    {
        expirationOpt.MaxAge = 65;
        expirationOpt.CacheLocation = CacheLocation.Public;
    },
    (validationOpt) =>
    {
        validationOpt.MustRevalidate = true;
    });

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;

    ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
