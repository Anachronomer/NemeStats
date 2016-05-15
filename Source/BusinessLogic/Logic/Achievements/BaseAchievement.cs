using System.Collections.Generic;
using System.Linq;
using BusinessLogic.DataAccess;
using BusinessLogic.Models.Achievements;

namespace BusinessLogic.Logic.Achievements
{
    public abstract class BaseAchievement : IAchievement
    {
        protected IDataContext DataContext { get; set; }

        protected BaseAchievement(IDataContext dataContext)
        {
            DataContext = dataContext;
        }

        protected AchievementLevel GetLevelAwarded(int count)
        {
            return LevelThresholds.OrderByDescending(l => l.Value)
                .FirstOrDefault(l => l.Value <= count)
                .Key;
        }

        public abstract AchievementId Id { get; }
        public abstract AchievementGroup Group { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string IconClass { get; }
        public abstract Dictionary<AchievementLevel, int> LevelThresholds { get; }
        public abstract AchievementAwarded IsAwardedForThisPlayer(int playerId);
    }
}