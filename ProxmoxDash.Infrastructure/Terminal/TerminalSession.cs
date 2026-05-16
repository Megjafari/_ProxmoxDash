using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ProxmoxDash.Infrastructure.Terminal;

internal class TerminalSession : IDisposable
{
    public string Id { get; }
    public Process Process { get; }
    public Func<string, Task> OnOutput { get; }

    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cts = new();

    public TerminalSession(string id, Process process, Func<string, Task> onOutput, ILogger logger)
    {
        Id = id;
        Process = process;
        OnOutput = onOutput;
        _logger = logger;
    }

    public void StartOutputPump()
    {
        // stdout pump
        _ = Task.Run(async () =>
        {
            var buffer = new char[4096];
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var read = await Process.StandardOutput.ReadAsync(
                        buffer.AsMemory(0, buffer.Length), _cts.Token);
                    if (read == 0)
                    {
                        if (Process.HasExited) break;
                        await Task.Delay(50, _cts.Token);
                        continue;
                    }
                    await OnOutput(new string(buffer, 0, read));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Terminal session {SessionId} stdout pump ended", Id);
            }
        }, _cts.Token);

        // stderr pump — ssh writes auth prompts and warnings here
        _ = Task.Run(async () =>
        {
            var buffer = new char[4096];
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var read = await Process.StandardError.ReadAsync(
                        buffer.AsMemory(0, buffer.Length), _cts.Token);
                    if (read == 0)
                    {
                        if (Process.HasExited) break;
                        await Task.Delay(50, _cts.Token);
                        continue;
                    }
                    await OnOutput(new string(buffer, 0, read));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Terminal session {SessionId} stderr pump ended", Id);
            }
        }, _cts.Token);
    }

    public void WriteInput(string input)
    {
        if (Process.HasExited) return;
        try
        {
            Process.StandardInput.Write(input);
            Process.StandardInput.Flush();
        }
        catch
        {
            // Process died between HasExited check and Write — safe to swallow
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try
        {
            if (!Process.HasExited)
            {
                Process.Kill(entireProcessTree: true);
            }
        }
        catch { }
        Process.Dispose();
        _cts.Dispose();
    }
}