using BankingMicroservices.ServiceDiscovery.Hosting;
using BankingMicroservices.ServiceDiscovery.Services;
using BankingMicroservices.Shared.Extensions;
using BankingMicroservices.Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.UseBankingSerilog();

var serviceDiscoveryUrl = Environment.GetEnvironmentVariable("SERVICE_DISCOVERY_URL")
    ?? builder.Configuration["Bootstrap:ServiceDiscoveryUrl"]
    ?? "http://localhost:5003";
var configurationServiceUrl = Environment.GetEnvironmentVariable("CONFIGURATION_SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ConfigurationServiceUrl"]
    ?? "http://localhost:5004";
var serviceUrl = Environment.GetEnvironmentVariable("SERVICE_URL")
    ?? builder.Configuration["Bootstrap:ServiceUrl"]
    ?? "http://localhost:5003";

// Services
builder.Services.AddSingleton<IServiceRegistry, ServiceRegistry>();

// Background Services
builder.Services.AddHostedService<StaleServiceCleanupHostedService>();

builder.Services.AddBankingHttpClients();
builder.Services.AddBankingInfrastructure(
    serviceDiscoveryUrl,
    configurationServiceUrl,
    "service-discovery",
    serviceUrl);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseBankingPipeline();

// Configure Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dashboard endpoint - serve the enhanced HTML UI
app.MapGet("/", async (IServiceRegistry serviceRegistry, HttpContext context) =>
{
    var enhancedHtml = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Banking Microservices - Eureka Discovery Server</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        
        body { 
            font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Arial, sans-serif;
            background-color: #f8f9fa;
            color: #212529;
            line-height: 1.5;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        
        .header {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        
        .header h1 {
            color: #495057;
            font-size: 1.75rem;
            font-weight: 500;
            margin-bottom: 5px;
        }
        
        .header p {
            color: #6c757d;
            font-size: 0.95rem;
            margin: 0;
        }
        
        .stats-row {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
        }
        
        .stat-box {
            flex: 1;
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            padding: 15px;
            text-align: center;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        
        .stat-box h3 {
            font-size: 0.8rem;
            font-weight: 600;
            color: #6c757d;
            text-transform: uppercase;
            margin-bottom: 8px;
            letter-spacing: 0.5px;
        }
        
        .stat-number {
            font-size: 1.8rem;
            font-weight: 700;
            line-height: 1;
        }
        
        .stat-number.total { color: #007bff; }
        .stat-number.healthy { color: #28a745; }
        .stat-number.stale { color: #dc3545; }
        .stat-number.response { color: #6f42c1; }
        
        .services-panel {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        
        .panel-header {
            background: #f8f9fa;
            border-bottom: 1px solid #dee2e6;
            padding: 15px 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .panel-header h2 {
            font-size: 1.1rem;
            font-weight: 600;
            color: #495057;
            margin: 0;
        }
        
        .refresh-indicator {
            font-size: 0.85rem;
            color: #6c757d;
        }
        
        .refresh-timer {
            display: inline-block;
            background: #e9ecef;
            padding: 2px 8px;
            border-radius: 3px;
            font-weight: 500;
            margin-left: 5px;
        }
        
        .services-table {
            width: 100%;
        }
        
        .service-row {
            border-bottom: 1px solid #f1f3f4;
            padding: 15px 20px;
            display: flex;
            align-items: center;
            transition: background-color 0.2s;
        }
        
        .service-row:hover {
            background-color: #f8f9fa;
        }
        
        .service-row:last-child {
            border-bottom: none;
        }
        
        .service-status {
            width: 4px;
            height: 40px;
            margin-right: 15px;
            border-radius: 2px;
        }
        
        .service-status.up { background-color: #28a745; }
        .service-status.down { background-color: #dc3545; }
        
        .service-info {
            flex: 1;
        }
        
        .service-name {
            font-size: 1rem;
            font-weight: 600;
            color: #495057;
            margin-bottom: 4px;
        }
        
        .service-details {
            font-size: 0.85rem;
            color: #6c757d;
            margin-bottom: 2px;
        }
        
        .service-url {
            color: #007bff;
            text-decoration: none;
            font-weight: 500;
        }
        
        .service-url:hover {
            text-decoration: underline;
        }
        
        .status-badge {
            padding: 3px 8px;
            border-radius: 3px;
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-left: 15px;
        }
        
        .status-badge.up {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .status-badge.down {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .footer-panel {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            padding: 15px 20px;
            margin-top: 20px;
            text-align: center;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        
        .footer-links {
            display: flex;
            justify-content: center;
            gap: 20px;
            margin-bottom: 10px;
        }
        
        .footer-link {
            color: #007bff;
            text-decoration: none;
            font-size: 0.9rem;
            font-weight: 500;
        }
        
        .footer-link:hover {
            text-decoration: underline;
        }
        
        .last-updated {
            font-size: 0.8rem;
            color: #6c757d;
            margin: 0;
        }
        
        @media (max-width: 768px) {
            .container { padding: 10px; }
            .stats-row { flex-direction: column; }
            .service-row { flex-direction: column; align-items: flex-start; }
            .status-badge { margin-left: 0; margin-top: 5px; }
            .footer-links { flex-direction: column; gap: 10px; }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Eureka</h1>
            <p>Service Discovery Dashboard</p>
        </div>
        
        <div class=""stats-row"" id=""statsRow"">
            <!-- Stats will be loaded here -->
        </div>
        
        <div class=""services-panel"">
            <div class=""panel-header"">
                <h2>Instances currently registered with Eureka</h2>
                <div class=""refresh-indicator"">
                    Auto-refresh in<span class=""refresh-timer"" id=""countdown"">10s</span>
                </div>
            </div>
            <div class=""services-table"" id=""servicesTable"">
                <!-- Services will be loaded here -->
            </div>
        </div>
        
        <div class=""footer-panel"">
            <div class=""footer-links"">
                <a href=""/api/registry"" class=""footer-link"">JSON Registry</a>
                <a href=""/api/stats"" class=""footer-link"">API Statistics</a>
                <a href=""/swagger"" class=""footer-link"">API Documentation</a>
            </div>
            <p class=""last-updated"">Last updated: <span id=""lastUpdated""></span></p>
        </div>
    </div>

    <script>
        var countdownTimer = 10;
        
        function loadServices() {
            console.log('Loading services...');
            
            // Use XMLHttpRequest for better compatibility
            var xhr = new XMLHttpRequest();
            xhr.open('GET', '/api/registry', true);
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        try {
                            var services = JSON.parse(xhr.responseText);
                            console.log('Services loaded:', services);
                            
                            if (!services || !Array.isArray(services)) {
                                throw new Error('Invalid services data');
                            }
                            
                            // Update stats
                            var healthy = 0;
                            for (var i = 0; i < services.length; i++) {
                                if (services[i].status === 'Healthy') {
                                    healthy++;
                                }
                            }
                            var stale = services.length - healthy;
                            var totalUptime = 0;
                            for (var i = 0; i < services.length; i++) {
                                totalUptime += Math.abs(services[i].uptimeSeconds || 0);
                            }
                            var avgUptime = services.length > 0 ? Math.round(totalUptime / services.length) : 0;
                            
                            // Update statistics
                            document.getElementById('statsRow').innerHTML = 
                                '<div class=""stat-box"">' +
                                    '<h3>Total Applications</h3>' +
                                    '<div class=""stat-number total"">' + services.length + '</div>' +
                                '</div>' +
                                '<div class=""stat-box"">' +
                                    '<h3>Available</h3>' +
                                    '<div class=""stat-number healthy"">' + healthy + '</div>' +
                                '</div>' +
                                '<div class=""stat-box"">' +
                                    '<h3>Unavailable</h3>' +
                                    '<div class=""stat-number stale"">' + stale + '</div>' +
                                '</div>' +
                                '<div class=""stat-box"">' +
                                    '<h3>Avg Response</h3>' +
                                    '<div class=""stat-number response"">' + avgUptime + 's</div>' +
                                '</div>';
                            
                            // Update services table
                            var servicesHtml = '';
                            for (var i = 0; i < services.length; i++) {
                                var service = services[i];
                                var isHealthy = service.status === 'Healthy';
                                var statusClass = isHealthy ? 'up' : 'down';
                                var lastHeartbeat = new Date(service.lastHeartbeat).toLocaleString();
                                
                                servicesHtml += 
                                    '<div class=""service-row"">' +
                                        '<div class=""service-status ' + statusClass + '""></div>' +
                                        '<div class=""service-info"">' +
                                            '<div class=""service-name"">' + service.name.toUpperCase() + '</div>' +
                                            '<div class=""service-details"">' +
                                                '<a href=""' + service.url + '"" target=""_blank"" class=""service-url"">' + service.url + '</a>' +
                                            '</div>' +
                                            '<div class=""service-details"">Last heartbeat: ' + lastHeartbeat + '</div>' +
                                        '</div>' +
                                        '<div class=""status-badge ' + statusClass + '"">' + service.status + '</div>' +
                                    '</div>';
                            }
                            
                            document.getElementById('servicesTable').innerHTML = servicesHtml;
                            document.getElementById('lastUpdated').textContent = new Date().toLocaleString();
                            
                        } catch (error) {
                            console.error('Failed to parse services:', error);
                            document.getElementById('servicesTable').innerHTML = 
                                '<div style=""padding: 40px; text-align: center; color: #d0021b;"">Failed to load services: ' + error.message + '</div>';
                        }
                    } else {
                        console.error('HTTP error:', xhr.status);
                        document.getElementById('servicesTable').innerHTML = 
                            '<div style=""padding: 40px; text-align: center; color: #d0021b;"">HTTP Error: ' + xhr.status + '</div>';
                    }
                }
            };
            xhr.send();
        }
        
        function updateCountdown() {
            var countdownElement = document.getElementById('countdown');
            if (countdownElement) {
                countdownElement.textContent = countdownTimer + 's';
            }
            countdownTimer--;
            
            if (countdownTimer < 0) {
                countdownTimer = 10;
                loadServices();
            }
        }
        
        // Wait for page to load
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                loadServices();
            });
        } else {
            loadServices();
        }
        
        // Update countdown every second
        setInterval(updateCountdown, 1000);
        
        // Auto-refresh every 10 seconds
        setInterval(loadServices, 10000);
    </script>
</body>
</html>";

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(enhancedHtml);
});

// API endpoint to get registry in a cleaner format
app.MapGet("/api/registry", (IServiceRegistry serviceRegistry) => 
{
    var services = serviceRegistry.GetAll();
    return Results.Ok(services.Select(s => 
    {
        var timeSinceHeartbeat = (DateTime.UtcNow - s.LastHeartbeat).TotalSeconds;
        var isHealthy = timeSinceHeartbeat < 30;
        
        // Map internal Docker URLs to external accessible URLs with Swagger endpoints
        var externalUrl = s.Url switch
        {
            var url when url.Contains("service-discovery:5003") => "http://localhost:5003/swagger",
            var url when url.Contains("configuration-service:5004") => "http://localhost:5004/swagger", 
            var url when url.Contains("customer-service:5001") => "http://localhost:5001/swagger",
            var url when url.Contains("account-service:5002") => "http://localhost:5002/swagger",
            var url when url.Contains("banking-api-gateway:5000") => "http://localhost:5010/swagger",
            _ => s.Url + "/swagger"
        };
        
        return new 
        {
            name = s.Name,
            url = externalUrl,
            internalUrl = s.Url,
            lastHeartbeat = s.LastHeartbeat,
            status = isHealthy ? "Healthy" : "Stale",
            uptimeSeconds = timeSinceHeartbeat
        };
    }));
});

// API endpoint to get registry statistics
app.MapGet("/api/stats", (IServiceRegistry serviceRegistry) => 
{
    var services = serviceRegistry.GetAll().ToList();
    var healthyCount = services.Count(s => (DateTime.UtcNow - s.LastHeartbeat).TotalSeconds < 30);
    
    return Results.Ok(new
    {
        servicesCount = services.Count,
        healthyCount = healthyCount,
        staleCount = services.Count - healthyCount,
        services = services.Select(s => s.Name).ToList(),
        lastUpdated = DateTime.UtcNow,
        status = healthyCount > 0 ? "UP" : "DOWN"
    });
});

// Health check endpoints
app.MapGet("/health", () => Results.Json(new { status = "UP", service = "service-discovery" }));

app.MapControllers();
app.Run();