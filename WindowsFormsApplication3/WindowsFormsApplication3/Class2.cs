using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Threading;

namespace WindowsFormsApplication3
{
    class Class2
    {
        CancellationTokenSource masterCancelToken = new CancellationTokenSource();
        public GameState gameState = new GameState();
        public static Class2 init()
        {
            return new Class2();
        }
        private Class2()
        {
            getData();
        }
        private void getData()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (masterCancelToken.IsCancellationRequested) return;
                    try
                    {
                        if (await WebRequestUtil.IsLive("https://127.0.0.1:2999/liveclientdata/allgamedata"))
                        {
                            break;
                        }
                        else
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                    catch (WebException e)
                    {
                        // TODO: Account for League client disconnects, game ended, etc. without crashing the whole program
                        //throw new InvalidOperationException("Couldn't connect with the game client", e);
                        await Task.Delay(1000);
                        continue;
                    }

                }
                await OnGameInitialized();
            });
        }

        private async Task OnGameInitialized()
        {
            await QueryPlayerInfo(true);
            var _ = Task.Run(async () =>
             {
                 while (true)
                 {
                     if (masterCancelToken.IsCancellationRequested) return;
                     await QueryPlayerInfo();
                     await Task.Delay(150);
                 }
             });
        }
        private async Task QueryPlayerInfo(bool firstTime = false)
        {
            string json;
            try
            {
                json = await WebRequestUtil.GetResponse("https://127.0.0.1:2999/liveclientdata/allgamedata");
            }
            catch (WebException e)
            {
                Console.WriteLine("InvalidOperationException: Game client disconnected");
                throw new InvalidOperationException("Couldn't connect with the game client", e);
            }

            var gameData = JsonConvert.DeserializeObject<dynamic>(json);
            //gameState.GameEvents = (gameData.events.Events as JArray).ToObject<List<Event>>();
            // Get active player info
            gameState.ActivePlayer = ActivePlayer.FromData(gameData.activePlayer);
            // Get player champion info (IsDead, Items, etc)
            //gameState.Champions = (gameData.allPlayers as JArray).ToObject<List<Champion>>();
            //gameState.PlayerChampion = gameState.Champions.Find(x => x.SummonerName == gameState.ActivePlayer.SummonerName);
            // Update active player based on player champion data
            if (gameState.ActivePlayer.Stats.CurrentHealth == 0) {
                gameState.ActivePlayer.IsDead = true;
            } else
            {
                gameState.ActivePlayer.IsDead = false;
            }
            
            // Update champion LED module information
            // Set current game state
            //CurrentGameState = gameState; // This call is possibly not needed because the reference is always the same
            // Get player items
            

            // Process game events
            //ProcessGameEvents(firstTime);

        }
    }
}
