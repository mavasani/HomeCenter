﻿using HomeCenter.Services.MotionService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class RoomService
    {
        private readonly IReadOnlyDictionary<string, Room> _rooms;
        private readonly MotionConfiguration _motionConfiguration;

        public Room this[string uid]
        {
            get { return _rooms[uid]; }
        }

        private Room this[MotionWindow window]
        {
            get { return _rooms[window.Start.Uid]; }
        }

        private Room this[MotionVector vector]
        {
            get { return _rooms[vector.EndPoint]; }
        }

        public RoomService(IEnumerable<Room> rooms, MotionConfiguration motionConfiguration)
        {
            _rooms = rooms.ToDictionary(k => k.Uid, v => v).AsReadOnly();
            _motionConfiguration = motionConfiguration;
        }

        public int NumberOfPersons() => _rooms.Sum(md => md.Value.NumberOfPersons);

        /// <summary>
        /// Check if two point in time can physically be a proper vector
        /// </summary>
        /// <param name="start"></param>
        /// <param name="potencialEnd"></param>
        /// <returns></returns>
        public bool IsProperVector(MotionPoint start, MotionPoint potencialEnd)
        {
            return AreNeighbors(start, potencialEnd) && potencialEnd.IsMovePhisicallyPosible(start, _motionConfiguration.MotionMinDiff);
        }

        public async Task HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0) return;

            var targetRoom = this[motionVectors.First()];
            await targetRoom.HandleVectors(motionVectors);
        }

        public Task MarkMotion(MotionWindow point)
        {
            return this[point].MarkMotion(point.Start.TimeStamp);
        }

        /// <summary>
        /// Evaluates each room state
        /// </summary>
        public async Task UpdateRooms(DateTimeOffset motionTime)
        {
            await _rooms.Values.Select(r => r.EvaluateConfusions(motionTime)).WhenAll();

            await _rooms.Values.Select(r => r.PeriodicUpdate(motionTime)).WhenAll();
        }

        /// <summary>
        /// Check if two points are neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool AreNeighbors(MotionPoint p1, MotionPoint p2) => _rooms[p1.Uid].IsNeighbor(p2.Uid);
    }
}