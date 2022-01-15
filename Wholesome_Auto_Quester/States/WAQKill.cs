﻿using robotManager.FiniteStateMachine;
using FlXProfiles;
using Wholesome_Auto_Quester.Bot;
using Wholesome_Auto_Quester.Helpers;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace Wholesome_Auto_Quester.States
{
    class WAQKill : State
    {
        public override string DisplayName { get; set; } = "Kill creature [SmoothMove - Q]";

        public override bool NeedToRun
        {
            get
            {
                if (!Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause 
                    || !ObjectManager.Me.IsValid)
                    return false;

                if (WAQTasks.TaskInProgress?.TaskType == TaskType.Kill || WAQTasks.TaskInProgress?.TaskType == TaskType.KillAndLoot)
                {
                    DisplayName = $"Kill {WAQTasks.TaskInProgress.TargetName} for {WAQTasks.TaskInProgress.QuestTitle} [SmoothMove - Q]";
                    return true;
                }

                return false;
            }
        }

        public override void Run()
        {
            WAQTask task = WAQTasks.TaskInProgress;
            WoWObject gameObject = WAQTasks.TaskInProgressWoWObject;

            if (gameObject != null)
            {
                if (gameObject.Type != WoWObjectType.Unit) {
                    Logger.LogError($"Expected a WoWUnit for Kill but got {gameObject.Type} instead.");
                    return;
                }

                WoWUnit killTarget = (WoWUnit)gameObject;

                ToolBox.CheckSpotAround(killTarget);

                if (!wManager.wManagerSetting.IsBlackListed(killTarget.Guid))
                {
                    Logger.Log($"Unit found - Fighting {killTarget.Name}");
                    MoveHelper.StopCurrentMovementThread();
                    Fight.StartFight(killTarget.Guid);
                    if (killTarget.IsDead && task.TaskType == TaskType.Kill
                        || killTarget.IsDead && !killTarget.IsLootable && task.TaskType == TaskType.KillAndLoot)
                    {
                        task.PutTaskOnTimeout("Completed");
                        WAQTasks.UpdateTasks();
                    }
                }
            }
            else
            {
                if (!MoveHelper.IsMovementThreadRunning ||
                    MoveHelper.CurrentMovementTarget?.DistanceTo(task.Location) > 8) {
                    
                    Logger.Log($"Moving to Hotspot for {task.QuestTitle} (Kill).");
                    MoveHelper.StartGoToThread(task.Location, randomizeEnd: 8f);
                }
                if (task.GetDistance <= 12f) {
                    task.PutTaskOnTimeout("No creature to kill in sight");
                    // MoveHelper.StopAllMove();
                } else if (ToolBox.DangerousEnemiesAtLocation(task.Location) && WAQTasks.TasksPile.FindAll(t => t.TargetEntry == task.TargetEntry).Count > 1) {
                    task.PutTaskOnTimeout("Dangerous mobs in the area");
                }
            }
        }
    }
}
