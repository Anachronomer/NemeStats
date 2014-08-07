﻿using BusinessLogic.DataAccess;
using BusinessLogic.Models;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Logic.GameDefinitions
{
    public class GameDefinitionRetrieverImpl : GameDefinitionRetriever
    {
        protected DataContext dataContext;

        public GameDefinitionRetrieverImpl(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public IList<GameDefinition> GetAllGameDefinitions(ApplicationUser currentUser)
        {
            return dataContext.GetQueryable<GameDefinition>(currentUser)
                .Where(gameDefinition => gameDefinition.GamingGroupId == currentUser.CurrentGamingGroupId.Value)
                .ToList();
        }

        public GameDefinition GetGameDefinitionDetails(int id, int numberOfPlayedGamesToRetrieve, ApplicationUser currentUser)
        {
            GameDefinition gameDefinition = GetGameDefinition(id, currentUser);
            IList<PlayedGame> playedGames = AddPlayedGamesToTheGameDefinition(numberOfPlayedGamesToRetrieve, currentUser, gameDefinition);
            IList<int> distinctPlayerIds = AddPlayerGameResultsToEachPlayedGame(currentUser, playedGames);
            AddPlayersToPlayerGameResults(currentUser, playedGames, distinctPlayerIds);
             
            //TODO implement validation
            return gameDefinition;
        }

        private GameDefinition GetGameDefinition(int id, ApplicationUser currentUser)
        {
            GameDefinition gameDefinition = dataContext.GetQueryable<GameDefinition>(currentUser)
                .Where(gameDef => gameDef.Id == id)
                .FirstOrDefault();
            return gameDefinition;
        }

        private IList<PlayedGame> AddPlayedGamesToTheGameDefinition(int numberOfPlayedGamesToRetrieve, ApplicationUser currentUser, GameDefinition gameDefinition)
        {
            IList<PlayedGame> playedGames = dataContext.GetQueryable<PlayedGame>(currentUser)
                .Where(playedGame => playedGame.GameDefinitionId == gameDefinition.Id)
                .Take(numberOfPlayedGamesToRetrieve)
                .ToList();
            gameDefinition.PlayedGames = playedGames;
            return playedGames;
        }

        private IList<int> AddPlayerGameResultsToEachPlayedGame(ApplicationUser currentUser, IList<PlayedGame> playedGames)
        {
            List<int> playedGameIds = (from playedGame in playedGames
                                       select playedGame.Id).ToList();

            IList<PlayerGameResult> playerGameResults = dataContext.GetQueryable<PlayerGameResult>(currentUser)
                .Where(playerGameResult => playedGameIds.Contains(playerGameResult.PlayedGameId))
                .ToList();

            HashSet<int> distinctPlayerIds = new HashSet<int>();

            foreach (PlayedGame playedGame in playedGames)
            {
                playedGame.PlayerGameResults = (from playerGameResult in playerGameResults
                                                where playerGameResult.PlayedGameId == playedGame.Id
                                                select playerGameResult).ToList();

                ExtractDistinctListOfPlayerIds(distinctPlayerIds, playedGame);
            }

            return distinctPlayerIds.ToList();
        }

        private static void ExtractDistinctListOfPlayerIds(HashSet<int> distinctPlayerIds, PlayedGame playedGame)
        {
            foreach (PlayerGameResult playerGameResult in playedGame.PlayerGameResults)
            {
                if (!distinctPlayerIds.Contains(playerGameResult.PlayerId))
                {
                    distinctPlayerIds.Add(playerGameResult.PlayerId);
                }
            }
        }

        private void AddPlayersToPlayerGameResults(ApplicationUser currentUser, IList<PlayedGame> playedGames, IList<int> distinctPlayerIds)
        {
            IList<Player> players = dataContext.GetQueryable<Player>(currentUser)
                .Where(player => distinctPlayerIds.Contains(player.Id))
                .ToList();

            foreach (PlayedGame playedGame in playedGames)
            {
                foreach (PlayerGameResult playerGameResult in playedGame.PlayerGameResults)
                {
                    playerGameResult.Player = players.First(player => player.Id == playerGameResult.PlayerId);
                }
            }
        }
    }
}
