﻿using BusinessLogic.Models;
using BusinessLogic.Models.Players;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Models.Players;
using UI.Transformations;
using UI.Transformations.Player;

namespace UI.Tests.UnitTests.TransformationsTests.PlayerTests.PlayerDetailsViewModelBuilderImplTests
{
    [TestFixture]
    public class BuildTests
    {
        private PlayerDetails playerDetails;
        private PlayerDetailsViewModel playerDetailsViewModel;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            GameDefinition gameDefinition1 = new GameDefinition()
            {
                Name = "test game 1",
                Id = 1
            };
            PlayedGame playedGame1 = new PlayedGame()
            {
                Id = 1,
                GameDefinition = gameDefinition1
            };
            GameDefinition gameDefinition2 = new GameDefinition()
            {
                Name = "test game 2",
                Id = 2
            };
            PlayedGame playedGame2 = new PlayedGame()
            {
                Id = 2,
                GameDefinition = gameDefinition2
            };
            List<PlayerGameResult> playerGameResults = new List<PlayerGameResult>()
            {
                new PlayerGameResult(){ PlayedGameId = 12, PlayedGame = playedGame1 },
                new PlayerGameResult(){ PlayedGameId = 13, PlayedGame = playedGame2 }
            };

            playerDetails = new PlayerDetails()
            {
                Id = 134,
                Active = true,
                Name = "Skipper",
                PlayerGameResults = playerGameResults
            };

            GameResultViewModelBuilder relatedEntityBuilder
                = MockRepository.GenerateMock<GameResultViewModelBuilder>();
            relatedEntityBuilder.Expect(build => build.Build(playerGameResults[0]))
                    .Repeat
                    .Once()
                    .Return(new Models.PlayedGame.GameResultViewModel() { PlayedGameId = playerGameResults[0].PlayedGameId });
            relatedEntityBuilder.Expect(build => build.Build(playerGameResults[1]))
                    .Repeat
                    .Once()
                    .Return(new Models.PlayedGame.GameResultViewModel() { PlayedGameId = playerGameResults[1].PlayedGameId });
            PlayerDetailsViewModelBuilderImpl builder = new PlayerDetailsViewModelBuilderImpl(relatedEntityBuilder);

            playerDetailsViewModel = builder.Build(playerDetails);
        }

        [Test]
        public void PlayerDetailsCannotBeNull()
        {
            PlayerDetailsViewModelBuilderImpl builder = new PlayerDetailsViewModelBuilderImpl(null);

            var exception = Assert.Throws<ArgumentNullException>(() =>
                    builder.Build(null));

            Assert.AreEqual("playerDetails", exception.ParamName);
        }

        [Test]
        public void ItRequiresPlayerGameResults()
        {
            PlayerDetailsViewModelBuilderImpl builder = new PlayerDetailsViewModelBuilderImpl(null);

            var exception = Assert.Throws<ArgumentException>(() =>
                    builder.Build(new PlayerDetails()));

            Assert.AreEqual(PlayerDetailsViewModelBuilderImpl.EXCEPTION_PLAYER_GAME_RESULTS_CANNOT_BE_NULL, exception.Message);
        }

        [Test]
        public void ItCopiesThePlayerId()
        {
            Assert.AreEqual(playerDetails.Id, playerDetailsViewModel.PlayerId);
        }

        [Test]
        public void ItCopiesThePlayerName()
        {
            Assert.AreEqual(playerDetails.Name, playerDetailsViewModel.PlayerName);
        }

        [Test]
        public void ItCopiesTheActiveFlag()
        {
            Assert.AreEqual(playerDetails.Active, playerDetailsViewModel.Active);
        }

        [Test]
        public void ItPopulatesThePlayerGameSummaries()
        {
            int numberOfPlayerGameResults = playerDetails.PlayerGameResults.Count();
            int expectedPlayedGameId;
            int actualPlayedGameId;
            for(int i = 0; i < numberOfPlayerGameResults; i++)
            {
                expectedPlayedGameId = playerDetails.PlayerGameResults[i].PlayedGameId;
                actualPlayedGameId = playerDetailsViewModel.PlayerGameResultDetails[i].PlayedGameId;
                Assert.AreEqual(expectedPlayedGameId, actualPlayedGameId);
            }
        }
    }
}
