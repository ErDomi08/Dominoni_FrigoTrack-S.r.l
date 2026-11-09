
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Registrazione dei servizi
builder.Services.AddSingleton<Gestionesessioni>();
builder.Services.AddSingleton<DiffieHellman>();
builder.Services.AddSingleton<Crittografia_AES>();
builder.Services.AddSingleton<Controllo_JSON>();
builder.Services.AddSingleton<Salvataggiomisura>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

public partial class Program { }
