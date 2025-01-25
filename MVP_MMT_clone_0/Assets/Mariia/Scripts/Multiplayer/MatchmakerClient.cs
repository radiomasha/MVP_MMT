using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using StatusOptions = Unity.Services.Matchmaker.Models.MultiplayAssignment.StatusOptions;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class MatchmakerClient : MonoBehaviour
{

   private string _ticketId;
   private const int MAX_POLL_ATTEMPTS = 20;
   private int _currentPollAttempt = 0;
   private void Start()
   {
      SignIn();
   }

   private async void SignIn()
   {
      await ClientSignIn($"Snake_{GetCloneNumberSuffix()}");
      await AuthenticationService.Instance.SignInAnonymouslyAsync();
      StartClient();
   }
   
   private async Task ClientSignIn(string serviceProfileName = null)
   {
      if (serviceProfileName != null)
      {
      #if UNITY_EDITOR
         serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
      #endif
      var initOptions = new InitializationOptions();
      initOptions.SetProfile(serviceProfileName);
      await UnityServices.InitializeAsync(initOptions);
      }
      else
      {
         await UnityServices.InitializeAsync();
      }
    
      Debug.Log($"SignedIn Anonymously as {serviceProfileName}{PlayerID()}");
   }

   private string PlayerID()
   {
      return AuthenticationService.Instance.PlayerId;
   }

   #if UNITY_EDITOR
   private string GetCloneNumberSuffix()
   {
      try {
         string projectPath = ClonesManager.GetCurrentProjectPath();
         int lastUnderscore = projectPath.LastIndexOf('_');
         if (lastUnderscore == -1) return "";
        
         string suffix = projectPath.Substring(lastUnderscore + 1);
         return suffix.Length == 1 ? suffix : "";
      }
      catch {
         return "";
      }
   }
   #endif

   public void StartClient()
   {
      CreateATicket();
   }

   private async void CreateATicket()
   {
      try {
         var options = new CreateTicketOptions("Server2");
         var players = new List<Player>() {
            new Player(PlayerID(), new MatchmakingPlayerData { Skill = 100 }),
         };
         var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
         if (ticketResponse?.Id == null) {
            Debug.LogError("No ticket ID received");
            return;
         }
         _ticketId = ticketResponse.Id;
         PollTicketStatus();
      }
      catch (Exception e) {
         Debug.LogError($"Ticket creation failed: {e.Message}");
      }
   }

   private async void PollTicketStatus()
    {
        try {
            MultiplayAssignment multiplayAssignment = null;
            bool gotAssignment = false;
            int retryCount = 0;
            const int maxRetries = 10;

            do {
                await Task.Delay(TimeSpan.FromSeconds(3f));
                _currentPollAttempt++;
                
                try {
                    Debug.Log($"Polling ticket {_ticketId} - Attempt {_currentPollAttempt}/{MAX_POLL_ATTEMPTS}");
                    var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(_ticketId);
                    retryCount = 0;
                    
                    if (ticketStatus?.Value is MultiplayAssignment assignment) {
                        multiplayAssignment = assignment;
                        Debug.Log($"Ticket status: {multiplayAssignment.Status} - Message: {multiplayAssignment.Message}");
                        
                        switch (multiplayAssignment.Status) {
                            case StatusOptions.Found:
                                gotAssignment = true;
                                TicketAssigned(multiplayAssignment);
                                break;
                            case StatusOptions.InProgress:
                                Debug.Log("Matchmaking in progress...");
                                break;
                            case StatusOptions.Failed:
                            case StatusOptions.Timeout:
                                gotAssignment = true;
                                Debug.LogError($"Ticket failed: {multiplayAssignment.Message}");
                                break;
                        }
                    }
                }
                catch (Exception e) when (e.Message.Contains("429")) {
                    retryCount++;
                    Debug.LogWarning($"Rate limit hit. Retry {retryCount}/{maxRetries}");
                    if (retryCount >= maxRetries) {
                        Debug.LogError("Max retries exceeded");
                        break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5f));
                }

                if (_currentPollAttempt >= MAX_POLL_ATTEMPTS) {
                    Debug.LogError("Max polling attempts reached. Creating new ticket.");
                    CreateATicket();
                    break;
                }
            } while (!gotAssignment);
        }
        catch (Exception e) {
            Debug.LogError($"Polling failed: {e}");
            CreateATicket(); // Retry matchmaking on error
        }
    }

   private void TicketAssigned(MultiplayAssignment assignment)
   {
      Debug.Log($"Ticket assigned: {assignment.Ip}:{assignment.Port}");
      NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
      NetworkManager.Singleton.StartClient();
   }

   public class MatchmakingPlayerData
   {
      public int Skill;
   }
}
