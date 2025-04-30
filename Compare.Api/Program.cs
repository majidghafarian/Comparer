 
using Serilog.Events;
using Serilog;
using Application.IService;
using Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);
// پیکربندی Serilog برای ذخیره لاگ‌ها در فایل
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("LogsCompare/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // اینجا باید UseSerilog() را اضافه کنید.

 

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
//builder.Services.AddScoped<IObjectComparer, ObjectComparer>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7221") // آدرس Blazor
              .AllowAnyHeader()
              .AllowAnyMethod();

    });
});



var app = builder.Build();
app.UseCors("AllowBlazorClient");



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
