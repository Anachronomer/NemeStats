﻿using BusinessLogic.DataAccess;
using BusinessLogic.Logic;
using BusinessLogic.Models;
using BusinessLogic.Models.Players;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Tests.IntegrationTests.LogicTests.PlayerRepositoryTests
{
    [TestFixture]
    public class GetPlayerDetailsIntegrationTests : IntegrationTestBase
    {
        private UserContextBuilder userContextBuilder;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            userContextBuilder = new UserContextBuilderImpl();
        }

        [Test]
        public void ItEagerlyFetchesPlayerGameResults()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                PlayerRepository playerRepository = new PlayerRepository(dbContext);

                dbContext.Configuration.LazyLoadingEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;

                PlayerDetails playerDetails = playerRepository.GetPlayerDetails(testPlayer1.Id, 1, testUserContextForUserWithDefaultGamingGroup);
                Assert.NotNull(playerDetails.PlayerGameResults, "Failed to retrieve PlayerGameResults.");
            }
        }

        [Test]
        public void ItEagerlyFetchesPlayedGames()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                dbContext.Configuration.LazyLoadingEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;
                PlayerRepository playerRepository = new PlayerRepository(dbContext);

                PlayerDetails testPlayerDetails = playerRepository.GetPlayerDetails(testPlayer1.Id, 1, testUserContextForUserWithDefaultGamingGroup);

                Assert.NotNull(testPlayerDetails.PlayerGameResults.First().PlayedGame);
            }
        }

        [Test]
        public void ItEagerlyFetchesGameDefinitions()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                dbContext.Configuration.LazyLoadingEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;
                PlayerRepository playerRepository = new PlayerRepository(dbContext);

                PlayerDetails testPlayerDetails = playerRepository.GetPlayerDetails(testPlayer1.Id, 1, testUserContextForUserWithDefaultGamingGroup);

                Assert.NotNull(testPlayerDetails.PlayerGameResults.First().PlayedGame.GameDefinition);
            }
        }

        [Test]
        public void ItSetsPlayerStatistics()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                PlayerDetails playerDetails = new PlayerRepository(dbContext)
                    .GetPlayerDetails(testPlayer1.Id, 1, testUserContextForUserWithDefaultGamingGroup);

                Assert.NotNull(playerDetails.PlayerStats);
            }
        }

        [Test]
        public void ItOnlyGetsTheSpecifiedNumberOfRecentGames()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                int numberOfGamesToRetrieve = 1;

                PlayerDetails playerDetails = new PlayerRepository(dbContext)
                    .GetPlayerDetails(testPlayer1.Id, numberOfGamesToRetrieve, testUserContextForUserWithDefaultGamingGroup);

                Assert.AreEqual(numberOfGamesToRetrieve, playerDetails.PlayerGameResults.Count);
            }
        }

        [Test]
        public void ItOrdersPlayerGameResultsByTheDatePlayedDescending()
        {
            using (NemeStatsDbContext dbContext = new NemeStatsDbContext())
            {
                int numberOfGamesToRetrieve = 3;

                PlayerDetails playerDetails = new PlayerRepository(dbContext)
                    .GetPlayerDetails(testPlayer1.Id, numberOfGamesToRetrieve, testUserContextForUserWithDefaultGamingGroup);
                long lastTicks = long.MaxValue; ;
                Assert.IsTrue(playerDetails.PlayerGameResults.Count == numberOfGamesToRetrieve);
                foreach(PlayerGameResult result in playerDetails.PlayerGameResults)
                {
                    Assert.GreaterOrEqual(lastTicks, result.PlayedGame.DatePlayed.Ticks);

                    lastTicks = result.PlayedGame.DatePlayed.Ticks;
                }
            }
        }
    }
}