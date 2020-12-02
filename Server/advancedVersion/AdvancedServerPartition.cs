using Grpc.Core;
using Server.utils;
using ServerDomain;
using System;
using System.Collections.Generic;
using System.Threading;
using static AdvancedServerObjectInfo;
using static ServerDomain.AdvancedServerStorage;

public class AdvancedServerPartition : IPartition {
    private string _masterUrl;
    private string _id;
    private readonly int _masterTimeout = 200;
    private readonly int _slaveMaxTimeout = 4000;
    private readonly int _slaveMinTimeout = 2500;
    private bool _timeoutReset = true;
    private AdvancedServerStorage _storage;
    public List<ServerInfo> Servers { get; }

    public Dictionary<string, AdvancedServerObjectInfo> Objects { get; }

    private bool _isMaster;

    public bool IsMaster {
        get => _isMaster;
        set {
            _isMaster = value;
            if (_isMaster) SetMasterTimeout();
        }
    }

    //needs lock mechanism
    private bool _hasVote = true;


    public AdvancedServerPartition(string id, string masterUrl,
        AdvancedServerStorage advancedServerStorage) {
        _id = id;
        _masterUrl = masterUrl;
        Objects = new Dictionary<string, AdvancedServerObjectInfo>();
        Servers = new List<ServerInfo>();
        IsMaster = false;
        _storage = advancedServerStorage;
    }

    private void SetMasterTimeout() {
        Console.WriteLine("Setting master timeout");
        var t = new Thread(() => Heartbeat());
        t.Start();
    }

    public void SetSlaveTimeout() {
        Console.WriteLine("Setting slave timeout");
        var t = new Thread(() => SlaveTimeout());
        t.Start();
    }

    public void ResetTimeout() {
        _timeoutReset = true;
    }

    public int GetRandomSlaveTimeout() {
        var r = new Random();
        //return r.Next(_slaveMinTimeout, _slaveMaxTimeout);
        var val = r.Next(_slaveMinTimeout, _slaveMaxTimeout);
        return val;
    }

    public void SlaveTimeout() {
        while (!IsMaster) {
            Thread.Sleep(GetRandomSlaveTimeout());

            if (!_timeoutReset) break;

            _timeoutReset = false;
        }

        //_advancedServerStorage.IsMasterInAnyPartition();
        //becomes candidate
        if (_hasVote) {
            BecomeCandidate();
        }
        else {
            _hasVote = true;
            SlaveTimeout();
        }
    }

    public void BecomeCandidate() {
        Console.WriteLine("I am now a candidate of partition: " + _id);

        _hasVote = false;
        Console.WriteLine("My vote is NOT AVAILABLE!");
        var votes = 1;
        var toRemove = new List<ServerInfo>();

        foreach (var server in Servers)
            try {
                Console.WriteLine("Asking vote from: " + server.ServerId);
                var response = server.ServerChannel.AskVote(new VoteRequest {PartitionId = _id});
    
                if (response.Res) votes++;
                if (votes >= Math.Ceiling(Servers.Count / 2f) + 1) {
                    BecomeMaster();
                    return;
                }

            }
            catch (RpcException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                                  ") OFFICIALLY CRASHED");
                toRemove.Add(server);
                Console.WriteLine("Removed : " + server.ServerId);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


        foreach (var server in toRemove) {
            Servers.Remove(server);
        }

        if (votes >= Math.Ceiling(Servers.Count / 2f) + 1) {
            BecomeMaster();
            return;
        }

        _hasVote = true;
        Console.WriteLine("My vote is AVAILABLE!");

        _timeoutReset = false;
        Thread.Sleep(GetRandomSlaveTimeout());
        if (_timeoutReset) SetSlaveTimeout();
        // Someone turned to master
        else //_hasVote = true;
            BecomeCandidate();
    }

    public void BecomeMaster() {
        Console.WriteLine("I am now the master of partition: " + _id);

        _masterUrl = _storage.ServerUrl;

        var toRemove = new List<ServerExtraInfo>();

        foreach (var server in _storage.GetServersNotFromPartition(_id))
            try {
                server.ServerChannel.InformLeader(new InformLeaderRequest {PartitionId = _id, MasterUrl = _masterUrl});
            }
            catch (RpcException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                                  ") OFFICIALLY CRASHED");
                toRemove.Add(server);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        foreach (var server in toRemove) {
            _storage.Servers.Remove(server);
            for (var i = 0; i < Servers.Count; i++)
                if (Servers[i].ServerId.Equals(server.ServerId)) {
                    Servers.RemoveAt(i);
                    i--;
                }
        }

        var objectInfo = new List<ObjectInfo>();

        foreach (var obj in Objects) {
            var objInfo = obj.Value.Read();
            objectInfo.Add(new ObjectInfo {
                ObjectId = obj.Key,
                ObjectValue = objInfo.value,
                Timestamp = objInfo.timestampCounter
            });
        }

        var request = new InformLeaderPartitionRequest {
            PartitionId = _id,
            NewMasterUrl = _masterUrl,
            //ServerId = _storage.ServerId,
            ObjectInfo = {objectInfo}
        };

        var objectInfos = new List<ObjectInfo>();
        var toRemovePart = new List<ServerInfo>();

        foreach (var server in Servers)
            try {
                var response = server.ServerChannel.InformLeaderPartition(request);
                objectInfos.AddRange(response.ObjectInfo);
            }
            catch (RpcException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                                  ") OFFICIALLY CRASHED");
                toRemovePart.Add(server);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        foreach (var objInfo in objectInfos) Objects[objInfo.ObjectId].Write(objInfo.ObjectValue, objInfo.Timestamp);

        var finishRequest = new FinishLeaderTransitionRequest {ObjectInfo = {objectInfos}};
        foreach (var server in Servers)
        foreach (var objInfo in objectInfos)
            try {
                server.ServerChannel.FinishLeaderTransition(finishRequest);
            }
            catch (RpcException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                                  ") OFFICIALLY DIED");
                toRemovePart.Add(server);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        foreach (var server in toRemovePart) {
            Servers.Remove(server);
        }

        IsMaster = true;
    }

    public void Heartbeat() {
        while (_isMaster) {
            Thread.Sleep(_masterTimeout);

            var toRemove = new List<ServerInfo>();
            foreach (var server in Servers)
                try {
                    Console.WriteLine("[SENDING] Heartbeat. Heartbeat for Server:" + server.ServerId);
                    //heartbeat should be async
                    server.ServerChannel.HeartbeatAsync(new HeartbeatRequest {PartitionId = _id, Sending = _masterUrl});
                }
                catch (RpcException e) {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + server.ServerId +
                                      ") OFFICIALLY CRASHED");
                    toRemove.Add(server);
                }
                catch (Exception e) {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

            foreach (var server in toRemove) Servers.Remove(server);
        }
    }

    public string GetMasterUrl() {
        return _masterUrl;
    }

    public void SetMasterUrl(string url) {
        _masterUrl = url;
    }

    public ObjectVal Read(string objKey) {
        var objectInfo = Objects[objKey];
        return objectInfo.Read();
    }

    public void Write(string objKey, string objValue, int timestamp = -1) {
        if (IsMaster) {
            WriteMaster(objKey, objValue);
            return;
        }

        WriteSlave(objKey, objValue, timestamp);
    }

    public int WriteMaster(string objKey, string objValue) {
        AdvancedServerObjectInfo objectInfo;

        lock (Objects) {
            if (!Objects.TryGetValue(objKey, out objectInfo))
                Objects.Add(objKey, objectInfo = new AdvancedServerObjectInfo("NA"));
        }

        var timeStamp = Objects[objKey].WriteNext(objValue);
        var request = new WriteSlaveRequest {
            PartitionId = _id,
            ObjectId = objKey,
            ObjectValue = objValue,
            Timestamp = timeStamp
        };

        var toRemove = new List<ServerInfo>();
        foreach (var slave in Servers)
            try {
                slave.ServerChannel.WriteSlaveAsync(request);
            }
            catch (RpcException e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("ONE OF THE SERVERS I WAS TALKING TO (" + slave.ServerId +
                                  ") OFFICIALLY DIED");
                toRemove.Add(slave);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        foreach (var slave in toRemove) Servers.Remove(slave);

        return timeStamp;
    }

    public void WriteSlave(string objKey, string objValue, int timestamp) {
        AdvancedServerObjectInfo objectInfo = null;
        lock (Objects) {
            if (Objects.ContainsKey(objKey)) {
                objectInfo = Objects[objKey];
            }
            else {
                Objects.Add(objKey, objectInfo = new AdvancedServerObjectInfo("NA"));
            }
        }
        objectInfo.Write(objValue, timestamp); // we can do it this way because objects are never removedpeç
    }

    public bool AskVote() {
        Console.WriteLine("I've been asked to vote");
        if (_hasVote) {
            Console.WriteLine("Does it have any votes? " + _hasVote + "for Partition with id: " + _id);
            _hasVote = false;
            return true;
        }
        Console.WriteLine("I don't have any votes");
        return false;
    }

    public List<ObjectInfo> InformLeaderPartition(string newMasterUrl, List<ObjectInfo> objectInfos) {
        _timeoutReset = true;
        _masterUrl = newMasterUrl;
        IsMaster = false;
        var result = new List<ObjectInfo>();
        AdvancedServerObjectInfo objectInfo = null;

        foreach (var objInfo in objectInfos) {
            if (!Objects.ContainsKey(objInfo.ObjectId)) {
                Console.Write("InformLeaderPartition: I DONT have the object key");
                Objects.Add(objInfo.ObjectId, objectInfo = new AdvancedServerObjectInfo("NA"));
            }

            var objVal = Objects[objInfo.ObjectId].Read();
            if (objVal.timestampCounter == objInfo.Timestamp) continue;
            if (objVal.timestampCounter > objInfo.Timestamp)
                result.Add(new ObjectInfo {
                    ObjectId = objInfo.ObjectId,
                    ObjectValue = objVal.value,
                    Timestamp = objVal.timestampCounter
                });
            else
                Objects[objInfo.ObjectId].Write(objInfo.ObjectValue, objInfo.Timestamp);
        }

        foreach (var myObj in Objects) {
            var newLeaderHasThisObj = false;
            foreach (var objInfo in objectInfos)
                if (objInfo.ObjectId.Equals(myObj.Key)) {
                    newLeaderHasThisObj = true;
                    break;
                }

            if (newLeaderHasThisObj) continue;

            result.Add(new ObjectInfo {
                ObjectId = myObj.Key,
                ObjectValue = myObj.Value.Read().value,
                Timestamp = myObj.Value.Read().timestampCounter
            });
        }

        return result;
    }


    public class ServerInfo {
        public ServerInfo(string serverId, AdvancedSlaveService.AdvancedSlaveServiceClient serverChannel) {
            ServerId = serverId;
            ServerChannel = serverChannel;
        }

        public string ServerId { get; }
        public AdvancedSlaveService.AdvancedSlaveServiceClient ServerChannel { get; }
    }
}