using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Multiplay;
using UnityEngine;
using Unity.Services.Matchmaker.Models;

public class ServerStartup : MonoBehaviour
{
    public static event System.Action ClientInstance;
        
    private const string _internalServerIP = "0.0.0.0";
    private string _externalServerIP = "0.0.0.0";
    private ushort _serverPort = 7777;
    private MatchmakingResults _matchmakingPayload;
    private string _externalConnectionString => $"{_externalServerIP}:{_serverPort}";

    private bool _backfilling = false;
    
    private IMultiplayService _multiplayService;
    private const int _multipalyServiceTimeout = 20000;
    
    private string _allocationId;
    private MultiplayEventCallbacks _serverCallbacks;
    private IServerEvents _serverEvents;
    private BackfillTicket _localBackfillTicket;
    private CreateBackfillTicketOptions _createBackfillTicketOptions;
    private const int _ticketCheckMs = 1000;
    async void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }

            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                _serverPort = (ushort)int.Parse(args[i + 1]);
            }

            if (args[i] == "-ip" && (i + 1 < args.Length))
            {
                _externalServerIP = args[i + 1];
            }
        }

        if (server)
        {
            await StartServerServices(); // Initialize services first
            StartServer();              // Then start server
        }
    }

    private async void StartServer()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found in scene");
            return;
        }
    
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport component not found");
            return;
        }

        transport.SetConnectionData(_internalServerIP, _serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    private async void ClientDisconnected(ulong clientId)
    {
        if (!_backfilling && NetworkManager.Singleton?.ConnectedClients.Count > 0 && NeedsPlayers())
        {
            await BeginBackFilling(_matchmakingPayload);
        }
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            _multiplayService = MultiplayService.Instance;
            await _multiplayService.StartServerQueryHandlerAsync((ushort)9 ,"n/a", "n/a", "0", "n/a");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service: {e.Message}");
        }
        
        try
        {
            _matchmakingPayload = await GetMatchMakerPayload(_multipalyServiceTimeout);
            if (_matchmakingPayload != null)
            {
                Debug.Log($"Got payload: {_matchmakingPayload}");
                await StartBackfill(_matchmakingPayload);
            }
            else
            {
                Debug.LogWarning("Getting the Matchmaking Payload timed out, starting with defaults.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong trying to set up the Allocation & Backfill Services: {e.Message}");
        }
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        _localBackfillTicket = new BackfillTicket{Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties};
        await BeginBackFilling(payload);
    }

    private async Task BeginBackFilling(MatchmakingResults payload)
    {
        _localBackfillTicket ??= new BackfillTicket();
    
        if (string.IsNullOrEmpty(_localBackfillTicket.Id))
        {
            var matchProperties = payload.MatchProperties;
            _createBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = _externalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(matchProperties)
            };
        
            _localBackfillTicket.Id = await MatchmakerService.Instance?.CreateBackfillTicketAsync(_createBackfillTicketOptions);
        }

        _backfilling = true;
        await BackfillLoop();
    }

    private async Task BackfillLoop()
    {
        while (_backfilling && NeedsPlayers() && MatchmakerService.Instance != null && _localBackfillTicket != null)
        {
            _localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_localBackfillTicket.Id);
            if (!NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(_localBackfillTicket.Id);
                _localBackfillTicket = null;
                _backfilling = false;
                return;
            }
            await Task.Delay(_ticketCheckMs);
        }
        _backfilling = false;
    }
    
    private async void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
        }

        if (!string.IsNullOrEmpty(_localBackfillTicket?.Id) && MatchmakerService.Instance != null)
        {
            await MatchmakerService.Instance.DeleteBackfillTicketAsync(_localBackfillTicket.Id);
            _localBackfillTicket = null;
        }
   
        _backfilling = false;

        if (_serverCallbacks != null)
        {
            _serverCallbacks.Allocate -= OnMultiplayAllocation;
        }

        if (_serverEvents != null) 
        {
            await _serverEvents.UnsubscribeAsync();
        }
    }
    
    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.ConnectedClients.Count < 9;
    }

    private async Task<MatchmakingResults> GetMatchMakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if(_multiplayService == null) return null;
        _allocationId = null;
        _serverCallbacks = new MultiplayEventCallbacks();
        _serverCallbacks.Allocate += OnMultiplayAllocation;
        _serverEvents = await _multiplayService.SubscribeToServerEventsAsync(_serverCallbacks);

        _allocationId = await AwaitAllocationID();
        var mmPayload = await GetMatchMakerAllocationPayloadAsync(_multipalyServiceTimeout);

        return mmPayload;
    }

    private async Task<MatchmakingResults> GetMatchMakerAllocationPayloadAsync(int multipalyServiceTimeout)
    {
        try
        {
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"nameof(GetMatchmakerAllocationPayloadAsync):\n{modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong trying to get the MatchMaker Payload in GetMatchMakerAllocationPayloadAsync: {e.Message}");
        }

        return null;
    }

    private async Task<string> AwaitAllocationID()
    {
        var config = _multiplayService.ServerConfig;
        Debug.Log("Awaiting Allocation. Server Config:\n" + 
                  $"-ServerID: {config.ServerId}\n" +
                  $"-AllocationID: {config.AllocationId}\n" +
                  $"-Port: {config.Port}\n" +
                  $"-QPort: {config.QueryPort}\n" +
                  $"-Logs: {config.ServerLogDirectory}");
        
        while (string.IsNullOrEmpty(_allocationId))
        {
            var configId = config.AllocationId;

            if (!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(_allocationId))
            {
                _allocationId = configId;
                break;
            }

            await Task.Delay(100);
        }
        
        return _allocationId;
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");
        if (string.IsNullOrEmpty(allocation.AllocationId)) return;
        _allocationId = allocation.AllocationId;
    }
}
