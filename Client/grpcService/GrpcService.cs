using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using Client.model;
using Grpc.Core;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService{
    public class GrpcService {
        public readonly string ObjectNotPresent = "N/A";
        private DIDAService.DIDAServiceClient _client;
        private ClientLogic _clientLogic;
        private string _usedUrl;
        public bool UseBaseVersion { get; }

        private PreviousStorage _previousStorage;
        public GrpcService(string serverIp, int serverPort, ClientLogic clientLogic, bool baseVer){
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _usedUrl = $"http://{serverIp}:{serverPort}";
            _client = BuildClientFromServerUrl(_usedUrl);
            _clientLogic = clientLogic;
            UseBaseVersion = baseVer;
            _previousStorage = new PreviousStorage(this);
        }

        private void ConnectToNewServer(string partitionId)
        {
            //_clientLogic.ServerList[partitionId].Remove(_usedUrl);
            _client = null;
            var aux = _usedUrl.Split("//");
            string url = aux[1];
            foreach (var parId in _clientLogic.ServerList.Keys)
            {
                Console.WriteLine("Removing from partition:" + parId + " .Url: " + url);
                _clientLogic.ServerList[parId].Remove(url);
            }

            if (_clientLogic.ServerList[partitionId].Count == 0)
                throw new Exception("should not happen");

            Random r = new Random();
            _usedUrl = "http://" + _clientLogic.ServerList[partitionId][
                r.Next(0, _clientLogic.ServerList[partitionId].Count)];

            Console.WriteLine(_usedUrl);
            _client = BuildClientFromServerUrl(_usedUrl);
        }

        private void RemoveClientUrl(string url)
        {
            foreach (var parId in _clientLogic.ServerList.Keys)
            {
                _clientLogic.ServerList[parId].Remove(url);
            }
        }

        private static DIDAService.DIDAServiceClient BuildClientFromServerUrl(string serverUrl)
        {
            var channel = GrpcChannel.ForAddress(serverUrl);
            return new DIDAService.DIDAServiceClient(channel);
        }

        private string MapServerIdToUrl(string serverId)
        {
            var request = new ServerUrlRequest { ServerId = serverId };
            return _client.getServerUrl(request).ServerUrl;
        }

        private static ListServerResult MapToListServerResult(ListServerResponseEntity it)
        {
            return new ListServerResult(it.ObjectId, it.ObjectValue, it.IsMaster);
        }

        public void Write(string partitionId, string objectId, string objectValue)
        {
            if (UseBaseVersion)
            {
                WriteBasic(partitionId, objectId, objectValue);
            }
            else
            {
                WriteAdvanced(partitionId, objectId, objectValue);
            }
        }

        private void WriteBasic(string partitionId, string objectId, string objectValue){
            var request = new WriteRequest{PartitionId = partitionId, ObjectId = objectId, ObjectValue = objectValue};
            try{
                var response = _client.write(request);
                //Console.WriteLine("Am i here?");
                switch (response.ResponseCase){
                    case WriteResponse.ResponseOneofCase.ResponseMessage:
                        Console.WriteLine("Write Successful");
                        break;

                    case WriteResponse.ResponseOneofCase.MasterServerUrl:
                        Console.WriteLine($"Write - Changing to server {response.MasterServerUrl.ServerUrl}");
                        
                        _usedUrl = response.MasterServerUrl.ServerUrl;
                        _client = BuildClientFromServerUrl(response.MasterServerUrl.ServerUrl);
                        
                        WriteBasic(partitionId, objectId, objectValue);
                        break;

                    case WriteResponse.ResponseOneofCase.None:
                        Console.WriteLine("Unexpected Error");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (RpcException e) 
            {
                ConnectToNewServer(partitionId);
                Write(partitionId, objectId, objectValue);
                return;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Writing");

                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        private void WriteAdvanced(string partitionId, string objectId, string objectValue)
        {
            var request = new WriteAdvancedRequest { PartitionId = partitionId, ObjectId = objectId, ObjectValue = objectValue };
            try
            {
                var response = _client.writeAdvanced(request);
                //Console.WriteLine("Am i here?");
                switch (response.ResponseCase)
                {
                    case WriteAdvancedResponse.ResponseOneofCase.Timestamp:
                        Console.WriteLine("Advanced Write Successful");

                        _previousStorage.WriteToCache(partitionId, objectId, objectValue, response.Timestamp);
                        break;

                    case WriteAdvancedResponse.ResponseOneofCase.MasterServerUrl:
                        Console.WriteLine($"Advanced  Write - Changing to server {response.MasterServerUrl.ServerUrl}");
                        
                        _usedUrl = response.MasterServerUrl.ServerUrl;
                        _client = BuildClientFromServerUrl(response.MasterServerUrl.ServerUrl);
                        
                        WriteAdvanced(partitionId, objectId, objectValue);
                        break;

                    case WriteAdvancedResponse.ResponseOneofCase.None:
                        Console.WriteLine("Unexpected Error");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (RpcException)
            {
                ConnectToNewServer(partitionId);
                Write(partitionId, objectId, objectValue);
                return;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error Writing");

                Console.WriteLine(e.Message);
                Console.ReadLine();

            }
        }

        public string Read(string partitionId, string objectId, string serverId)
        {
            var response = "";
            if (UseBaseVersion)
            {
                response = ReadBasic(partitionId, objectId, serverId);
                Console.WriteLine("Base Read");
            }
            else
            {
                response = ReadAdvanced(partitionId, objectId, serverId);
                Console.WriteLine("Advanced Read");
            }
            return response;
        }

        private string ReadAdvanced(string partitionId, string objectId, string serverId)
        {
            var request = new ReadAdvancedRequest { PartitionId = partitionId, ObjectId = objectId, };
            try
            {
                var readResponse = _client.readAdvanced(request);
                if (!readResponse.ObjectValue.Equals(ObjectNotPresent)) {
                    return _previousStorage.GetAndUpdate(partitionId,objectId,readResponse);
                    //return readResponse.ObjectValue;
                }
                if (serverId.Equals("-1"))
                {
                    return _previousStorage.GetAndUpdate(partitionId, objectId);
                }
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var secondReadResponse = _client.readAdvanced(request);
                if (secondReadResponse.ObjectValue.Equals(ObjectNotPresent))
                {
                    return _previousStorage.GetAndUpdate(partitionId, objectId);
                }
                return _previousStorage.GetAndUpdate(partitionId, objectId, readResponse);
            }
            catch (RpcException e)
            {
                ConnectToNewServer(partitionId);
                
                Console.WriteLine("Error reading");
                Console.WriteLine(e.Message);

                return ReadAdvanced(partitionId, objectId, serverId);
            }
        }

        private string ReadBasic(string partitionId, string objectId, string serverId) {            

            var request = new ReadRequest{PartitionId = partitionId, ObjectId = objectId};
            try{
                var readResponse = _client.read(request);

                if (!readResponse.ObjectValue.Equals(ObjectNotPresent))
                    return readResponse.ObjectValue;

                if (serverId.Equals("-1")) return ObjectNotPresent;

                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);

                var secondReadResponse = _client.read(request);
                return secondReadResponse.ObjectValue.Equals(ObjectNotPresent)
                    ? ObjectNotPresent
                    : secondReadResponse.ObjectValue;
            }
            catch (RpcException e) 
            {
                ConnectToNewServer(partitionId);

                Console.WriteLine("Error reading");
                Console.WriteLine(e.Message);
                
                return Read(partitionId, objectId, serverId);
            }
        }


        public List<ListServerResult> ListServer(string serverId)
        {
            var request = new ListServerRequest();
            var serverUrl = MapServerIdToUrl(serverId);
            try
            {
                _client = BuildClientFromServerUrl(serverUrl);
                var listServerResponse = _client.listServer(request);
                return listServerResponse
                    .Objects
                    .Select(MapToListServerResult)
                    .ToList();
            }
            catch (RpcException e)
            {
                RemoveClientUrl(serverUrl);

                Console.WriteLine("Server is down");
                Console.WriteLine(e.Message);

                return new List<ListServerResult>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return new List<ListServerResult>();
            }
        }
        public List<ListServerResult> ListServerUrl(string serverUrl)
        {
            var request = new ListServerRequest();
            try
            {
                _client = BuildClientFromServerUrl("http://" + serverUrl);
                var listServerResponse = _client.listServer(request);
                return listServerResponse
                    .Objects
                    .Select(MapToListServerResult)
                    .ToList();
            }
            catch (RpcException e)
            {
                RemoveClientUrl(serverUrl);

                Console.WriteLine("Server is down");
                Console.WriteLine(e.Message);

                return new List<ListServerResult>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return new List<ListServerResult>();

            }
        }

        public Dictionary<string, List<ListServerResult>> ListGlobal()
        {
            Dictionary<string, List<ListServerResult>> result = new Dictionary<string, List<ListServerResult>> ();
            Console.WriteLine("I'm gonna try for each server in the serverlist");
            foreach (var serverUrl in _clientLogic.GetServerUrlList())
            {
                Console.WriteLine("Server: " + serverUrl);
                var res = ListServerUrl(serverUrl);
                if(res.Count > 0)
                {
                    Console.WriteLine("Success!");
                    result.Add(serverUrl, res);
                }
            }

            return result;
        }
    }
    public struct PartitionObject
    {
        public string partitionId;
        public string objectId;

        public PartitionObject(string partitionId, string objectId)
        {
            this.partitionId = partitionId;
            this.objectId = objectId;
        }

        public bool Equals(PartitionObject other)
        {
            return partitionId.Equals(partitionId) && objectId.Equals(other.objectId);
        }
    }


    public class PreviousStorage
    {
        private GrpcService _service;
        public PreviousStorage(GrpcService grpcService)
        {
            _service = grpcService;
        }

        private Dictionary<PartitionObject, ReadAdvancedResponse> _previousValues = new Dictionary<PartitionObject, ReadAdvancedResponse>();

        public void WriteToCache(string partitionId, string objectId, string objectValue, int timestamp)
        {
            ReadAdvancedResponse req = null;
            ReadAdvancedResponse r = new ReadAdvancedResponse { ObjectValue = objectValue, Timestamp = timestamp };
            PartitionObject obj = new PartitionObject(partitionId, objectId);
            if (!_previousValues.TryGetValue(obj, out req))
            {
                _previousValues.Add(obj, r);
            }
            else
            {
                _previousValues[obj] = r;

            }
        }

        public string GetAndUpdate(string partitionId, string objectId, ReadAdvancedResponse response = null)
        {
            Console.WriteLine("Get and update");

            ReadAdvancedResponse req = null;
            PartitionObject obj = new PartitionObject(partitionId, objectId);
            if (_previousValues.TryGetValue(obj, out req))
            {
                if (response == null) return req.ObjectValue;

                if (req.Timestamp <= response.Timestamp)
                {
                    _previousValues[obj] = response;
                    return response.ObjectValue;
                }
                return req.ObjectValue;
            }
            else
            {
                if (response == null) return _service.ObjectNotPresent;

                _previousValues.Add(obj, response);
                return response.ObjectValue;
            }
        }
    }
}