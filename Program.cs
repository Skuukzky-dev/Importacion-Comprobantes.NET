using API_Maestros_Core.BLL;
using API_Maestros_Core.Services;
using APIImportacionComprobantes.BO;
using Importacion_Comprobantes.NET.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped(typeof(IAuthService), typeof(AuthService));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(options =>
        options.DefaultPolicy =
        new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build()
    );
var issuer = builder.Configuration["AuthenticationSettings:Issuer"];
var audience = builder.Configuration["AuthenticationSettings:Audience"];
var signinKey = builder.Configuration["AuthenticationSettings:SigningKey"];


var myCorsPolicy = "MyCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myCorsPolicy,
       policy =>
       {
           policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("MiHeaderPersonalizado"); ;
       });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signinKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = async (context) =>
        {
            Console.WriteLine("Printing in the delegate OnAuthFailed");
        },
        OnChallenge = async (context) =>
        {
            Console.WriteLine("Printing in the delegate OnChallenge");

            // this is a default method
            // the response statusCode and headers are set here
            context.HandleResponse();

            // AuthenticateFailure property contains 
            // the details about why the authentication has failed
            if (context.AuthenticateFailure != null)
            {
                context.Response.StatusCode = 401;
                RespuestaToken tok = new RespuestaToken();
                tok.success = false;
                tok.error = new ErrorToken();
                tok.error.code = 4012;
                context.Response.ContentType = "application/json";
                //  var context1 = HttpContext.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme).Result;
                tok.error.message = "Token invalido. Acceso denegado. Ips: " + context.HttpContext.Connection.RemoteIpAddress;
              //  Logger.LoguearErrores("Token invalido. Acceso denegado Ips: " + context.HttpContext.Connection.RemoteIpAddress, "I", "", context.Request.Path.Value);

                // we can write our own custom response content here
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(tok));
            }
            else
            {
                context.Response.StatusCode = 401;
                RespuestaToken tok = new RespuestaToken();
                tok.success = false;
                tok.error = new ErrorToken();
                tok.error.code = 4013;
                context.Response.ContentType = "application/json";
                tok.error.message = "No se encontro el Token en el Request";
              //  Logger.LoguearErrores("No se encontro el Token en el Request", "I", "", context.Request.Path.Value);
                // we can write our own custom response content here
                await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(tok));
            }


        },
        OnTokenValidated = async (context) =>
        {
            //  GESI.CORE.BO.Verscom2k.APILogin MiObjetoLogin = GESI.CORE.BLL.Verscom2k.ApiLoginMgr.GetItem(context.SecurityToken["RowData"]);
            var accessToken = context.SecurityToken as JwtSecurityToken;
            if (accessToken != null)
            {
                ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    identity.AddClaim(new Claim("access_token", accessToken.RawData));
                    ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                    fileMap.ExeConfigFilename = System.IO.Directory.GetCurrentDirectory() + "\\app.config";
                    System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);


                    SqlConnection sqlapi = new SqlConnection(config.ConnectionStrings.ConnectionStrings["ConexionVersCom2k"].ConnectionString);
                    GESI.CORE.DAL.Configuracion._ConnectionString = sqlapi.ConnectionString;

                    GESI.CORE.BO.Verscom2k.APILogin MiObjetoLogin = GESI.CORE.BLL.Verscom2k.ApiLoginMgr.GetItem(accessToken.RawData);

                    if (MiObjetoLogin != null) // ESTA OK. ENCONTRO EL TOKEN EN LA TABLA DE TOKENS X USUARIO
                    {
                        comprobantesController.mstrUsuarioID = MiObjetoLogin.UsuarioID;
                        comprobantesController.HabilitadoPorToken = true;
                        comprobantesController.Token = accessToken.RawData;
                        /*
                        ProductosController.strUsuarioID = MiObjetoLogin.UsuarioID;
                        ProductosController.HabilitadoPorToken = true;
                        ProductosController.TokenEnviado = accessToken.RawData;

                        CanalesDeVentaController.strUsuarioID = MiObjetoLogin.UsuarioID;
                        CanalesDeVentaController.HabilitadoPorToken = true;
                        CanalesDeVentaController.TokenEnviado = accessToken.RawData;

                        CategoriasController.strUsuarioID = MiObjetoLogin.UsuarioID;
                        CategoriasController.HabilitadoPorToken = true;
                        CategoriasController.TokenEnviado = accessToken.RawData;


                        EmpresasController.strUsuarioID = MiObjetoLogin.UsuarioID;
                        EmpresasController.HabilitadoPorToken = true;
                        EmpresasController.TokenEnviado = accessToken.RawData;
                        */

                    }
                    else // NO LO ENCONTRO EN LA BASE
                    {
                        comprobantesController.HabilitadoPorToken = true;
                        /*
                        ProductosController.HabilitadoPorToken = false;
                        CanalesDeVentaController.HabilitadoPorToken = false;
                        CategoriasController.HabilitadoPorToken = false;
                        EmpresasController.HabilitadoPorToken = false;
                        */

                    }
                }
            }

        }
    };
}
);



var app = builder.Build();
app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

app.Run();
