using System;
using Verse;
using RimWorld.BaseGen;
using RimWorld;
using Verse.AI.Group;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CthulhuFactions
{
    public class SymbolResolver_AgencyInterrogations : SymbolResolverAgency
    {
        public virtual bool HorizontalHallway
        {
            get { return false; }
        }

        public override void PassingParameters(ResolveParams rp)
        {

            Map map = BaseGen.globalSettings.map;
            _ = new CellRect();
            _ = new List<IntVec3>();
            IEnumerable<CellRect> splitRooms = Utility.FourWaySplit(rp.rect, out CellRect hallway, out IEnumerable<IntVec3> doorLocs, 2, HorizontalHallway);


            ResolveParams hallwayParams = rp;
            hallwayParams.rect = hallway;

            foreach (IntVec3 doorLoc in doorLocs)
            {
                ResolveParams resolveParamsDoor = rp;
                resolveParamsDoor.rect = CellRect.SingleCell(doorLoc);
                resolveParamsDoor.singleThingDef = CthulhuFactionsDefOf.ROM_TemporaryDoorMarker;
                BaseGen.symbolStack.Push("thing", resolveParamsDoor);
            }

            foreach (CellRect current in splitRooms.InRandomOrder<CellRect>())
            {
                ResolveParams resolveParamsTemp = rp;
                resolveParamsTemp.rect = current;
                //BaseGen.symbolStack.Push("agencyDoorsNSEW", resolveParamsTemp);
                BaseGen.symbolStack.Push("roof", resolveParamsTemp);
                BaseGen.symbolStack.Push("edgeWalls", resolveParamsTemp);
                BaseGen.symbolStack.Push("agencyFillWithTablesAndChairs", resolveParamsTemp);
                UnfogRoomCenter(current.CenterCell);
            }

            Utility.RectReport(rp.rect);

            //Bring in Standard Pawns
            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(rp.faction, new LordJob_DefendBase(rp.faction, rp.rect.CenterCell), map, null);

            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect.ExpandedBy(1);
            resolveParams.faction = rp.faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = PawnGroupKindDefOf.Settlement;
            float points = 500;
            resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms
            {
                tile = map.Tile,
                faction = rp.faction,
                points = points
            };

            BaseGen.symbolStack.Push("pawnGroup", resolveParams);

        }

        private void UnfogRoomCenter(IntVec3 centerCell)
        {
            if (Current.ProgramState != ProgramState.MapInitializing)
            {
                return;
            }
            List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
            rootsToUnfog.Add(centerCell);
        }
    }
}
