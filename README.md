# Cloudflare Resolver

This is Cloudflare resolver.  
It install xvfb, chromium in docker. Open chromium using puppeteer and retrive cloudflare cookies.  
  
To use this send POST request to "api/Resolver" address with body  
```csharp
public class ResolverRequest
{
    public string Url { get; set; }

    public string UserAgent { get; set; }

    public string SelectorForWait { get; set; }
}
```

and you will get result like  
```csharp
public class ResolverResponse
{
    public List<Cookie> Cookies { get; set; }
}

public class Cookie
{
    public string Name { get; set; }

    public string Value { get; set; }
}
```
