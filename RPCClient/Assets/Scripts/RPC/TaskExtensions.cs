using System;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        // �Զ�����չ����ʾ����Ϊ����������Թ���
        public static async Task WithRetry(this Func<Task> func, int retryCount, TimeSpan delay)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    // ���ô�����첽����
                    await func();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"���� {i + 1}/{retryCount} ʧ��: {ex.Message}");

                    // ����������һ�γ��ԣ���ȴ�һ��ʱ����������
                    if (i < retryCount - 1)
                        await Task.Delay(delay);
                }
            }

            // ����������Զ�ʧ�ܣ����׳����һ�γ��Ե��쳣
            throw new InvalidOperationException("�������Գ��Ծ�ʧ��");
        }
    }
}