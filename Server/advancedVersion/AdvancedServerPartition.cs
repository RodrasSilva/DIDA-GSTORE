using Grpc.Core;
using Server.utils;
using ServerDomain;
using System;
using System.Collections.Generic;
using System.Threading;
using static AdvancedServerObjectInfo;

public class AdvancedServerPartition : IPartition{
    private readonly string _masterUrl;
    private string _id;
    private readonly int _masterTimeout = 200;
    private readonly int _slaveMaxTimeout = 1000;
    private readonly int _slaveMinTimeout = 500;
    private bool _timeoutReset = true;
    private AdvancedServerStorage _advancedServerStorage;
    public List<ServerInfo> Servers { get; }

    public Dictionary<string, AdvancedServerObjectInfo> Objects { get; }

    private bool _isMaster;
    public bool IsMaster
    {
        get { return _isMaster; }
        set { _isMaster = value; SetMasterTimeout(); }
    }

    private bool _hasVote = true;



    public AdvancedServerPartition(string id, string masterUrl,
        AdvancedServerStorage advancedServerStorage)
    {
        _id = id;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, AdvancedServerObjectInfo>();
        Servers = new List<ServerInfo>();
        IsMaster = false;
        _advancedServerStorage = advancedServerStorage;
    }

    private void SetMasterTimeout()
    {
        Thread t = new Thread( () => Heartbeat());
    }

    public void SetSlaveTimeout()
    {
        Thread t = new Thread(() => SlaveTimeout());
    }

    public void ResetTimeout()
    {
        _timeoutReset = true;
    }

    public int GetRandomSlaveTimeout()
    {
        Random r = new Random();
        return r.Next(_slaveMinTimeout, _slaveMaxTimeout);
    }
    public void SlaveTimeout()
    {
        while (!IsMaster)
        {
            Thread.Sleep(GetRandomSlaveTimeout());
            if(!_timeoutReset)
            {
                break;
            }
            _timeoutReset = false;
        }

        //_advancedServerStorage.IsMasterInAnyPartition();
        //becomes candidate
        if (!_hasVote)
        {
            BecomeCandidate();
        }
    }

    public void BecomeCandidate()
    {
        _hasVote = false;
        int votes = 1;
        foreach (var server in Servers)
        {
            VoteResponse response = server.ServerChannel.AskVote(new VoteRequest());
            if (response.Res) votes++;
            if (votes >= Servers.Count / 2 + 1)
            {
                BecomeMaster();
            }
        }
        _timeoutReset = false;
        Thread.Sleep(GetRandomSlaveTimeout());
        if(_timeoutReset)
        {
            SetSlaveTimeout();
            // Someone turned to master
            // Timestamps
            // Objects
        }
        else
        {
            _hasVote = true;
            BecomeCandidate();
        }
    }

    public void BecomeMaster()
    {

    }

    public void Heartbeat()
    {
        while (_isMaster)
        {
            List<ServerInfo> toRemove = new List<ServerInfo>();
            foreach (var server in Servers)
            {
                try
                {
                    server.ServerChannel.HeartbeatAsync(new HeartbeatRequest());
                }
                catch (RpcException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                        ") OFFICIALLY DIED");
                    toRemove.Add(server);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            foreach (var server in toRemove)
            {
                Servers.Remove(server);
            }

            Thread.Sleep(_masterTimeout);
        }
    }

    public string GetMasterUrl(){
        return _masterUrl;
    }

    public ObjectVal ReadAdvanced(string objKey, 
        string clientObjectValue, int clientTimestamp)
    {
        var objectInfo = Objects[objKey];
        objectInfo._lock.Set();
        try
        {
            return objectInfo.Read(clientObjectValue, clientTimestamp);
        }
        finally
        {
            objectInfo._lock.Reset();
        }
    }

    public void Write(string objKey, string objValue, int timestamp = -1){
        if (IsMaster) WriteMaster(objKey, objValue);
        WriteSlave(objKey, objValue, timestamp);
    }

    public void WriteMaster(string objKey, string objValue)
    {
        AdvancedServerObjectInfo objectInfo;

        lock (Objects)
        {
            if (!Objects.TryGetValue(objKey, out objectInfo))
            {
                Objects.Add(objKey, objectInfo = new AdvancedServerObjectInfo("NA"));
            }
        }
        var timeStamp = Objects[objKey].WriteNext(objValue);
        var request = new WriteSlaveRequest {
            PartitionId = _id,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };

        List<ServerInfo> toRemove = new List<ServerInfo>();
        foreach (var slave in Servers)
        {
            try
            {
                slave.ServerChannel.WriteSlaveAsync(request);
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + slave.ServerId +
                    ") OFFICIALLY DIED");
                toRemove.Add(slave);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        foreach(var slave in toRemove)
        {
            Servers.Remove(slave);
        }
    }

    public void WriteSlave(string objKey, string objValue, int timestamp){
        Objects[objKey].Write(objValue, timestamp);
    }


    public class ServerInfo
    {
        public ServerInfo(string serverId, AdvancedSlaveService.AdvancedSlaveServiceClient serverChannel)
        {
            ServerId = serverId;
            ServerChannel = serverChannel;
        }

        public string ServerId { get; }
        public AdvancedSlaveService.AdvancedSlaveServiceClient ServerChannel { get; }
    }
}