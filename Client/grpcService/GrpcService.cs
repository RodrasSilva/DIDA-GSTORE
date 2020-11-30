using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using Client.model;
using Grpc.Core;
using Grpc.Net.Client;

namespace DIDA_GSTORE.grpcService{
    public class GrpcService{
        private const string ObjectNotPresent = "N/A";
        private DIDAService.DIDAServiceClient _client;
        private ClientLogic _clientLogic;
        private string _usedUrl;

        public GrpcService(string serverIp, int serverPort, ClientLogic clientLogic){
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            _usedUrl = $"http://{serverIp}:{serverPort}";
            _client = BuildClientFromServerUrl(_usedUrl);
            _clientLogic = clientLogic;
        }

        private void ConnectToNewServer(string partitionId)
        {
            //_clientLogic.ServerList[partitionId].Remove(_usedUrl);
            _client = null;
            var aux = _usedUrl.Split("//");
            string url = aux[1];
            foreach (var parId in _clientLogic.ServerList.Keys)
            {
                foreach (var thing in _clientLogic.ServerList[parId])
                {
                    Console.WriteLine(thing);
                }

                _clientLogic.ServerList[parId].Remove(url);
                foreach(var thing in _clientLogic.ServerList[parId])
                {
                    Console.WriteLine(thing);
                }
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

        private void ConnectToNewServer()
        {
            //_clientLogic.ServerList[partitionId].Remove(_usedUrl);
            _client = null;
            RemoveClientUrl(_usedUrl);

            //if (_clientLogic.ServerList[partitionId].Count == 0)
            //    throw new Exception("should not happen");

            Random r = new Random();
            foreach(var parId in _clientLogic.ServerList.Keys)
            {
                if (_clientLogic.ServerList[parId].Count == 0) continue;

                _usedUrl = "http://" + _clientLogic.ServerList[parId][
                    r.Next(0, _clientLogic.ServerList[parId].Count)];
                _client = BuildClientFromServerUrl(_usedUrl);
                return;
            }
            throw new Exception("should not happen");
        }

        private static DIDAService.DIDAServiceClient BuildClientFromServerUrl(string serverUrl){
            var channel = GrpcChannel.ForAddress(serverUrl);
            return new DIDAService.DIDAServiceClient(channel);
        }

        private string MapServerIdToUrl(string serverId){
            var request = new ServerUrlRequest{ServerId = serverId};
            return _client.getServerUrl(request).ServerUrl;
        }

        private static ListServerResult MapToListServerResult(ListServerResponseEntity it){
            return new ListServerResult(it.ObjectId, it.ObjectValue, it.IsMaster);
        }
        /*
        private static ListGlobalResult mapToListGlobalResult(ListGlobalResponse it){
            return new ListGlobalResult(it.Objects.Select(mapToListGlobalResultIdentifier).ToList());
        }

        private static ListGlobalResultIdentifier mapToListGlobalResultIdentifier(ObjectIdentifier it){
            return new ListGlobalResultIdentifier(it.PartitionId, it.ObjectId);
        }
        */
        public void Write(string partitionId, string objectId, string objectValue){
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
                        Write(partitionId, objectId, objectValue);
                        break;
                    case WriteResponse.ResponseOneofCase.None:
                        Console.WriteLine("TO DO CASE");
                        break; //  TODO : Check how to handle when none of the above are returned  
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (RpcException e){
                ConnectToNewServer(partitionId);
                Write(partitionId, objectId, objectValue);
                return;
                Console.WriteLine("Error Writing");

                Console.WriteLine(e.Message);
                //Console.ReadLine();
                //throw; //  TODO : Check how to handle connection exceptions 
            } catch(Exception e)
            {
                Console.WriteLine("Error Writing");

                Console.WriteLine(e.Message);
                Console.ReadLine();

            }
        }
        public string ReadAdvanced(string partitionId, string objectId, string serverId)
        {
            var request = new ReadAdvancedRequest { PartitionId = partitionId, ObjectId = objectId,
                CurObjectValue = "NA(CLIENT)", CurTimestamp = -1 };
            try
            {
                var readResponse = _client.readAdvanced(request);
                if (!readResponse.ObjectValue.Equals(ObjectNotPresent))
                    return readResponse.ObjectValue;
                if (serverId.Equals("-1")) return ObjectNotPresent;
                var serverUrl = MapServerIdToUrl(serverId);
                _client = BuildClientFromServerUrl(serverUrl);
                var secondReadResponse = _client.readAdvanced(request);
                return secondReadResponse.ObjectValue.Equals(ObjectNotPresent)
                    ? ObjectNotPresent
                    : secondReadResponse.ObjectValue;
            }
            catch (RpcException e)
            {
                ConnectToNewServer(partitionId);
                
                Console.WriteLine("Error reading");
                Console.WriteLine(e.Message);

                //Console.ReadLine();
                
                return ReadAdvanced(partitionId, objectId, serverId);
                //return "Error";
                //throw; //  TODO : Check how to handle connection exceptions 
            }
        }

        public string Read(string partitionId, string objectId, string serverId) {
            var advanced = true;
            if(advanced)
            {
                return ReadAdvanced(partitionId, objectId, serverId);
            }
            
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
                
                //Console.ReadLine();

                return Read(partitionId, objectId, serverId);
                //throw; //  TODO : Check how to handle connection exceptions 
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

                //ConnectToNewServer();
                RemoveClientUrl(serverUrl);
                //_clientLogic.ServerList.Remove(serverId);

                Console.WriteLine("Server is down");
                Console.WriteLine(e.Message);
                //Console.ReadLine();

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

                //ConnectToNewServer();
                RemoveClientUrl(serverUrl);
                //_clientLogic.ServerList.Remove(serverUrl);

                Console.WriteLine("Server is down");
                Console.WriteLine(e.Message);
                //Console.ReadLine();

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
            var request = new ListGlobalRequest();
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
}