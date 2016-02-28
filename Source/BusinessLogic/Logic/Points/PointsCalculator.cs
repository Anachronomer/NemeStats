﻿using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Models;
using BusinessLogic.Models.Games;
using BusinessLogic.Models.PlayedGames;

namespace BusinessLogic.Logic.Points
{
    public class PointsCalculator : IPointsCalculator
    {
        private readonly IWeightTierCalculator _weightTierCalculator;
        private readonly IAverageGameDurationTierCalculator _averageGameDurationTierCalculator;

        public PointsCalculator(IWeightTierCalculator weightTierCalculator, 
            IAverageGameDurationTierCalculator averageGameDurationTierCalculator)
        {
            _weightTierCalculator = weightTierCalculator;
            _averageGameDurationTierCalculator = averageGameDurationTierCalculator;
        }

        public static readonly Dictionary<int, int> FIBONACCI_N_PLUS_2 = new Dictionary<int, int>
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 5},
            {5, 8},
            {6, 13},
            {7, 21},
            {8, 34},
            {9, 55},
            {10, 89},
            {11, 144},
            {12, 233},
            {13, 377},
            {14, 610},
            {15, 987},
            {16, 1000},
            {17, 1000},
            {18, 1000},
            {19, 1000},
            {20, 1000},
            {21, 1000},
            {22, 1000},
            {23, 1000},
            {24, 1000},
            {25, 1000}
        };

        internal const int DEFAULT_POINTS_PER_PLAYER_WHEN_EVERYONE_LOSES = 2;
        internal const int POINTS_PER_PLAYER = 10;
        internal const string EXCEPTION_MESSAGE_DUPLICATE_PLAYER = "Each player can only have one PlayerRank record but one or more players have duplicate PlayerRank records.";
        internal const string EXCEPTION_MESSAGE_CANNOT_EXCEED_MAX_PLAYERS = "There can be no more than 25 players.";

        public Dictionary<int, PointsScorecard> CalculatePoints(IList<PlayerRank> playerRanks, BoardGameGeekGameDefinition bggGameDefinition)
        {
            ValidatePlayerRanks(playerRanks);

            var playerToPoints = AwardBasePoints(playerRanks);

            if (bggGameDefinition?.AverageWeight != null)
            {
                var weightTier = _weightTierCalculator.GetWeightTier(bggGameDefinition.AverageWeight);
                AwardWeightBonusPoints(playerToPoints, weightTier);
            }

            if (bggGameDefinition?.PlayingTime != null)
            {
                var gameDurationTier = _averageGameDurationTierCalculator.GetAverageGameDurationTier(bggGameDefinition.PlayingTime);
                AwardPlayingTimeBonusPoints(playerToPoints, gameDurationTier);
            }

            return playerToPoints;
        }

        private static void ValidatePlayerRanks(IList<PlayerRank> playerRanks)
        {
            if (playerRanks.Count > FIBONACCI_N_PLUS_2.Count)
            {
                throw new ArgumentException(EXCEPTION_MESSAGE_CANNOT_EXCEED_MAX_PLAYERS);
            }

            if (playerRanks.GroupBy(x => x.PlayerId).Count(y => y.Count() > 1) > 0)
            {
                throw new ArgumentException(string.Format(EXCEPTION_MESSAGE_DUPLICATE_PLAYER));
            }
        }

        internal virtual Dictionary<int, PointsScorecard> AwardBasePoints(IList<PlayerRank> playerRanks)
        {
            var playerToPoints = new Dictionary<int, PointsScorecard>(playerRanks.Count);

            if (EveryoneLost(playerRanks))
            {
                GiveEveryoneTwoPoints(playerRanks, playerToPoints);
            }
            else
            {
                CalculatePointsForPlayers(playerRanks, playerToPoints);
            }
            return playerToPoints;
        }

        internal virtual void CalculatePointsForPlayers(IList<PlayerRank> playerRanks, Dictionary<int, PointsScorecard> playerToPoints)
        {
            int totalNumberOfPlayers = playerRanks.Count;
            int totalPointsToAllocate = totalNumberOfPlayers * POINTS_PER_PLAYER;
            decimal denominator = FibonacciSum(totalNumberOfPlayers);

            const int fibonacciOffset = 1;
            int highestRankSlotNotConsumed = 1;

            for (int rank = 1; rank <= totalNumberOfPlayers; rank++)
            {
                List<PlayerRank> playersWithThisRank = playerRanks.Where(x => x.GameRank == rank).ToList();
                int numberOfPlayersWithThisRank = playersWithThisRank.Count;
                if (numberOfPlayersWithThisRank > 0)
                {
                    int finoacciEndIndex = totalNumberOfPlayers + fibonacciOffset - highestRankSlotNotConsumed;
                    int fibonacciStartIndex = finoacciEndIndex + fibonacciOffset - numberOfPlayersWithThisRank;
                    decimal numerator = FibonacciSum(fibonacciStartIndex, finoacciEndIndex) / numberOfPlayersWithThisRank;
                    int points = RoundUpIfNonNegligible(totalPointsToAllocate * (numerator / denominator));

                    foreach (var playerRank in playersWithThisRank)
                    {
                        playerToPoints.Add(playerRank.PlayerId, new PointsScorecard { BasePoints = points });
                    }
                }

                highestRankSlotNotConsumed += numberOfPlayersWithThisRank;
            }
        }

        internal virtual void GiveEveryoneTwoPoints(IList<PlayerRank> playerRanks, Dictionary<int, PointsScorecard> playerToPoints)
        {
            foreach (var playerRank in playerRanks)
            {
                playerToPoints.Add(playerRank.PlayerId,
                                   new PointsScorecard
                                   {
                                       BasePoints = DEFAULT_POINTS_PER_PLAYER_WHEN_EVERYONE_LOSES
                                   });
            }
        }

        private static bool EveryoneLost(IList<PlayerRank> playerRanks)
        {
            return playerRanks.All(x => x.GameRank != 1);
        }

        private static decimal FibonacciSum(int fibonacciStartIndex, int finoacciEndIndex)
        {
            return FIBONACCI_N_PLUS_2.Where(fibonacci => fibonacci.Key >= fibonacciStartIndex
                && fibonacci.Key <= finoacciEndIndex).Sum(x => x.Value);
        }

        private static decimal FibonacciSum(int endIndex)
        {
            return FibonacciSum(1, endIndex);
        }

        private static int RoundUpIfNonNegligible(decimal valueToRound)
        {
            decimal floor = Math.Floor(valueToRound);
            return (int)((valueToRound - floor) > (decimal)0.1 ? floor + 1 : floor);
        }

        internal virtual void AwardWeightBonusPoints(Dictionary<int, PointsScorecard> pointsScorecardDictionary, WeightTierEnum gameWeight)
        {
            decimal multiplier;
            switch (gameWeight)
            {
                case WeightTierEnum.Casual:
                case WeightTierEnum.Unknown:
                    multiplier = 0;
                    break;
                case WeightTierEnum.Easy:
                    multiplier = new decimal(.25);
                    break;
                case WeightTierEnum.Advanced:
                    multiplier = new decimal(.50);
                    break;
                case WeightTierEnum.Challenging:
                    multiplier = new decimal(.75);
                    break;
                case WeightTierEnum.Hardcore:
                    multiplier = new decimal(1.0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameWeight), gameWeight, null);
            }

            foreach (var scorecard in pointsScorecardDictionary)
            {
                var bonusPoints = Math.Round(multiplier * scorecard.Value.BasePoints, MidpointRounding.AwayFromZero);
                scorecard.Value.GameWeightBonusPoints = (int)bonusPoints;
            }
        }

        internal virtual void AwardPlayingTimeBonusPoints(
            Dictionary<int, PointsScorecard> pointsScorecardDictionary, 
            AverageGameDurationTierEnum gameDuration)
        {
            decimal multiplier;
            switch (gameDuration)
            {
                case AverageGameDurationTierEnum.VeryShort:
                case AverageGameDurationTierEnum.Unknown:
                    multiplier = 0;
                    break;
                case AverageGameDurationTierEnum.Short:
                    multiplier = new decimal(.50);
                    break;
                case AverageGameDurationTierEnum.Medium:
                    multiplier = new decimal(1);
                    break;
                case AverageGameDurationTierEnum.Long:
                    multiplier = new decimal(1.5);
                    break;
                case AverageGameDurationTierEnum.VeryLong:
                    multiplier = new decimal(2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameDuration), gameDuration, null);
            }

            foreach (var scorecard in pointsScorecardDictionary)
            {
                var bonusPoints = Math.Round(multiplier * scorecard.Value.BasePoints, MidpointRounding.AwayFromZero);
                scorecard.Value.GameDurationBonusPoints = (int)bonusPoints;
            }
        }
    }
}
