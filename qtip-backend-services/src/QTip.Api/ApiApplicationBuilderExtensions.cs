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

        // Uncomment once you actually run HTTPS locally or behind a reverse proxy
        // app.UseHttpsRedirection();

        // app.UseCors("Default");

        app.MapHealthChecks("/health");
        app.MapControllers();

        // Nice sanity check endpoint
        app.MapGet("/", () => Results.Ok("QTip API is running"));

        return app;
    }
}