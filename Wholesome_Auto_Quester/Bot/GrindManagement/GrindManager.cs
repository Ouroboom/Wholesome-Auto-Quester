﻿using System.Collections.Generic;
using System.Linq;
using Wholesome_Auto_Quester.Bot.TaskManagement.Tasks;
using Wholesome_Auto_Quester.Bot.TravelManagement;
using Wholesome_Auto_Quester.Database;
using Wholesome_Auto_Quester.Database.Models;
using Wholesome_Auto_Quester.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace Wholesome_Auto_Quester.Bot.GrindManagement
{
    public class GrindManager : IGrindManager
    {
        private readonly List<IWAQTask> _grindTasks = new List<IWAQTask>();

        public GrindManager()
        {
            Initialize();
        }

        private void RecordGrindTasksFromDB()
        {
            _grindTasks.Clear();
            DB _database = new DB();
            List<ModelCreatureTemplate> ctToGrind = _database.QueryCreatureTemplatesToGrind();
            _database.Dispose();

            ctToGrind.RemoveAll(ct =>
                ct.Creatures.Any(c =>
                    !ContinentHelper.PointIsOnMyContinent(c.GetSpawnPosition, c.map) && !WholesomeAQSettings.CurrentSetting.ContinentTravel)
                || ct.IsFriendly
                || ct.faction == 188);
            Logger.Log($"Found {ctToGrind.Count} templates to grind");
            foreach (ModelCreatureTemplate template in ctToGrind)
            {
                foreach (ModelCreature creature in template.Creatures)
                {
                    _grindTasks.Add(new WAQTaskGrind(template, creature));
                }
            }
            _grindTasks.RemoveAll(task => task.WorldMapArea == null);
        }

        public void Initialize()
        {
            RecordGrindTasksFromDB();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += LuaEventHandler;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= LuaEventHandler;
        }

        private void LuaEventHandler(string eventid, List<string> args)
        {
            if (eventid == "PLAYER_LEVEL_UP" && ObjectManager.Me.Level < WholesomeAQSettings.CurrentSetting.StopAtLevel)
            {
                RecordGrindTasksFromDB();
            }
        }

        public List<IWAQTask> GetGrindTasks() => _grindTasks.FindAll(task => task.IsValid);
    }
}
