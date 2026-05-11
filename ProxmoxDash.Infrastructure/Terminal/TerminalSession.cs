using Renci.SshNet;

namespace ProxmoxDash.Infrastructure.Terminal;

internal class TerminalSession : IDisposable
{
    public string Id { get; }
    public SshClient Client { get; }
    public ShellStream Shell { get; }
    public Func<string, Task> OnOutput { get; }

    private readonly CancellationTokenSource _cts = new();

    public TerminalSession(string id, SshClient client, ShellStream shell, Func<string, Task> onOutput)
    {
        Id = id;
        Client = client;
        Shell = shell;
        OnOutput = onOutput;
    }

    public void StartOutputPump()
    {
        _ = Task.Run(async () =>
        {
            var buffer = new byte[4096];

            try
            {
                while (!_cts.IsCancellationRequested && Shell.CanRead)
                {
                    var read = await Shell.ReadAsync(buffer.AsMemory(0, buffer.Length), _cts.Token);
                    if (read == 0) break;

                    var text = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
                    await OnOutput(text);
                }
            }
            catch (OperationCanceledException)
            {
                // Session closed, expected
            }
            catch (Exception)
            {
                // Stream errors mean the SSH session is dead; we stop pumping
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { Shell.Close(); } catch { }
        try { Client.Disconnect(); } catch { }
        Shell.Dispose();
        Client.Dispose();
        _cts.Dispose();
    }
}