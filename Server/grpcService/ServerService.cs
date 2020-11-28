using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Server.utils;
using ServerDomain;

namespace DIDA_GSTORE.ServerService{
    public class ServerService : DIDAService.DIDAServiceBase{
        private readonly IStorage _storage;
        private readonly FreezeUtilities _freezeUtilities;
        private string _myUrl;
        private Action delayFunction;
        public ServerService(IStorage storage, FreezeUtilities freezeUtilities, string myUrl, Action delayFunction)
        {
            _storage = storage;
            _freezeUtilities = freezeUtilities;
            _myUrl = myUrl;
            this.delayFunction = delayFunction;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context){
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(Write(request));
        }

        public WriteResponse Write(WriteRequest request){
            Console.WriteLine("Received Write request. Id - " +
                request.ObjectId + ". Value" + request.ObjectValue);
            try
            {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var objectValue = request.ObjectValue;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster){
                    Console.WriteLine("I'm the master of this partition");
                    _storage.WriteMaster(partitionId, objectId, objectValue, -1);
                    return new WriteResponse{ResponseMessage = "OK"};
                }
                Console.WriteLine("I'm not the master of this partition");

                var url = _storage.GetMasterUrl(request.PartitionId);
                return new WriteResponse{MasterServerUrl = new ServerUrlResponse{ServerUrl = url}};
            }
            catch (Exception e) 
            {
                Console.WriteLine("I don't have the partition");

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("End of exception");
                return new WriteResponse();
            }
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context)
        {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(Read(request));
        }
        public override Task<ReadAdvancedResponse> readAdvanced(ReadAdvancedRequest request, ServerCallContext context)
        {
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            return Task.FromResult(ReadAdvanced(request));
        }


        public ReadResponse Read(ReadRequest request)
        {
            Console.WriteLine("Received Read Request: " + request.ToString());
            try
            {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var partition = _storage.GetPartitionOrThrowException(partitionId);
                var objectValue = _storage.Read(partitionId, objectId);
                var response = new ReadResponse { ObjectValue = objectValue };
                return response;
            }
            catch (Exception)
            {
                /*Partition not founded */
                return new ReadResponse { ObjectValue = "N/A" };
            }
        }

        public ReadAdvancedResponse ReadAdvanced(ReadAdvancedRequest request)
        {
            Console.WriteLine("Received Read Request: " + request.ToString());
            try
            {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var curobjectValue = request.CurObjectValue;
                var objectTimestamp = request.CurTimestamp;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                var objectValue = _storage.ReadAdvanced(partitionId, objectId, curobjectValue, objectTimestamp);

                var response = new ReadAdvancedResponse
                {
                    ObjectValue = objectValue.value,
                    Timestamp = objectValue.timestampCounter
                };

                return response;
            }
            catch (Exception)
            {
                /*Partition not founded */
                return new ReadAdvancedResponse { ObjectValue = "N/A" };
            }
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context){
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            Console.WriteLine("Received List Server Request: " + request.ToString());
            return Task.FromResult(_storage.ListServer());
        }

        public override Task<ListGlobalResponse> listGlobal(ListGlobalRequest request, ServerCallContext context){
            _freezeUtilities.WaitForUnfreeze();
            delayFunction();
            Console.WriteLine("Received List Global Request ");

            List<ListGlobalResponseEntity> listGlobalResponseEntities = new List<ListGlobalResponseEntity>();
            foreach(var partitionMaster in _storage.GetPartitionMasters())
            {
                if(partitionMaster.masterUrl.Equals(_myUrl))
                {
                    var response2 = _storage.ListPartition(partitionMaster.partitionId);
                    listGlobalResponseEntities.Add(new ListGlobalResponseEntity
                    {
                        ObjectIds = { response2.ObjectIds.ToList() },
                        PartitionId = partitionMaster.partitionId,
                    });
                    continue;
                }
                try { 
                //Console.WriteLine(partitionMaster.ToString());
                //result += "{ partition: " + partitionMaster.partitionId + ", [ ";
                ClientService grpcService = new ClientService(partitionMaster.masterUrl);
                var response = grpcService.ListPartitionGlobal(partitionMaster.partitionId);

                listGlobalResponseEntities.Add(new ListGlobalResponseEntity
                {
                    ObjectIds = { response.ObjectIds.ToList() }, 
                    PartitionId = partitionMaster.partitionId,
                });
                    //result += " ]}, ";
                }catch(Exception)
                {
                    Console.WriteLine($"Failed to fetch partition {partitionMaster.partitionId} from partition master {partitionMaster.masterUrl}");
                }
            }
            //Console.WriteLine(listGlobalResponseEntities.Count);
            return Task.FromResult(new ListGlobalResponse { Objects = { listGlobalResponseEntities } });
        }

        public override Task<ListPartitionGlobalResponse> listPartitionGlobal(ListPartitionGlobalRequest request, ServerCallContext context)
        {
            _freezeUtilities.WaitForUnfreeze();
            Console.WriteLine("List PARTITION Global Request: " + request.ToString());
            return Task.FromResult(_storage.ListPartition(request.PartitionId));
        }

        public override Task<ServerUrlResponse> getServerUrl(ServerUrlRequest request, ServerCallContext context){
            _freezeUtilities.WaitForUnfreeze();
            string serverId = request.ServerId;
            string serverUrl = _storage.GetServerOrThrowException(serverId);

            return Task.FromResult(new ServerUrlResponse { ServerUrl = serverUrl });

        }
    }
}