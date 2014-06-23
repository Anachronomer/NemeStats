﻿using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UI.Models.PlayedGame;
using UI.Transformations;

namespace UI.Transformations
{
    public class PlayedGameDetailsViewModelBuilderImpl : PlayedGameDetailsViewModelBuilder
    {
        internal const string EXCEPTION_GAME_DEFINITION_CANNOT_BE_NULL = "PlayedGame.GameDefintion cannot be null.";
        internal const string EXCEPTION_PLAYER_GAME_RESULTS_CANNOT_BE_NULL = "PlayedGame.PlayerGameResults cannot be null.";

        private GameResultViewModelBuilder playerResultBuilder;

        //TODO is it OK to do this to satisfy MVC's need for parameterless constructors?
        public PlayedGameDetailsViewModelBuilderImpl()
        {
            playerResultBuilder = new GameResultViewModelBuilderImpl();
        }

        public PlayedGameDetailsViewModelBuilderImpl(GameResultViewModelBuilder playerGameResultBuilder)
        {
            playerResultBuilder = playerGameResultBuilder;
        }

        public PlayedGameDetailsViewModel Build(PlayedGame playedGame)
        {
            if(playedGame == null)
            {
                throw new ArgumentNullException("playedGame");
            }

            if(playedGame.GameDefinition == null)
            {
                throw new ArgumentException(EXCEPTION_GAME_DEFINITION_CANNOT_BE_NULL);
            }

            if (playedGame.PlayerGameResults == null)
            {
                throw new ArgumentException(EXCEPTION_PLAYER_GAME_RESULTS_CANNOT_BE_NULL);
            }
            
            PlayedGameDetailsViewModel summary = new PlayedGameDetailsViewModel();
            summary.GameDefinitionName = playedGame.GameDefinition.Name;
            summary.GameDefinitionId = playedGame.GameDefinitionId;
            summary.PlayedGameId = playedGame.Id;
            summary.DatePlayed = playedGame.DatePlayed;
            summary.PlayerResults = new List<GameResultViewModel>();
            
            foreach(PlayerGameResult playerGameResult in playedGame.PlayerGameResults)
            {
                summary.PlayerResults.Add(playerResultBuilder.Build(playerGameResult));
            }

            return summary;
        }
    }
}