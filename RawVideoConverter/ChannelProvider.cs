using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RawVideoConverter
{
    class ChannelProvider
    {

        //任意の時間にchannelに書き込むproducerは1つしかないことを設定し、
        //同時書き込みの制御を利用しないことで、負荷を減らす。
        private Channel<string> channel = Channel.CreateUnbounded<string>
            (
                new UnboundedChannelOptions
                {
                    SingleWriter = true
                }
             );




        //channelのwriterを返す
        public ChannelWriter<string> getWriter()
        {
            return channel.Writer;
        }

        //consumerのobject宣言
        private List<Consumer> consumers;
        private List<Task> consumerTasks;

        //2つのconsumerを起動する。
        public void startConsumers(string outputDir, int consumerCount)
        {
            //Console.WriteLine("Channelの中身はstring");
            consumers = new List<Consumer>(consumerCount);
            consumerTasks = new List<Task>(consumerCount);
            for (int i = 0; i < consumerCount; i++)
            {
                consumers.Add(new Consumer(channel.Reader));//Create a consumer

                //For Console program:
                //This works in Console program, but freezes UI in a UI program.
                //consumerTasks.Add(consumers[i].ConsumeData(destFolder));//Begin consuming content from the Channel

                //For UI program(Windows Form, WPF):
                int index = i;
                Task copyTask = Task.Run(() => consumers[index].ConsumeData(outputDir));
                //Why using Task.Run() here can avoid freezing UI 
                //Reference: https://stackoverflow.com/questions/69565851/using-await-task-run-somemethodasync-vs-await-somemethodasync-in-a-ui
                consumerTasks.Add(copyTask);
            }
        }



        internal class Consumer
        {
            private readonly ChannelReader<string> _reader;

            private readonly string _identifier;
            //private readonly int _delay;

            public Consumer(ChannelReader<string> reader)
            {
                _reader = reader;
               

            }

            public async Task ConsumeData(string outputDir)
            {
                
               
                while (await _reader.WaitToReadAsync()) //ここでChannel内に内容が追加されるのを待っている
                {
                    string channelItem = "";

                    //Consume the data in the queue
                    if (_reader.TryRead(out channelItem))
                    {

                        //Process the data received from the queue---------------
                       
                        Converter.convert2MP4(File.GetLastWriteTime(channelItem), Path.GetDirectoryName(channelItem), channelItem,outputDir);

                          
                        
                       

                        //Process the data received from the queue---------------
                    }
                }

                //Channelがcompleteとマークされたら、WaitToReadAsyncはfalseを返し、ここにくる。
                //Console.WriteLine($"CONSUMER ({_identifier}): Completed");

            }
        }




        internal class Producer
        {
            private readonly ChannelWriter<string> writer;
            


            public Producer(ChannelWriter<string> _writer)
            {
                writer = _writer;
              

            }

            public void writeData(string data)
            {
                //Synchronously write to the channel
                if (writer.TryWrite(data))
                {
                    //If write to the channel successfully, do the following

                    ;
                    //string msg = $"PRODUCER ({identifier}) ThreadID ({Thread.CurrentThread.ManagedThreadId}): Wrote {Path.GetFileName(data)} to the channel";
                    //string msg = $"カメラ ({identifier}): Wrote {Path.GetFileName(data)} to the channel";

                    //Console.WriteLine(msg);
                }
            }


            public async void writeDataAsync(string data)
            {
                await writer.WriteAsync(data);


                
            }
        }
    }
}
