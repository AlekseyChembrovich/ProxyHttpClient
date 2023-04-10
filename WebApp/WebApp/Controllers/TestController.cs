using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public TestController(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var client = _factory.CreateClient("proxy-client");

        var url = "some url";
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        return Ok(content);
    }
}
