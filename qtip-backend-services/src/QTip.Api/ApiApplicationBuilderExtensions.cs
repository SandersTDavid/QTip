namespace QTip.Api;

public static class ApiApplicationBuilderExtensions
{
    public static WebApplication UseApi(this WebApplication app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHealthChecks("/health");
        app.MapControllers();
        
        app.MapGet("/", () => Results.Ok("QTip API is running"));

        return app;
    }
}