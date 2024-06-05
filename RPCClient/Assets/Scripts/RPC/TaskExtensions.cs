using System;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        // 自定义扩展方法示例：为任务添加重试功能
        public static async Task WithRetry(this Func<Task> func, int retryCount, TimeSpan delay)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    // 调用传入的异步函数
                    await func();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"尝试 {i + 1}/{retryCount} 失败: {ex.Message}");

                    // 如果不是最后一次尝试，则等待一段时间后进行重试
                    if (i < retryCount - 1)
                        await Task.Delay(delay);
                }
            }

            // 如果所有重试都失败，则抛出最后一次尝试的异常
            throw new InvalidOperationException("所有重试尝试均失败");
        }
    }
}