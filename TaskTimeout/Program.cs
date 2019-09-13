using System;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq.Expressions;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            int taskNo = 1;

            while (true)
            {
                /*CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;
                
                var task = Task.Run(() => RunMethod(taskNo, token));
                if (task.Wait(TimeSpan.FromSeconds(3)))
                {
                    string result = task.Result;
                    Console.WriteLine(result);
                    taskNo++;
                }
                else
                {
                    Console.WriteLine("Timeout out running task #" + taskNo);
                    tokenSource.Cancel();
                    taskNo = 1;
                }*/

                CancellationTokenSource tokenSource2 = new CancellationTokenSource();
                var resultText = RunTask(() => RunMethod(taskNo, tokenSource2.Token), tokenSource2, 3);

                if (string.IsNullOrEmpty(resultText))
                {
                    Console.WriteLine("Timeout out running task #" + taskNo);
                    taskNo = 1;
                }
                else
                {
                    Console.WriteLine(resultText);
                    taskNo++;
                }
            }
        }
        catch (Exception ex)
        {
            string error = GetExceptionMessages(ex);
            Console.WriteLine(error);
        }
    }

    private static T RunTask<T>(Func<T> function, CancellationTokenSource tokenSource, int timeoutInSecs = 0)
    {
        timeoutInSecs = timeoutInSecs == 0 ? 10 : timeoutInSecs;
        //CancellationTokenSource tokenSource = new CancellationTokenSource();
        //CancellationToken token = tokenSource.Token;

        //var task = Task.Run(function, token);
        Task<T> task = Task.Run(function);
        if (task.Wait(TimeSpan.FromSeconds(timeoutInSecs)))
        {
            return task.Result;
        }
        else
        {
            tokenSource.Cancel();
            return default(T);
        }
    }

    private static string RunMethod(int taskNo, CancellationToken token)
    {
        int sec = 1100;
        int sleepingTime = taskNo * sec;

        try
        {
            for (int i = 0; i < taskNo; i++)
            {
                Thread.Sleep(sec);
                ProcessTaskNo(taskNo, token);
            }
            
            return "Task #" + taskNo + " is done.";
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Killing task #" + taskNo);
            return string.Empty;
        }
    }

    private static void ProcessTaskNo(int taskNo, CancellationToken token)
    {
        CheckCancellationRequest(token);

        Console.WriteLine("Task #" + taskNo + " is processing.");
    }

    private static void CheckCancellationRequest(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
    }

    private static string GetExceptionMessages(Exception exception, int exceptionNo = 1)
    {
        try
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Exception #" + exceptionNo + " => " + exception.Message + "\r\n");

            for (int i = 1; i < exceptionNo; i++)
                msg.Append("\t");
            msg.Append("Stacktrace #" + exceptionNo + " => " + exception.StackTrace.Replace(Environment.NewLine, "\t"));

            if (exception.InnerException != null)
            {
                msg.Append("\r\n");
                for (int i = 0; i < exceptionNo; i++)
                    msg.Append("\t");

                msg.Append("Inner ");
                msg.Append(GetExceptionMessages(exception.InnerException, exceptionNo + 1));
            }

            return msg.ToString();
        }
        catch (Exception)
        {
            return "Failed getting exception messages.";
        }
    }
}