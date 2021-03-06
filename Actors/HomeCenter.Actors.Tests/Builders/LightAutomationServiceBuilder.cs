﻿using HomeCenter.Services.Configuration.DTO;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
{

    internal class LightAutomationServiceBuilder
    {
        private TimeSpan? _confusionResolutionTime;
        private string _workingTime;

        private readonly Dictionary<string, RoomBuilder> _rooms = new Dictionary<string, RoomBuilder>();

        public RoomBuilder this[string room]
        {
            get { return _rooms[room]; }
        }

        public LightAutomationServiceBuilder WithConfusionResolutionTime(TimeSpan confusionResolutionTime)
        {
            _confusionResolutionTime = confusionResolutionTime;
            return this;
        }

        public LightAutomationServiceBuilder WithWorkingTime(string wortkingTime)
        {
            _workingTime = wortkingTime;
            return this;
        }

        public LightAutomationServiceBuilder WithRoom(RoomBuilder room)
        {
            _rooms.Add(room.Name, room);
            return this;
        }


        public ServiceDTO Build()
        {
            var serviceDto = new ServiceDTO
            {
                IsEnabled = true,
                Properties = new Dictionary<string, string>()
            };

            foreach (var room in _rooms.Values)
            {
                AddRoom(serviceDto, room);
            }

            if (_confusionResolutionTime.HasValue)
            {
                serviceDto.Properties.Add(MotionProperties.ConfusionResolutionTime, _confusionResolutionTime.ToString());
            }

            return serviceDto;
        }

        private void AddRoom(ServiceDTO serviceDto, RoomBuilder roomBuilder)
        {
            var area = new AttachedPropertyDTO
            {
                AttachedActor = roomBuilder.Name,
                Properties = new Dictionary<string, string>()
            };

            if (!string.IsNullOrWhiteSpace(_workingTime))
            {
                area.Properties[MotionProperties.WorkingTime] = _workingTime;
            }

            foreach (var property in roomBuilder.Properties)
            {
                area.Properties[property.Key] = property.Value;
            }

            foreach (var detector in roomBuilder.Detectors.Values)
            {
                AddMotionSensor(detector.DetectorName, roomBuilder.Name, detector.Neighbors, serviceDto);
            }


            serviceDto.AreasAttachedProperties.Add(area);
        }

        private void AddMotionSensor(string motionSensor, string area, IEnumerable<string> neighbors, ServiceDTO serviceDto)
        {
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = motionSensor,
                AttachedArea = area,
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", neighbors),
                    [MotionProperties.Lamp] = motionSensor
                }
            });
        }
    }
}