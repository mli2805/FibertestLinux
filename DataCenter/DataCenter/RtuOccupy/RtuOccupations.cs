﻿using System.Collections.Concurrent;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public class RtuOccupations
{
    private readonly ILogger<RtuOccupations> _logger;
    private const int TimeoutSec = 100;

    public ConcurrentDictionary<Guid, RtuOccupationState> RtuStates = new();

    public RtuOccupations(ILogger<RtuOccupations> logger)
    {
        _logger = logger;
    }

    // checks and if possible set occupation
    public bool TrySetOccupation(Guid rtuId, RtuOccupation newRtuOccupation, string userName, out RtuOccupationState? state)
    {
        var action = newRtuOccupation == RtuOccupation.None
            ? $@"free RTU {rtuId.First6()}"
            : $@"occupy RTU {rtuId.First6()} for {newRtuOccupation}";
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $@"Client {userName} asked to {action}");
        if (newRtuOccupation == RtuOccupation.None)
        {
            /////////////  it is a CHECK or CLEANUP  //////////////////////
            if (!RtuStates.TryGetValue(rtuId, out state))
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $@"RTU {rtuId.First6()} is free already");
                return true;
            }

            if (state.UserName != userName)
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                    $@"{userName} can't free RTU {rtuId.First6()}, cos it's occupied by {state.UserName}");
                return false;
            }

            if (RtuStates.TryRemove(rtuId, out state))
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $@"RTU {rtuId.First6()} is free now");
                return true;
            }

            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), @"Something went wrong while dictionary cleanup!");
            return false;
        }

        if (!RtuStates.TryGetValue(rtuId, out RtuOccupationState? currentState))
        { ////////// NEW OCCUPATION /////////////////
            state = new RtuOccupationState() { RtuOccupation = RtuOccupation.None };
            if (RtuStates.TryAdd(rtuId,
                    new RtuOccupationState()
                    {
                        // RtuId = rtuId,
                        RtuOccupation = newRtuOccupation,
                        UserName = userName,
                        Expired = DateTime.Now.AddSeconds(TimeoutSec),
                    }))
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                    $@"Applied! RTU {rtuId.First6()} is occupied by {userName} for {newRtuOccupation}");
                return true;
            }
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), @"Something went wrong while dictionary addition!");
            return false;
        }

        state = currentState;
        if (currentState.UserName == userName || currentState.Expired < DateTime.Now)
        {  /////////  REFRESH   //////////////////
            if (RtuStates.TryUpdate(rtuId, new RtuOccupationState()
            {
                // RtuId = rtuId,
                RtuOccupation = newRtuOccupation,
                UserName = userName,
                Expired = DateTime.Now.AddSeconds(TimeoutSec)
            }, state))
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
                    $@"Applied! RTU {rtuId.First6()} is occupied by {userName} for {newRtuOccupation}");
                return true;
            }
            _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), @"Something went wrong while dictionary update!");
            return false;
        }

        ///////// DENY ///////////////
        var cs = $@"(current state is {currentState.RtuOccupation}, expires at {currentState.Expired:HH:mm:ss})";
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(),
            $@"Denied! RTU {rtuId.First6()} is occupied by {currentState.UserName} {cs}");
        return false;
    }
}