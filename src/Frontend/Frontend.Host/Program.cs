using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

var gatewayBaseUrl = builder.Configuration["Gateway:BaseUrl"] ?? "http://gateway:8080";

builder.Services.AddHttpClient("gateway", c =>
{
    c.BaseAddress = new Uri(gatewayBaseUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapMethods("/api/{**path}", new[] { "GET", "POST", "PUT", "PATCH", "DELETE" },
    async (HttpContext ctx, IHttpClientFactory factory, string? path) =>
{
    var client = factory.CreateClient("gateway");

    var targetPath = "/" + (path ?? string.Empty);
    var targetUri = targetPath + ctx.Request.QueryString;

    using var reqMsg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), targetUri);

    // Тело запроса (POST/PUT...)
    if (ctx.Request.ContentLength is > 0)
    {
        reqMsg.Content = new StreamContent(ctx.Request.Body);
        if (!string.IsNullOrWhiteSpace(ctx.Request.ContentType))
            reqMsg.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
    }

    // Заголовки
    foreach (var h in ctx.Request.Headers)
    {
        if (string.Equals(h.Key, "Host", StringComparison.OrdinalIgnoreCase))
            continue;

        if (!reqMsg.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray()))
            reqMsg.Content?.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray());
    }

    using var resp = await client.SendAsync(reqMsg, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);

    ctx.Response.StatusCode = (int)resp.StatusCode;

    foreach (var h in resp.Headers)
        ctx.Response.Headers[h.Key] = h.Value.ToArray();

    foreach (var h in resp.Content.Headers)
        ctx.Response.Headers[h.Key] = h.Value.ToArray();

    ctx.Response.Headers.Remove("transfer-encoding");

    await resp.Content.CopyToAsync(ctx.Response.Body);
});

app.Run();
