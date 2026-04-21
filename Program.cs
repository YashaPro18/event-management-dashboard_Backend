using EventManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudinaryDotNet;
using EventManagement.API.Helpers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;



namespace EventManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args);

            //var options = new WebApplicationOptions
            //{
            //    Args = args,
            //    EnvironmentName = Environments.Production
            //};

            //var builder = WebApplication.CreateBuilder(options);

            //var builder = WebApplication.CreateBuilder(args);
            var options = new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = Environments.Production
            };

            var builder = WebApplication.CreateBuilder(options);

            // Disable file watching (IMPORTANT for Render)
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            //// Disable reload on change
            //builder.Configuration
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            //    .AddEnvironmentVariables();




            if (!FirebaseApp.DefaultInstance?.Equals(null) ?? true)
            {
                //if (File.Exists("firebase-service-account.json"))
                //{
                //    FirebaseApp.Create(new AppOptions()
                //    {
                //        Credential = GoogleCredential
                //            .FromFile("firebase-service-account.json")
                //    });
                //}

                //changed for render deployment (and also commented the upper part)

                var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

                if (!string.IsNullOrEmpty(firebaseJson))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(firebaseJson)
                    });
                }

            }

            //builder.Configuration
            //.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //.AddEnvironmentVariables();


            // -------------------- SERVICES --------------------

            // Controllers
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            //ADD JWT SECURITY CONFIG TO SWAGGER
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                            {
                                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });




            // DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection")));

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // CORS
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAngular",
            //        policy =>
            //        {
            //            policy.AllowAnyOrigin()
            //                  .AllowAnyMethod()
            //                  .AllowAnyHeader();
            //        });
            //});

            //Chaned for render setup CORS
            var frontendUrl = builder.Configuration["FRONTEND_URL"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(frontendUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });



            // 🔹 Cloudinary configuration
            builder.Services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection("Cloudinary"));

            builder.Services.AddSingleton(provider =>
            {
                var config = provider
                    .GetRequiredService<IConfiguration>()
                    .GetSection("Cloudinary")
                    .Get<CloudinarySettings>();

                return new Cloudinary(new Account(
                    config!.CloudName,
                    config.ApiKey,
                    config.ApiSecret
                ));
            });

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    db.Database.Migrate();
            //}
            app.UseHttpsRedirection();


            //move UseCors after UseHttpsRedirection.
            app.UseCors("AllowFrontend");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        
    }
}
