﻿using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UI.Models.PlayedGame;

namespace UI.Transformations
{
    public interface GameResultViewModelBuilder
    {
        GameResultViewModel Build(PlayerGameResult playerGameResult);
    }
}