using Microsoft.AspNetCore.Mvc;
using Dbets.Domain.Common;
using Dapper;
using System.Diagnostics;

namespace Dbets.Api.Controllers;

/// <summary>
/// Controller para verificação de saúde da aplicação e seus componentes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IUnitOfWork unitOfWork, ILogger<HealthController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Verificação básica de saúde da API
    /// </summary>
    /// <returns>Status da API</returns>
    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var healthCheck = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "Dbets API",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            _logger.LogInformation("Health check executado com sucesso");
            return Ok(healthCheck);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no health check da API");
            
            var healthCheck = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Service = "Dbets API",
                Version = "1.0.0",
                Error = ex.Message
            };

            return StatusCode(503, healthCheck);
        }
    }

    /// <summary>
    /// Verificação de saúde do banco de dados
    /// </summary>
    /// <returns>Status da conexão com o banco</returns>
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Testa a conexão executando uma query simples
            var result = await _unitOfWork.Connection.QuerySingleAsync<int>("SELECT 1");
            
            stopwatch.Stop();
            
            if (result == 1)
            {
                var healthyResponse = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "PostgreSQL Database",
                    Message = "Conexão com banco de dados bem-sucedida",
                    ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    DatabaseVersion = await GetDatabaseVersion()
                };

                _logger.LogInformation("Health check do banco executado com sucesso em {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return Ok(healthyResponse);
            }
            else
            {
                var unhealthyResponse = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "PostgreSQL Database",
                    Message = "Resposta inesperada do banco de dados"
                };
                
                return StatusCode(503, unhealthyResponse);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Falha no health check do banco de dados");
            
            var errorResponse = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Service = "PostgreSQL Database",
                Message = "Falha na verificação do banco de dados",
                Error = ex.Message,
                ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms"
            };

            return StatusCode(503, errorResponse);
        }
    }

    /// <summary>
    /// Verificação detalhada de saúde de todos os componentes
    /// </summary>
    /// <returns>Status detalhado de todos os componentes</returns>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var checks = new List<object>();
        var overallStatus = "Healthy";
        var overallStopwatch = Stopwatch.StartNew();

        // Verificação da API
        try
        {
            var apiStopwatch = Stopwatch.StartNew();
            
            // Simula verificações internas da API
            await Task.Delay(1); // Simula processamento
            
            apiStopwatch.Stop();
            
            checks.Add(new
            {
                Component = "API",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                ResponseTime = $"{apiStopwatch.ElapsedMilliseconds}ms",
                Details = new
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount
                }
            });
        }
        catch (Exception ex)
        {
            overallStatus = "Unhealthy";
            checks.Add(new
            {
                Component = "API",
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }

        // Verificação do Banco de Dados
        var dbStopwatch = Stopwatch.StartNew();
        try
        {
            // Testa conexão básica
            var connectionTest = await _unitOfWork.Connection.QuerySingleAsync<int>("SELECT 1");
            
            // Testa uma operação mais complexa
            var userCount = await _unitOfWork.Connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM users WHERE deleted_at IS NULL");
            
            var dbVersion = await GetDatabaseVersion();
            
            dbStopwatch.Stop();
            
            if (connectionTest == 1)
            {
                checks.Add(new
                {
                    Component = "PostgreSQL Database",
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    ResponseTime = $"{dbStopwatch.ElapsedMilliseconds}ms",
                    Details = new
                    {
                        Version = dbVersion,
                        ActiveUsers = userCount,
                        ConnectionState = _unitOfWork.Connection.State.ToString()
                    }
                });
            }
            else
            {
                overallStatus = "Unhealthy";
                checks.Add(new
                {
                    Component = "PostgreSQL Database",
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Message = "Falha no teste de conexão"
                });
            }
        }
        catch (Exception ex)
        {
            dbStopwatch.Stop();
            overallStatus = "Unhealthy";
            checks.Add(new
            {
                Component = "PostgreSQL Database",
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message,
                ResponseTime = $"{dbStopwatch.ElapsedMilliseconds}ms"
            });
        }

        // Verificação de Memória
        try
        {
            var memoryStopwatch = Stopwatch.StartNew();
            
            var workingSet = Environment.WorkingSet;
            var gcMemory = GC.GetTotalMemory(false);
            
            memoryStopwatch.Stop();
            
            var memoryStatus = "Healthy";
            var memoryMB = workingSet / 1024 / 1024;
            
            // Considera unhealthy se usar mais de 1GB
            if (memoryMB > 1024)
            {
                memoryStatus = "Warning";
            }
            
            checks.Add(new
            {
                Component = "Memory",
                Status = memoryStatus,
                Timestamp = DateTime.UtcNow,
                ResponseTime = $"{memoryStopwatch.ElapsedMilliseconds}ms",
                Details = new
                {
                    WorkingSetMB = memoryMB,
                    GCMemoryMB = gcMemory / 1024 / 1024,
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2)
                }
            });
        }
        catch (Exception ex)
        {
            checks.Add(new
            {
                Component = "Memory",
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }

        overallStopwatch.Stop();

        var response = new
        {
            Status = overallStatus,
            Timestamp = DateTime.UtcNow,
            Service = "Dbets API",
            Version = "1.0.0",
            TotalResponseTime = $"{overallStopwatch.ElapsedMilliseconds}ms",
            Checks = checks
        };

        _logger.LogInformation("Health check detalhado executado: {Status} em {ElapsedMs}ms", 
            overallStatus, overallStopwatch.ElapsedMilliseconds);

        return overallStatus == "Healthy" ? Ok(response) : StatusCode(503, response);
    }

    /// <summary>
    /// Endpoint para verificar se a aplicação está pronta para receber tráfego
    /// </summary>
    /// <returns>Status de prontidão</returns>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Verifica se consegue conectar ao banco
            await _unitOfWork.Connection.QuerySingleAsync<int>("SELECT 1");
            
            var readyResponse = new
            {
                Status = "Ready",
                Timestamp = DateTime.UtcNow,
                Message = "Aplicação pronta para receber tráfego"
            };
            
            return Ok(readyResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Aplicação não está pronta");
            
            var notReadyResponse = new
            {
                Status = "NotReady",
                Timestamp = DateTime.UtcNow,
                Message = "Aplicação não está pronta para receber tráfego",
                Error = ex.Message
            };
            
            return StatusCode(503, notReadyResponse);
        }
    }

    /// <summary>
    /// Endpoint para verificar se a aplicação está viva
    /// </summary>
    /// <returns>Status de vida</returns>
    [HttpGet("live")]
    public IActionResult Live()
    {
        var liveResponse = new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
        };
        
        return Ok(liveResponse);
    }

    /// <summary>
    /// Obtém a versão do banco de dados PostgreSQL
    /// </summary>
    /// <returns>Versão do banco</returns>
    private async Task<string> GetDatabaseVersion()
    {
        try
        {
            var version = await _unitOfWork.Connection.QuerySingleAsync<string>("SELECT version()");
            return version.Split(' ')[1]; // Extrai apenas o número da versão
        }
        catch
        {
            return "Unknown";
        }
    }
}