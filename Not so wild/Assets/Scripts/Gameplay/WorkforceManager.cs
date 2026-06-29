using NotSoWild.Core;
using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class WorkforceManager : MonoBehaviour
    {
        ResidentManager _residents;

        public void Initialize(ResidentManager residents)
        {
            _residents = residents;
        }

        public bool TryAssignWorker(TownState state, PlacedBuildingRecord building, ResidentRecord worker)
        {
            if (state == null || building == null || worker == null)
            {
                return false;
            }

            if (!WorkforceHelper.IsIdle(worker) ||
                building.Worker != null ||
                !WorkforceHelper.CanWorkAt(worker, building.Definition))
            {
                return false;
            }

            building.Worker = worker;
            worker.WorkState = ResidentWorkState.Working;
            worker.AssignedBuilding = building;
            worker.ConstructionSite = null;
            building.View?.SetStaffed(true);
            _residents?.SyncAgentWorkState(worker);

            string workerName = worker.Definition != null ? worker.Definition.DisplayName : "Worker";
            string buildingName = building.Definition != null ? building.Definition.DisplayName : "building";
            state.AddLog($"{workerName} now works at {buildingName}.");
            return true;
        }

        public bool TryUnassignWorker(TownState state, ResidentRecord worker)
        {
            if (state == null || worker == null || worker.WorkState != ResidentWorkState.Working)
            {
                return false;
            }

            var building = worker.AssignedBuilding;
            if (building == null)
            {
                worker.WorkState = ResidentWorkState.Idle;
                _residents?.SyncAgentWorkState(worker);
                return true;
            }

            building.Worker = null;
            building.View?.SetStaffed(false);
            worker.AssignedBuilding = null;
            worker.WorkState = ResidentWorkState.Idle;
            _residents?.SyncAgentWorkState(worker);

            string workerName = worker.Definition != null ? worker.Definition.DisplayName : "Worker";
            string buildingName = building.Definition != null ? building.Definition.DisplayName : "building";
            state.AddLog($"{workerName} left {buildingName}. It is now inactive.");
            return true;
        }

        public bool TryUnassignBuilding(TownState state, PlacedBuildingRecord building)
        {
            if (building?.Worker == null)
            {
                return false;
            }

            return TryUnassignWorker(state, building.Worker);
        }
    }
}
