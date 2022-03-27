using FileUpload.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var maxFileSize = builder.Configuration.GetValue<long>("MaxFileSize");
var maxFrameSize = builder.Configuration.GetValue<int>("MaxFrameSize");
var storedFilePath = builder.Configuration.GetValue<string>("StoredFilesPath");

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.MaxRequestBodySize = maxFileSize;
    opts.Limits.MaxResponseBufferSize = maxFileSize;
    opts.Limits.Http2.MaxFrameSize = maxFrameSize;
});
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
var baseFilePath = Path.Combine(Directory.GetCurrentDirectory(), storedFilePath);
var physicalProvider = new PhysicalFileProvider(baseFilePath);
builder.Services.AddSingleton<IFileProvider>(physicalProvider);
// builder.Services.AddScoped<ValidateMimeMultipartContentFilterService>();
builder.Services.AddScoped<FileUploadService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
    // app.UseHsts();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    ctx.Request.EnableBuffering();
    await next().ConfigureAwait(false);
});

app.MapControllers();


app.Run();
