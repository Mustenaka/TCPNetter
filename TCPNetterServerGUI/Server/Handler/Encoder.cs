using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using System.Text;
using TCPNetterServerGUI.Server.Model;

namespace TCPNetterServerGUI.Server.Handler;


public class MessageDecoder : ByteToMessageDecoder
{
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        // Assuming messages are JSON strings terminated by newline
        //int index = input.IndexOf(0, input.ReaderIndex, input.ReadableBytes); // Find newline
        var index = input.IndexOf(input.ReaderIndex, input.WriterIndex, (byte)'\n');
        if (index < 0)
            return; // Not enough data yet

        var length = index - input.ReaderIndex;
        var jsonMessage = input.ReadSlice(length).ToString(Encoding.UTF8);
        input.SkipBytes(1); // Skip newline character

        try
        {
            var message = JsonConvert.DeserializeObject<MessageModel>(jsonMessage);
            if (message != null)
            {
                output.Add(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing message: {ex.Message}");
        }
    }
}

public class MessageEncoder : MessageToByteEncoder<object>
{
    //protected override void Encode(IChannelHandlerContext context, MessageModel message, IByteBuffer output)
    //{
    //    var jsonMessage = JsonConvert.SerializeObject(message);
    //    var jsonBytes = Encoding.UTF8.GetBytes(jsonMessage + "\n");
    //    output.WriteBytes(jsonBytes);
    //}

    protected override void Encode(IChannelHandlerContext context, object message, IByteBuffer output)
    {
        string jsonMessage;

        if (message is MessageModel singleMessage)
        {
            // 如果是单个MessageModel
            jsonMessage = JsonConvert.SerializeObject(singleMessage);
        }
        else if (message is List<MessageModel> messageList)
        {
            // 如果是MessageModel的列表
            jsonMessage = JsonConvert.SerializeObject(messageList);
        }
        else if (message is List<SaveModel> historyList)
        {
            // 如果是hsitoryList的列表
            jsonMessage = JsonConvert.SerializeObject(historyList);
        }
        else
        {
            throw new InvalidOperationException("Unsupported message type.");
        }

        // 将json字符串编码为UTF-8字节并写入输出
        var jsonBytes = Encoding.UTF8.GetBytes(jsonMessage + "\n");
        output.WriteBytes(jsonBytes);
    }
}