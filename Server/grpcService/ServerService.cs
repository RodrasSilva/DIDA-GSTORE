using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace DIDA_GSTORE.ServerService{
    public class ServerService : DIDAService.DIDAServiceBase{
        private readonly IStorage _storage;

        public ServerService(IStorage storage){
            _storage = storage;
        }


        public override Task<WriteResponse> write(WriteRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(Write(request));
        }

        public WriteResponse Write(WriteRequest request){
            ServerDomain.Server.DelayMessage();
            try
            {
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var objectValue = request.ObjectValue;

                var partition = _storage.GetPartitionOrThrowException(partitionId);
                if (partition.IsMaster){
                    _storage.WriteMaster(partitionId, objectId, objectValue, -1);
                    return new WriteResponse{ResponseMessage = "OK"};
                }

                var url = _storage.GetMasterUrl(request.PartitionId);
                return new WriteResponse{MasterServerUrl = new ServerUrlResponse{ServerUrl = url}};
            }
            catch (Exception e) //later to be named NotFound or smth
            {
                Console.WriteLine(e.Message);
                return new WriteResponse();
            }
        }

        public override Task<ReadResponse> read(ReadRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(Read(request));
        }


        public ReadResponse Read(ReadRequest request){
            ServerDomain.Server.DelayMessage();
            try{
                var partitionId = request.PartitionId;
                var objectId = request.ObjectId;
                var partition = _storage.GetPartitionOrThrowException(partitionId);
                var objectValue = _storage.Read(partitionId, objectId);
                var response = new ReadResponse{ObjectValue = objectValue};
                return response;
            }
            catch (Exception){
                /*Partition not founded */
                return new ReadResponse{ObjectValue = "-1"};
            }
        }

        public override Task<ListServerResponse> listServer(ListServerRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(_storage.ListServer());
        }

        public override Task<ListGlobalResponse> listGlobal(ListGlobalRequest request, ServerCallContext context){
            ServerDomain.Server.DelayMessage();
            return Task.FromResult(_storage.ListGlobal());
        }

        public override Task<ServerUrlResponse> getServerUrl(ServerUrlRequest request, ServerCallContext context){
            return base.getServerUrl(request, context); // TODO : Implement
        }
    }
}