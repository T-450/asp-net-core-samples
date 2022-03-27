using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationPatternExample.Business;
using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Notifications;
using DbContext = NotificationPatternExample.Data.DbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
);
builder.Services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("Product"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

builder.Services.AddScoped<INotificator, Notificator>();
builder.Services.AddScoped<ProductService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
//app.UseSwagger();
//app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
